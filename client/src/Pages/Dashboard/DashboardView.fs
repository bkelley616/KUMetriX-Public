module DashboardView 

open Feliz
open Feliz.Router
open Fable.Core.JsInterop
importAll "./Styles.scss"

open HelpersUtil.TypedCSS
open Dashboard


let percentChange (openPrice: float) (closePrice: float)=
  let difference = closePrice - openPrice
  let percentChange = ((closePrice / openPrice) - 1.0 ) * 100.0
  difference, percentChange
  


let showChange openPrice closePrice = 
  match percentChange openPrice closePrice with 
  | result when snd result > 0.0 -> 
    Html.h3 [
      prop.classes [Bulma.has_text_success; Bulma.has_text_centered; Bulma.is_size_4; Bulma.is_size_5_mobile;  ]
      let formattedOutput = sprintf "%.2f (%.2f%%)" (fst(result)) (snd(result))
      prop.children [
        Html.span [
          prop.classes [Bulma.icon_text]
          prop.children [
            Html.span [
              prop.text formattedOutput
            ]
            Html.span [
              prop.classes [Bulma.icon]
              prop.children [
                Html.i [
                  prop.classes [ Icon.fas; Icon.fa_caret_square_up; Bulma.is_success] 
                ]
              ]
            ] 
          ]
        ]
      ]
    ]
  | result  -> 
    Html.h3 [
        prop.classes [Bulma.has_text_danger; Bulma.has_text_centered;  Bulma.is_size_4; Bulma.is_size_5_mobile;]
        let formattedOutput = sprintf "%.2f (%.2f%%)" (fst(result)) (snd(result) * -1.0)
        prop.children [
          Html.span [
            prop.classes [Bulma.icon_text]
            prop.children [
              Html.span [
                prop.text formattedOutput
              ]
              Html.span [
                prop.classes [Bulma.icon]
                prop.children [
                  Html.i [
                    prop.classes [ Icon.fas; Icon.fa_caret_square_down; Bulma.is_danger] 
                  ]
                ]
              ] 
            ]
          ]
        ]
    ]


let loadingSpinner = 
  Html.div [
    prop.children[
      Html.i [
        prop.classes [ Icon.fas; Icon.fa_spinner; Icon.fa_pulse; Bulma.has_text_info; "app-loading-spinner"]
      ]
    ]
  ]

type stockCard =
  | Market
  | Pinned

let renderStockCards ( stocks: stockData list) (stockType: stockCard)   =
  Html.div [
    prop.classes[Bulma.is_flex; Bulma.mb_6; Bulma.is_flex_wrap_wrap; Bulma.is_justify_content_center ]
    prop.children [
      for stock in stocks ->
        Html.div [
          prop.classes [Bulma.card; Bulma.m_6;  if stockType = Market then "app-stock-card" else "app-small-stock-card" ]
          prop.onClick (fun _ -> Router.navigate("stock", stock.ticker))
          prop.children [
            Html.header [
              Html.p [
                prop.classes [Bulma.card_header_title; Bulma.is_centered; if stockType = Market then Bulma.is_size_3; Bulma.is_size_4_mobile; else Bulma.is_size_4; Bulma.is_size_5_mobile;]
                prop.text (stock.name)
              ]
            ]
            Html.div [
              prop.classes [Bulma.card_content]
              prop.children [
                match stock.summary with
                | HasNotStartedYet -> Html.none
                | InProgress -> loadingSpinner
                | Resolved data -> 
                  match data with
                  | SummaryDataError errMsg ->
                    Html.p [
                      prop.classes [ Bulma.has_text_danger; Bulma.has_text_centered; Bulma.is_size_6]
                      prop.text errMsg
                    ]
                  | SummaryDataRetrieved data ->
                      showChange (fst data) (snd data)
              ]
            ]
          ]
        ]
    ]
  ]

let render (state: State) (dispatch: Msg -> unit) = 
  Html.div [
    prop.classes [Bulma.container; "app-dashboard"]
    prop.children [
      Html.div [
        prop.children [
          Html.h2 [
            prop.classes [Bulma.subtitle; Bulma.is_3; Bulma.has_text_centered; "app-market-overview-header"]
            prop.text "Market Overview:"
          ]
          Html.div [
            prop.children [
              renderStockCards state.Markets Market
            ]
          ]
        ]
      ]
      Html.div [
        prop.className "is-divider app-divider"
      ]
      Html.div [
        prop.classes [ ]
        prop.children [
          Html.h2 [
            prop.classes [Bulma.subtitle; Bulma.is_3; Bulma.has_text_centered;]
            prop.text "Pinned Stocks:"
          ]
          if state.User.Pinned.Length < 1 then
            Html.p [
              prop.classes [Bulma.has_text_centered; Bulma.mb_6;"app-no-pins-text"]
              prop.text "No pinned stocks to display... Get started by searching for a stock!"
            ]
          else 
            renderStockCards state.Pinned Pinned
        ]
      ]
    ]
  ]