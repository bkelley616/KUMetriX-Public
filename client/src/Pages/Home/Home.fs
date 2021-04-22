module Home


open Feliz
open Feliz.Router
open Fable.Core.JsInterop

open HelpersUtil.TypedCSS
open HelpersUtil.Images
importAll "./Styles.scss"



let render  =
    Html.div [
        prop.classes ["homeBackground"; Bulma.hero; Bulma.is_fullheight]
        prop.children [
        Html.div [
            prop.classes [ Bulma.hero_head]
            prop.children [
                Html.header [
                    prop.classes [Bulma.navbar]
                    prop.children[
                        Html.div [
                            prop.classes [ Bulma.container]
                            prop.children [
                                Html.div [
                                    prop.classes [Bulma.navbar_brand;]
                                    prop.children [
                                        Html.span [
                                            prop.classes [Bulma.navbar_item]
                                            prop.children [
                                                Html.a [
                                                    prop.classes [ Bulma.button; "github-link"]
                                                    
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
                            ]
                        ]
                    ]
                ]   
            ]
        ]

        Html.div[
            prop.classes[Bulma.hero_body; ]
            prop.children [
                Html.div[
                    prop.classes [ Bulma.container; Bulma.has_text_centered;  ]
                    prop.children[
                        Html.div[
                            prop.children[
                                Html.div[
                                    prop.children [
                                        Html.figure[
                                            
                                            prop.children[
                                                Html.img [
                                                    prop.classes [ "logoImg"]
                                                    prop.src(kumetrixLogo())

                                                 ]
                                            ]
                                        ]
                                        Html.a [
                                            prop.classes ["goToLoginButton"; Bulma.button; Bulma.is_rounded; Bulma.is_hidden_tablet; ]
                                            prop.onClick (fun _ -> Router.navigate("login"))
                                            prop.children[
                                                Html.span[
                                                    prop.classes [ Bulma.icon]
                                                    prop.children [
                                                        Html.i [
                                                            prop.classes[Icon.fas; Icon.fa_sign_in_alt]
                                                        ]
                                                    ]
                                                ]
                                                Html.span[
                                                    prop.text "Login or Create an Account"
                                                ]
                                            ]
                                        ]
                                        Html.a [
                                            prop.classes ["goToLoginButton"; Bulma.button; Bulma.is_rounded; Bulma.is_hidden_mobile; Bulma.is_medium]

                                            prop.onClick (fun _ -> Router.navigate("login"))
                                            prop.children[
                                                Html.span[
                                                    prop.classes [ Bulma.icon]
                                                    prop.children [
                                                        Html.i [
                                                            prop.classes[Icon.fas; Icon.fa_sign_in_alt]
                                                        ]
                                                    ]
                                                ]
                                                Html.span[
                                                    prop.text "Login or Create an Account"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                                            
                    ]
                ]
            ]
        ]
        

        ]
        

        
    ]


    