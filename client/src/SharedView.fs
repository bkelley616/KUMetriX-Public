module SharedView

open Feliz
open HelpersUtil.TypedCSS
open Shared.Types
open Feliz.Router
open HelpersUtil.Images
open Fable.Import

open Autocomplete


type Msg = 
  | ClearSuccess
  | ClearError
  | SharedViewSearchBarFocusedMsg of bool

let modal (dispatch: Msg -> unit) (eventType: RequestResult) (bodyText:string) =
  Html.div [
    prop.classes [Bulma.modal; Bulma.is_active;]
    prop.children [
      Html.div [
        prop.classes [Bulma.modal_background;]
      ]
      Html.div [
        prop.classes [Bulma.modal_content; Bulma.p_6;]
        prop.children [
          Html.div [
            prop.classes [Bulma.box;
            match eventType with
            | SucessEvent -> 
              Bulma.has_background_success_light
              Bulma.has_text_success_dark
            | ErrorEvent -> 
              Bulma.has_background_danger_light
              Bulma.has_text_danger_dark
            ]
            prop.children [
              Html.div [
                prop.classes [ ]
                prop.children [
                  Html.p [
                    prop.classes [Bulma.mt_3; Bulma.mb_6; Bulma.is_size_4]
                    prop.text bodyText
                  ]
                  Html.button [
                    prop.classes [Bulma.button; Bulma.is_info; Bulma.my_3;]
                    prop.text "Dismiss"
                    match eventType with
                    | SucessEvent -> prop.onClick (fun _ -> dispatch ClearSuccess)
                    | ErrorEvent -> prop.onClick (fun _ -> dispatch ClearError)
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
  ]



let autoCompleteSearchPressed (keyPressed: Browser.Types.KeyboardEvent) (ticker:string) (dispatch: Msg -> unit) =
  if keyPressed.charCode = 13.0 then
    SharedViewSearchBarFocusedMsg false |> dispatch
    Router.navigate(sprintf "stock", ticker)

let autoCompleteSearchClick (ticker:string) (dispatch: Msg -> unit) =
    SharedViewSearchBarFocusedMsg false |> dispatch
    Router.navigate(sprintf "stock", ticker)




let trimSymbols (filterSymbols: string array) =
  match filterSymbols.Length with 
  | size when size > 5  ->
      filterSymbols |> Array.take 5
  | size when size = 0 ->
      [||]
  | _ ->
    filterSymbols


let ignoreTabKeyOnBlur (keyPressed: option<float>) (dispatch: Msg -> unit) =
    match keyPressed with
    | Some charCode ->
        if charCode <> 9.0 then
          SharedViewSearchBarFocusedMsg false |> dispatch
    | None -> SharedViewSearchBarFocusedMsg false |> dispatch

let autocompleteOptions (searchValue:string) (lastKeyPressed:option<float>)  (dispatch: Msg -> unit):Fable.React.ReactElement = 

  let matchingSymbols =  (Array.filter (fun (sym:string) -> sym.ToUpper().IndexOf(searchValue.ToUpper()) = 0) autocompleteArray) 
  let renderSymbols = trimSymbols matchingSymbols
 
      
  Html.div [
    prop.classes [Bulma.dropdown_menu; "app-dropdown-menu"]
    prop.children [
      Html.div [
        prop.classes [Bulma.dropdown_content; "app-dropdown-content"]
        prop.children [
          match renderSymbols.Length <= 0 with
          | true ->
              yield 
                Html.a [
                prop.classes [Bulma.dropdown_item; "app-no-suggestions"]
                prop.text "No Suggestions..."
                ]
          | false ->
            for symbol in renderSymbols ->
              Html.a [
                prop.tabIndex 0

                prop.onFocus (fun _ -> SharedViewSearchBarFocusedMsg true |> dispatch)
                prop.onBlur (fun _ -> ignoreTabKeyOnBlur lastKeyPressed dispatch)
                prop.onKeyPress (fun event -> autoCompleteSearchPressed event symbol dispatch)
                prop.onClick (fun event -> autoCompleteSearchClick symbol dispatch)
                prop.classes [Bulma.dropdown_item; "app-dropdown-item"]
                prop.text symbol
              ]
        ]
      ]
    ]
  ]


let footer =
  Html.div [
    prop.classes [ Bulma.footer; "app-footer"]
    prop.children [
      Html.div [
        prop.classes [Bulma.container; "app-footer-container"]
        prop.children [
          Html.a [
            prop.classes [ Bulma.button; "logout-button"]
            prop.onClick (fun _ -> Router.navigate("logout"))
            prop.children[
                Html.span[
                    prop.classes [ Bulma.icon]
                    prop.children [
                        Html.i [
                            prop.classes[Icon.fas; Icon.fa_sign_out_alt]
                        ]
                    ]
                ]
                Html.span[
                    prop.text "Logout"
                ]
            ]
          ]

          Html.a [
            prop.classes [ Bulma.button; "account-settings-button"]
            prop.onClick (fun _ -> printfn "accountSettings") //TODO implement account settings
            prop.children[
                Html.span[
                    prop.classes [ Bulma.icon]
                    prop.children [
                        Html.i [
                            prop.classes[Icon.fas; Icon.fa_cog]
                        ]
                    ]
                ]
                Html.span[
                    prop.text "Account Settings"
                ]
            ]
          ]

          Html.a [
            prop.classes [ Bulma.button; "github-link-footer"]
            prop.href "https://github.com/bkelley616/KUMetriX-Public"
            prop.children[
                Html.span[
                    prop.classes [ Bulma.icon]
                    prop.children [
                        Html.i [
                            prop.classes[Icon.fab; Icon.fa_github]
                        ]
                    ]
                ]
                Html.span[
                    prop.text "Github"
                ]
            ]
          ]
        ]
      ]
    ]
  ]