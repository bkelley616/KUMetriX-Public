namespace KumetrixServer.Handlers

open Giraffe
open Giraffe.HttpStatusCodeHandlers.ServerErrors
open Giraffe.HttpStatusCodeHandlers.RequestErrors
open Giraffe.HttpStatusCodeHandlers.Successful
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http

open Shared.Types
open Shared.Validators
open KumetrixServer.DataAccess
open KumetrixServer.Helpers


module Auth =

    let signupCodes =
        [ config.["CSUSIGNUPCODE"]
          config.["PROGSIGNUPCODE"]
          config.["GENERICSIGNUPCODE"] ]

    let signup =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<SignupPayload> ctx

                match payload with
                | Some request ->

                    match (signupCodes
                           |> List.exists ((=) request.SignUpCode)) with
                    | false -> return! (BAD_REQUEST "Error: Invalid Sign Up Code") next ctx
                    | true ->

                        match validateSignUp request.Username request.Password request.Email with
                        | Ok () ->
                            let! addedAccount = createNewUserAccount request.Username request.Password request.Email

                            match addedAccount with
                            | Ok msg -> return! (CREATED msg) next ctx
                            | Error err -> return! (INTERNAL_ERROR err) next ctx
                        | Error err -> return! (BAD_REQUEST err) next ctx

                | None -> return! (BAD_REQUEST "Missing Signup Request Info") next ctx
            }

    let login =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<LoginPayload> ctx

                match payload with
                | Some request ->
                    let! login = loginUser request.Username request.Password

                    match login with
                    | Ok session -> return! json session next ctx
                    | Error err -> return! (FORBIDDEN err) next ctx
                | None -> return! (BAD_REQUEST "Missing Login Request Info") next ctx
            }


    let logout =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<RequestPayload> ctx

                match payload with
                | Some request ->
                    let! logout = logoutUser request.Username request.SessionId

                    match logout with
                    | Ok msg -> return! json msg next ctx
                    | Error err -> return! (FORBIDDEN err) next ctx
                | None -> return! (BAD_REQUEST "Missing Logout Request Info") next ctx
            }

module Api =
    let getTickerData ticker =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<RequestPayload> ctx

                match payload with
                | Some request ->
                    let! getData = getStockData request.Username request.SessionId ticker

                    match getData with
                    | Ok data -> return! json data next ctx
                    | Error err -> return! (FORBIDDEN err) next ctx
                | None -> return! (BAD_REQUEST "Missing Stock Data Request Info") next ctx
            }

    let getTickerDataSummary ticker =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<RequestPayload> ctx

                match payload with
                | Some request ->
                    let! getData = getStockDataSummary request.Username request.SessionId ticker

                    match getData with
                    | Ok data -> return! json data next ctx
                    | Error err -> return! (FORBIDDEN err) next ctx
                | None -> return! (BAD_REQUEST "Missing Stock Data Summary Request Info") next ctx
            }

    let addPinnedTicker =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<PinnedRequestPayload> ctx

                match payload with
                | Some request ->
                    let! addPin = addUserPinnedTicker request.Username request.SessionId request.Ticker

                    match addPin with
                    | Ok response -> return! json response next ctx
                    | Error err -> return! (FORBIDDEN err) next ctx
                | None -> return! (BAD_REQUEST "Missing Add Pin Request Info") next ctx
            }

    let removeUserPinnedTicker =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = tryBindJsonAsync<PinnedRequestPayload> ctx

                match payload with
                | Some request ->
                    let! addPin = removeUserPinnedTickerUser request.Username request.SessionId request.Ticker

                    match addPin with
                    | Ok response -> return! json response next ctx
                    | Error err -> return! (FORBIDDEN err) next ctx
                | None -> return! (BAD_REQUEST "Missing Remove Pin Request Info") next ctx
            }
