
module NotFound

open Feliz
open Feliz.Router
open Fable.Core.JsInterop

open HelpersUtil.TypedCSS
open HelpersUtil.Images
importAll "./Styles.scss"



let render  =
  Html.div [
    prop.classes[Bulma.is_flex; Bulma.is_flex_direction_column; Bulma.is_justify_content_center; Bulma.is_align_items_center; Bulma.p_6;  "not-found-container"]
    prop.children [
      Html.img [
        prop.classes ["not-found-dog"; Bulma.m_3]
        prop.src (notFoundDog())
      ]

      Html.h3 [
        prop.classes [Bulma.is_size_3; Bulma.m_3]
        prop.text "Looks like you\'re lost?"
      ]
      
      Html.button [
        prop.onClick(fun _ -> Router.navigate("home"))
        prop.classes [Bulma.button; Bulma.is_success; Bulma.m_3]
        prop.text "Go Home"
      ]

    ]
  ]
