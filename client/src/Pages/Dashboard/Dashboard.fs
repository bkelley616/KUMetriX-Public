module Dashboard


open Elmish
open Feliz
open Feliz.Router
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Thoth.Json


open Shared.Types
open HelpersUtil.TypedCSS
open HelpersUtil.Images

importAll "./Styles.scss"

type StockSummaryResult =
  | SummaryDataError of string
  | SummaryDataRetrieved of (float * float)
  
type stockData =
    { name: string
      ticker: string
      summary: Deferred<StockSummaryResult> }



let marketIndexIndices =
    [ ("S&P 500", "SPY")
      ("NASDAQ", "ONEQ")
      ("DOW", "DIA") ]


type State =
    { User: AccountSession
      Markets: stockData list
      Pinned: stockData list }

type Msg =
    | LoadStockSummaryData
    | LoadedStockSummary of string * StockSummaryResult




let getStockSummary (username: string) (sessionId: string) (ticker: string)  =
    async {
      let requestBody = { Username = username; SessionId = sessionId}

      let! response =
        Http.request (sprintf "api/data/summary/%s" ticker) 
        |> Http.method POST
        |> Http.content (BodyContent.Text(Encode.Auto.toString(4,requestBody)))
        |> Http.header (Headers.contentType "application/json")
        |> Http.header (Headers.authorization "kumetrix")
        |> Http.send
        
      if response.statusCode = 200 then
        let tryDecodeNewAccountSession = Decode.Auto.fromString<SummaryResponse>(response.responseText)
        match tryDecodeNewAccountSession with
        | Ok summary -> return LoadedStockSummary( ticker, SummaryDataRetrieved (summary.Open, summary.Close))
        | Error msg -> return LoadedStockSummary( ticker, SummaryDataError (msg.Replace ("\"", "")))
      else
        return LoadedStockSummary( ticker, SummaryDataError (response.responseText.Replace ("\"", "")))

    }


let init (user: AccountSession) =
    let markets =
        marketIndexIndices
        |> List.map
            (fun m ->
                { name = fst m
                  ticker = snd m
                  summary = HasNotStartedYet })

    let pinnedTickers =
        user.Pinned
        |> List.map
            (fun t ->
                { name = t
                  ticker = t
                  summary = HasNotStartedYet })

    { User = user
      Markets = markets
      Pinned = pinnedTickers },
    Cmd.ofMsg (LoadStockSummaryData)


let update (msg: Msg) (state: State) =
    match msg with
    | LoadStockSummaryData  ->
      let marketsLoading = state.Markets |> List.map (fun m -> {m with summary = InProgress})
      let pinnedLoading = state.Pinned |> List.map (fun p -> {p with summary = InProgress})
      let newState = {state with Markets = marketsLoading; Pinned = pinnedLoading}
      let marketCmds = [for market in state.Markets -> Cmd.fromAsync (getStockSummary state.User.Username state.User.SessionId market.ticker)]
      let pinnedCmds = [for stock in state.Pinned -> Cmd.fromAsync (getStockSummary state.User.Username state.User.SessionId stock.ticker)]
      newState, Cmd.batch (marketCmds @ pinnedCmds) 
    | LoadedStockSummary (ticker, result)  ->
      let marketsUpdated = state.Markets |> List.map (fun m -> if m.ticker = ticker then {m with summary = Resolved result} else m)
      let pinnedUpdated = state.Pinned |> List.map (fun p -> if p.ticker = ticker then {p with summary = Resolved result} else p)
      let newState = {state with Markets = marketsUpdated; Pinned = pinnedUpdated}
      newState, Cmd.none