namespace KumetrixServer

open MongoDB.Bson
open MongoDB.Driver
open Konscious.Security.Cryptography
open FSharp.Data
open System
open Newtonsoft.Json.Linq

open KumetrixServer.Helpers
open Shared.Types

module DataAccess =


    let inactiveSessionId = config.["INACTIVESESSIONID"]
    let inactiveSessionExpiration = config.["INACTIVESESSIONEXPIRATION"]
    let ConnectionString = config.["DBCONNECTIONSTRING"]
    let dbName = config.["DBNAME"]
    let collectionName = config.["COLLECTIONNAME"]
    let prefixSalt = config.["PREFIXSALT"]
    let suffixSalt = config.["SUFFIXSALT"]
    let externalStockApiKey = config.["EXTERNALSTOCKAPIKEY"]
    let stockApiBaseUrl = config.["STOCKAPIBASEURL"]

    type MarketStackResponse = JsonProvider<""" { 
        "open": 129.8,
        "high": 133.04,
        "low": 129.47,
        "close": 132.995,
        "volume": 106686703.0,
        "adj_high": 133.04,
        "adj_low": 129.47,
        "adj_close": 132.995,
        "adj_open": 129.8,
        "adj_volume": 106686703.0,
        "split_factor": 1.0,
        "symbol": "AAPL",
        "exchange": "XNAS",
        "date": "2021-04-09T00:00:00+0000" } """>


    let client = MongoClient(ConnectionString)
    let db = client.GetDatabase(dbName)
    let accountCollection = db.GetCollection<Account> collectionName



    let private generateHash (username: string) (password: string) =
        async {
            use argon2id =
                new Argon2id(Text.Encoding.ASCII.GetBytes(password))

            argon2id.Iterations <- 4
            argon2id.Salt <- Text.Encoding.ASCII.GetBytes($"{prefixSalt}{username}{suffixSalt}")
            argon2id.DegreeOfParallelism <- 8
            argon2id.MemorySize <- 1024 * 1024

            let! hash = argon2id.GetBytesAsync(16) |> Async.AwaitTask
            return hash |> Convert.ToBase64String
        }



    let createNewUserAccount username password email =
        async {
            let id = BsonObjectId(ObjectId.GenerateNewId())
            let! passwordHash = generateHash username password

            let newAccount =
                { _id = id
                  Username = username
                  Password = passwordHash
                  Email = email
                  SessionId = inactiveSessionId
                  SessionExpiration = inactiveSessionExpiration
                  Pinned = List.empty }

            try
                do!
                    accountCollection.InsertOneAsync(newAccount)
                    |> Async.AwaitTask

                return Ok "Sucessfully Created Account! Please Login!"
            with
            | err when err.Message.Contains "E11000" -> return Error "Error: Username Already Taken"
            | _ when true -> return Error "Error: Failed to Create Account"
        }


    let loginUser username password =
        async {
            try
                let! accountInfo =
                    accountCollection
                        .Find(fun acct -> acct.Username = username)
                        .Limit(1)
                        .SingleAsync()
                    |> Async.AwaitTask

                let! enteredPasswordHash = generateHash username password

                match String.Equals(accountInfo.Password, enteredPasswordHash) with
                | true ->
                    let newSessionId = Guid.NewGuid().ToString()

                    let newSessionExpiration =
                        DateTime.UtcNow.AddHours(1.0).ToString("o")

                    let accountUsername =
                        Builders<Account>.Filter.Eq ((fun acct -> acct.Username), username)

                    let updateSession =
                        Builders<Account>
                            .Update.Set((fun acct -> acct.SessionId), newSessionId)
                            .Set((fun acct -> acct.SessionExpiration), newSessionExpiration)

                    try
                        do!
                            accountCollection.UpdateOneAsync(accountUsername, updateSession)
                            |> Async.AwaitTask
                            |> Async.Ignore

                        return
                            Ok
                                { Username = accountInfo.Username
                                  Pinned = accountInfo.Pinned |> List.ofSeq
                                  SessionId = newSessionId
                                  SessionExpiration = newSessionExpiration
                                   }
                    with err -> return Error err.Message

                | false -> return Error "Error: Incorrect Password"

            with
            | err when err.Message.Contains "Sequence contains no elements" ->
                return Error "Error: Account Doesn't Exist"
            | err when true -> return Error err.Message
        }


    let logoutUser username sessionId =
        async {
            try
                let! accountInfo =
                    accountCollection
                        .Find(fun acct -> acct.Username = username)
                        .Limit(1)
                        .SingleAsync()
                    |> Async.AwaitTask

                match String.Equals(accountInfo.SessionId, sessionId) with
                | true ->
                    let accountUsername =
                        Builders<Account>.Filter.Eq ((fun acct -> acct.Username), username)

                    let updateSession =
                        Builders<Account>
                            .Update.Set((fun acct -> acct.SessionId), inactiveSessionId)
                            .Set((fun acct -> acct.SessionExpiration), inactiveSessionExpiration)

                    try
                        do!
                            accountCollection.UpdateOneAsync(accountUsername, updateSession)
                            |> Async.AwaitTask
                            |> Async.Ignore

                        return Ok "Logged Out Sucessfully!"
                    with err -> return Error err.Message

                | false -> return Error "Error: SessionId does not match"

            with err -> return Error err.Message
        }


    let addUserPinnedTicker username sessionId ticker =
        async {
            try
                let! accountInfo =
                    accountCollection
                        .Find(fun acct -> acct.Username = username)
                        .Limit(1)
                        .SingleAsync()
                    |> Async.AwaitTask

                match String.Equals(accountInfo.SessionId, sessionId)
                      && (DateTime.UtcNow.ToString("o") < accountInfo.SessionExpiration) with
                | true ->
                    match accountInfo.Pinned |> Seq.length >= 10 with
                    | true -> return Error "Error: Maximum number of pins reached"
                    | false ->
                        match accountInfo.Pinned |> Seq.exists ((=) ticker) with
                        | false ->
                            let accountUsername =
                                Builders<Account>.Filter.Eq ((fun acct -> acct.Username), username)

                            let newPinned =
                                accountInfo.Pinned |> Seq.append [ ticker ]

                            let updatePinnedTickers =
                                Builders<Account>.Update.Set ((fun acct -> acct.Pinned), newPinned)

                            try
                                do!
                                    accountCollection.UpdateOneAsync(accountUsername, updatePinnedTickers)
                                    |> Async.AwaitTask
                                    |> Async.Ignore

                                return Ok { Pinned = newPinned |> List.ofSeq }
                            with err -> return Error $"Error: Failled to add %s{ticker} to Pinned"
                        | true -> return Error $"Error: Already had %s{ticker} Pinned"

                | false -> return Error "Error: SessionId does not match or Expired"

            with
            | err when err.Message.Contains "Sequence contains no elements" ->
                return Error "Error: Account Doesn't Exist"
            | err when true -> return Error err.Message
        }


    let removeUserPinnedTickerUser username sessionId ticker =
        async {
            try
                let! accountInfo =
                    accountCollection
                        .Find(fun acct -> acct.Username = username)
                        .Limit(1)
                        .SingleAsync()
                    |> Async.AwaitTask

                match String.Equals(accountInfo.SessionId, sessionId)
                      && (DateTime.UtcNow.ToString("o") < accountInfo.SessionExpiration) with
                | true ->


                    let accountUsername =
                        Builders<Account>.Filter.Eq ((fun acct -> acct.Username), username)

                    let newPinned =
                        accountInfo.Pinned |> Seq.filter ((<>) ticker)

                    let updatePinnedTickers =
                        Builders<Account>.Update.Set ((fun acct -> acct.Pinned), newPinned)

                    try
                        do!
                            accountCollection.UpdateOneAsync(accountUsername, updatePinnedTickers)
                            |> Async.AwaitTask
                            |> Async.Ignore

                        return Ok { Pinned = newPinned |> List.ofSeq}
                    with err -> return Error $"Error: Failled to remove %s{ticker} from Pinned"

                | false -> return Error "Error: SessionId does not match or Expired"

            with
            | err when err.Message.Contains "Sequence contains no elements" ->
                return Error "Error: Account Doesn't Exist"
            | err when true -> return Error err.Message
        }

    

    let getStockData username sessionId ticker =
        async {
            try
                let! accountInfo =
                    accountCollection
                        .Find(fun acct -> acct.Username = username)
                        .Limit(1)
                        .SingleAsync()
                    |> Async.AwaitTask

                match String.Equals(accountInfo.SessionId, sessionId)
                      && (DateTime.UtcNow.ToString("o") < accountInfo.SessionExpiration) with
                | true ->

                    try
                        let! apiResponse =
                            Http.AsyncRequestString(
                                stockApiBaseUrl,
                                httpMethod = "GET",
                                query =
                                    [ "access_key", externalStockApiKey
                                      "symbols", ticker
                                      "limit", "1000"],
                                headers = [ "Accept", "application/json" ]
                            )


                        let jsonResponse = JObject.Parse apiResponse
                        let data = jsonResponse.SelectToken "data" |> string
                        let dataArray = MarketStackResponse.ParseList(data) 
                        let closePrices, dates = dataArray |> Array.map (fun d -> d.Close |> float, d.Date.DateTime ) |> Array.unzip
                       

                        return Ok {ClosePrices = closePrices; Dates = dates}
                    with err -> return Error $"Error: Unable to retrieve data for {ticker}"

                | false -> return Error "Error: SessionId does not match or Expired"

            with
            | err when err.Message.Contains "Sequence contains no elements" ->
                return Error "Error: Account Doesn't Exist"
            | err when true -> return Error err.Message

        }

    let getStockDataSummary username sessionId ticker =
        async {
            try
                let! accountInfo =
                    accountCollection
                        .Find(fun acct -> acct.Username = username)
                        .Limit(1)
                        .SingleAsync()
                    |> Async.AwaitTask

                match String.Equals(accountInfo.SessionId, sessionId)
                      && (DateTime.UtcNow.ToString("o") < accountInfo.SessionExpiration) with
                | true ->

                    try
                        let! apiResponse =
                            Http.AsyncRequestString(
                                stockApiBaseUrl,
                                httpMethod = "GET",
                                query =
                                    [ "access_key", externalStockApiKey
                                      "symbols", ticker
                                      "limit", "1" ],
                                headers = [ "Accept", "application/json" ]
                            )

                        let jsonResponse = JObject.Parse apiResponse
                        let summaryOpen = jsonResponse.SelectToken "data[0].open" |> string
                        let summaryClose = jsonResponse.SelectToken "data[0].close" |> string
                        let summary = { Open = summaryOpen |> float; Close = summaryClose |> float;}
                        return Ok summary
                    with err -> return Error $"Error: Unable to retrieve data for {ticker}"

                | false -> return Error "Error: SessionId does not match or Expired"

            with
            | err when err.Message.Contains "Sequence contains no elements" ->
                return Error "Error: Account Doesn't Exist"
            | err when true -> return Error err.Message

        }
