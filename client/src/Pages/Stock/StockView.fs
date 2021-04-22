module StockView 

open Feliz
open Feliz.Router
open Feliz.Plotly
open Fable.Core.JsInterop
importAll "./Styles.scss"

open Shared.Validators
open HelpersUtil.TypedCSS
open HelpersUtil.Images
open SharedView
open Stock


let pinButton (isAdded: bool) (isLoading: bool) (ticker:string) (dispatch: Msg -> unit)  =
  Html.div [
    prop.classes []
    prop.children [
      match isAdded with
      | true ->
      Html.div [
        prop.onClick (fun _ -> dispatch(StartPinAction (RemovePin ticker)))
        prop.classes [Bulma.button; Bulma.is_danger; "app-pin-button";
          if isLoading then Bulma.is_loading ]
        prop.children[
          Html.i [
            prop.classes [ Icon.fas;  Icon.fa_minus; Icon.fa_1x; "bigger-icon"]
          ]
        ]
      ]
      | false ->
        Html.div [
          prop.onClick (fun _ -> dispatch(StartPinAction (AddPin ticker)))
          prop.classes [Bulma.button; Bulma.is_success; "app-pin-button";
          if isLoading then Bulma.is_loading ]
          prop.children[
            Html.i [
              prop.classes [ Icon.fas;  Icon.fa_plus; Icon.fa_1x; "bigger-icon" ]
            ]
          ]
        ]
    ]
  ]


let render (state: State) (dispatch: Msg -> unit) = 
  Html.div [
    prop.classes [Bulma.container; "app-stock-page-container"; ]
    prop.children [
      if (state.GraphClosePrices <> None && state.GraphDates <> None) then
        Html.div [
          prop.classes [Bulma.card;Bulma.mx_4 ]
          prop.children [
            Html.div[
              prop.classes [Bulma.mt_6; Bulma.ml_6; Bulma.pt_6; Bulma.is_flex; ]
              prop.children [
                Html.h1 [
                  prop.classes [ Bulma.title; Bulma.is_1; Bulma.mr_3;]
                  prop.text state.Ticker
                ]
                pinButton state.IsPinAdded state.PinIsLoading state.Ticker dispatch
              ]
            ]
            Html.div [
              prop.classes [Bulma.mt_3; Bulma.mx_6; Bulma.pl_0; "app-graph"]
              prop.children [
                Plotly.plot [
                  
                  plot.config [
                    config.responsive true
                    config.displayModeBar.false'
                  ]
                  plot.traces [
                    traces.scatter [
                      scatter.mode.lines
                      scatter.name state.Ticker
                      match state.GraphDates with
                      | Some dates -> scatter.x dates
                      | _ -> ()
    
                      match state.GraphClosePrices with
                      | Some closePrices -> scatter.y closePrices
                      | _ -> ()
                    ]
                  ]
                  plot.layout [
                    layout.height 450
                    layout.margin [
                      margin.r 32
                      margin.b 32
                      margin.t 32
                      margin.l 32
                    ]
                    layout.xaxis [
                      xaxis.autorange.true'
                      xaxis.rangeselector [
                        rangeselector.buttons [
                          buttons.button [
                          button.count 1
                          button.label "1m"
                          button.step.month
                          button.stepmode.backward
                          ]
                          buttons.button [
                            button.count 6
                            button.label "6m"
                            button.step.month
                            button.stepmode.backward
                          ]
                          buttons.button [
                            button.count 1
                            button.label "1y"
                            button.step.year
                            button.stepmode.backward
                          ]
                          buttons.button [
                            button.count 5
                            button.label "5y"
                            button.step.year
                            button.stepmode.backward
                          ]
                          buttons.button [
                            button.step.all
                          ]
                        ]
                      ]
                      xaxis.rangeslider []
                      xaxis.type'.date
                    ]
                    layout.yaxis [
                      yaxis.autorange.true'
                      yaxis.type'.linear
                    ]
                ]
                ]
              ]
            ]
          ]
        ]
        

    ]
  ]