
module LoginView 

open Feliz
open Feliz.Router
open Fable.Core.JsInterop
importAll "./Styles.scss"

open Shared.Validators
open HelpersUtil.TypedCSS
open HelpersUtil.Images
open Login

  let loginLayout (childElements:ReactElement list) =
    Html.div [
        prop.classes ["loginBackground"; Bulma.hero; Bulma.is_fullheight]
        prop.children[
            Html.div [
                prop.classes [Bulma.hero_body]
                prop.children [
                    Html.div [
                        prop.classes [Bulma.container; Bulma.has_text_centered]
                        prop.children[
                            Html.div [
                                prop.classes[Bulma.columns; Bulma.is_centered]
                                prop.children[
                                    Html.div [
                                        prop.classes [ Bulma.column; Bulma.is_four_fifths_tablet;  Bulma.is_8_desktop; Bulma.is_6_widescreen]
                                        prop.children childElements
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

   let loginForm (state: State) (dispatch: Msg -> unit) = 
    Html.div[
        prop.classes[]
        prop.children[
            Html.p [
                prop.classes [Bulma.is_size_3; Bulma.has_text_weight_bold; Bulma.mb_5]
                prop.text "Login:"
            ]
            
            Html.div[
                prop.classes["field"]
                prop.children[
                    Html.label[
                        prop.classes [Bulma.label; Bulma.has_text_left; Bulma.is_size_5]
                        prop.text "Username"
                    ]
                    Html.div[
                        prop.classes[Bulma.control; Bulma.has_icons_left]
                        prop.children[
                            Html.input[
                                prop.classes[Bulma.input]
                                prop.placeholder "GMEToTheMoon"
                                prop.valueOrDefault state.LoginUsername
                                prop.onChange (LoginUsernameChanged >> dispatch)
                            ]
                            Html.span[
                                prop.classes [Bulma.icon; Bulma.is_left;]
                                prop.children[
                                    Html.i[
                                        prop.classes[Icon.fas; Icon.fa_user]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            Html.div[
                prop.classes["field"]
                prop.children[
                    Html.label[
                        prop.classes [Bulma.label; Bulma.has_text_left; Bulma.is_size_5]
                        prop.text "Password"
                    ]
                    Html.div[
                        prop.classes[Bulma.control; Bulma.has_icons_left]
                        prop.children[
                            Html.input[
                                prop.classes[Bulma.input]
                                prop.type'.password
                                prop.placeholder "Much Secret"
                                prop.valueOrDefault state.LoginPassword
                                prop.onChange (LoginPasswordChanged >> dispatch)
                            ]
                            Html.span[
                                prop.classes [Bulma.icon; Bulma.is_left;]
                                prop.children[
                                    Html.i[
                                        prop.classes[Icon.fas; Icon.fa_lock]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            Html.div[
               prop.classes [Bulma.field]
               prop.children [
                   Html.button [
                       prop.classes ["loginButton";Bulma.button; Bulma.is_fullwidth; Bulma.mt_6; Bulma.mb_5; 
                        if state.LoginAttempt = InProgress then Bulma.is_loading
                       ]
                       if state.LoginUsername.Length < 7 || state.LoginPassword.Length < 9 then prop.disabled  true
                       prop.onClick (fun _ -> dispatch (Login Started))
                       prop.text "Login"
                   ]
               ]
            ]
            
            Html.span[
                Html.p[
                    prop.classes [ Bulma.is_inline_block; Bulma.mr_2]
                    prop.text "Don't have an account?"
                ]
                Html.a[
                    prop.classes ["signUpLink"; Bulma.is_size_6;]
                    prop.text "Sign up"
                    prop.onClick (fun _ -> dispatch (ShowCreateAccount))
                ]
            ]    
        ]
    ]
   #nowarn "0058"
   let createAccountForm (state: State) (dispatch: Msg -> unit) = 
        Html.div[
           prop.classes[]
           prop.children[
                Html.p [
                    prop.classes [Bulma.is_size_3; Bulma.has_text_weight_bold; Bulma.mb_3]
                    prop.text "Create Account:"
                ]
                Html.div[
                    prop.classes["field"]
                    prop.children[

                        Html.label[
                            prop.classes [Bulma.label; Bulma.has_text_left; Bulma.is_size_5]
                            prop.text "Email"
                        ]
                        Html.div[
                            prop.classes[Bulma.control; Bulma.has_icons_left]
                            prop.children[
                                Html.input[
                                    prop.classes[Bulma.input]
                                    prop.type'.email
                                    prop.placeholder "example@kumetrix.com"
                                    prop.valueOrDefault state.NewAccountEmail
                                    prop.onChange (NewAccountEmailChanged >> dispatch)
                                ]
                                Html.span[
                                    prop.classes [Bulma.icon; Bulma.is_left;]
                                    prop.children[
                                        Html.i[
                                            prop.classes[Icon.fas; Icon.fa_envelope]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div[
                    prop.classes["field"]
                    prop.children[
                        Html.label[
                            prop.classes [Bulma.label; Bulma.has_text_left; Bulma.is_size_5]
                            prop.text "Username"
                        ]
                        Html.div[
                            prop.classes[Bulma.control; Bulma.has_icons_left]
                            prop.children[
                                Html.input[
                                    prop.classes[Bulma.input]
                                    prop.placeholder "GMEToTheMoon"
                                    prop.valueOrDefault state.NewAccountUsername
                                    prop.onChange (NewAccountUsernameChanged >> dispatch)
                                ]
                                Html.span[
                                    prop.classes [Bulma.icon; Bulma.is_left;]
                                    prop.children[
                                        Html.i[
                                            prop.classes[Icon.fas; Icon.fa_user]
                                        ]
                                    ]
                                ]
                                Html.p [
                                    prop.classes[Bulma.help; Bulma.has_text_left;]
                                    prop.text "*Minimum length of 7 characters"
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div[
                    prop.classes["field"]
                    prop.children[
                        Html.label[
                            prop.classes [Bulma.label; Bulma.has_text_left; Bulma.is_size_5]
                            prop.text "Password"
                        ]
                        Html.div[
                            prop.classes[Bulma.control; Bulma.has_icons_left]
                            prop.children[
                                Html.input[
                                    prop.classes[Bulma.input]
                                    prop.type'.password
                                    prop.placeholder "Much Secret"
                                    prop.valueOrDefault state.NewAccountPassword
                                    prop.onChange (NewAccountPasswordChanged >> dispatch)
                                ]
                                Html.span[
                                    prop.classes [Bulma.icon; Bulma.is_left;]
                                    prop.children[
                                        Html.i[
                                            prop.classes[Icon.fas; Icon.fa_lock]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.p [
                            prop.classes[Bulma.help; Bulma.has_text_left;]
                            prop.text "*Minimum length of 9 characters"
                        ]
                    ]
                ]

                Html.div[
                    prop.classes["field"]
                    prop.children[
                        Html.label[
                            prop.classes [Bulma.label; Bulma.has_text_left; Bulma.is_size_5]
                            prop.text "Sign Up Code"
                        ]
                        Html.div[
                            prop.classes[Bulma.control; Bulma.has_icons_left]
                            prop.children[
                                Html.input[
                                    prop.classes[Bulma.input;
                                    
                                    ]
                                    prop.type'.password
                                    prop.placeholder "If you know, you know"
                                    prop.valueOrDefault state.NewAccountSignUpCode
                                    prop.onChange (NewAccountSignUpCodeChanged >> dispatch)
                                    
                                ]
                                Html.span[
                                    prop.classes [Bulma.icon; Bulma.is_left;]
                                    prop.children[
                                        Html.i[
                                            prop.classes[Icon.fas; Icon.fa_key]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]

                Html.div[
                   prop.classes [Bulma.field]
                   prop.children [
                       Html.button [
                           prop.classes ["createAccountButton"; Bulma.button; Bulma.is_fullwidth; Bulma.mt_6; Bulma.mb_5;
                                if state.CreateAccountAttempt = InProgress then Bulma.is_loading
                           ]
                           match validateSignUp state.NewAccountUsername state.NewAccountPassword state.NewAccountEmail with
                           | Ok () ->
                                if state.NewAccountSignUpCode.Length < 10 then
                                    prop.disabled  true
                           | Error err -> prop.disabled  true
                           prop.onClick (fun _ -> dispatch (CreateAccount Started))
                           prop.text "Create Account"
                       ]
                   ]
                ]
                
                Html.span[
                    Html.p[
                        prop.classes [ Bulma.is_inline_block; Bulma.mr_2]
                        prop.text "Already have an account?"
                    ]
                    Html.a[
                        prop.classes ["loginLink"; Bulma.is_size_6;]
                        prop.text "Login"
                        prop.onClick (fun _ -> dispatch (ShowLogin))
                    ]
                ]
            ]
        ]

      let render (state: State) (dispatch: Msg -> unit)  =
        loginLayout [
            Html.div [
                prop.classes [Bulma.box]
                prop.children [
                    Html.div [
                        prop.classes []
                        prop.children [
                            Html.img [
                                prop.classes ["loginLogo"]
                                prop.src (kumetrixAltLogo())
                                prop.onClick (fun _ -> Router.navigate("home"))
                            ]
                        ]
                    ]
                    if state.ShowLoginForm = true 
                        then loginForm state dispatch 
                        else createAccountForm state dispatch
                ]
            ]
        ]