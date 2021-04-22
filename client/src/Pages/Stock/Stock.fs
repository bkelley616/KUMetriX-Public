module Stock


open System
open Elmish
open Feliz
open Feliz.Router
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Thoth.Json
open System.Text.RegularExpressions

open Shared.Types
open HelpersUtil.TypedCSS
open HelpersUtil.Images

importAll "./Styles.scss"


type PinAction =
  | RemovePin of string
  | AddPin of string

type State = { 
  Ticker: string
  IsPinAdded: bool
  PinIsLoading: bool
  User: AccountSession
  GraphClosePrices: option<float array>
  GraphDates: option<DateTime array>
   }

type Msg = 
  | LoadStockGraphData
  | LoadedStockGraphData of Result<StockDataResponse, string>
  | StartPinAction of PinAction
  | PinActionResult of Result<PinnedResponsePayload, string>

let startPinAction (username: string) (sessionId: string) (pinAction: PinAction)  =
    async {
      
      let path, ticker = 
        match pinAction with
        | AddPin ticker -> "api/pinned/add", ticker
        | RemovePin ticker -> "api/pinned/remove", ticker

      let requestBody = { Username = username; SessionId = sessionId; Ticker = ticker}

      let! response =
        Http.request (path) 
        |> Http.method POST
        |> Http.content (BodyContent.Text(Encode.Auto.toString(4,requestBody)))
        |> Http.header (Headers.contentType "application/json")
        |> Http.header (Headers.authorization "kumetrix")
        |> Http.send
        
      if response.statusCode = 200 then
        let tryDecodePinAction = Decode.Auto.fromString<PinnedResponsePayload>(response.responseText)
        match tryDecodePinAction with
        | Ok updatedPins -> return Ok (updatedPins)
        | Error msg -> return Error( (msg.Replace ("\"", "")))
      else
        return Error( (response.responseText.Replace ("\"", "")))
}

let loadGraphData (username: string) (sessionId: string) (ticker: String)  =
  async {
    
    let path = sprintf "api/data/%s" ticker
    let requestBody = { Username = username; SessionId = sessionId; Ticker = ticker}

    let! response =
      Http.request (path) 
      |> Http.method POST
      |> Http.content (BodyContent.Text(Encode.Auto.toString(4,requestBody)))
      |> Http.header (Headers.contentType "application/json")
      |> Http.header (Headers.authorization "kumetrix")
      |> Http.send

    if response.statusCode = 200 then
      let tryDecodeGraphDataResponse = Decode.Auto.fromString<StockDataResponse>(response.responseText)
      match tryDecodeGraphDataResponse with
      | Ok graphData -> return Ok (graphData)
      | Error msg -> return Error( (msg.Replace ("\"", "")))
    else
      return Error( (response.responseText.Replace ("\"", "")))
}


let init (user: AccountSession) =
  let tickerFromUrl = (Router.currentUrl ()).Tail.[0]
  
  
  let validTicker =
    if tickerFromUrl.Length < 6 && Regex.Match(tickerFromUrl, "^[a-zA-Z]+$").Success then true else false
  
  let isPinAdded = 
    if user.Pinned |> List.exists ((=) tickerFromUrl) then true else false

  let intialCmd = if validTicker then Cmd.ofMsg LoadStockGraphData else Cmd.navigate("dashboard")

  { Ticker = tickerFromUrl.ToUpper ()
    IsPinAdded = isPinAdded
    PinIsLoading = false
    User = user
    GraphClosePrices = None
    GraphDates = None}, intialCmd

let update (msg: Msg) (state: State) =
    match msg with
    | LoadStockGraphData ->
      let fetchGraphData = async {
        let! result = loadGraphData state.User.Username state.User.SessionId state.Ticker
        return LoadedStockGraphData result
      }
      state, Cmd.fromAsync fetchGraphData

    | LoadedStockGraphData result ->
      match result with
      | Ok data -> 
        let newState = {state with GraphClosePrices = Some data.ClosePrices ; GraphDates = Some data.Dates}
        newState, Cmd.none
      | _ -> state, Cmd.none
        

    | StartPinAction pinAction  ->
      let newState = {state with PinIsLoading = true}
      let tryPinAction = async {
        let! result = startPinAction state.User.Username state.User.SessionId pinAction 
        return PinActionResult result
      }
      newState, Cmd.fromAsync tryPinAction
    | PinActionResult result ->
      match result with
      | Ok updatedPins ->
        let updatedUser = {state.User with Pinned = updatedPins.Pinned}
        let isPinAdded = 
          if updatedUser.Pinned |> List.exists ((=) state.Ticker) then true else false
        {state with User = updatedUser; PinIsLoading = false; IsPinAdded = isPinAdded }, Cmd.none
      | _ ->
        {state with PinIsLoading = false; }, Cmd.none


