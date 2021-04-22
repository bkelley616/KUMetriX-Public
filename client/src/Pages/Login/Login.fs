module Login

open Elmish
open Feliz
open Feliz.Router
open Fable.Core.JsInterop

open HelpersUtil.TypedCSS
open HelpersUtil.Images
importAll "./Styles.scss"

type State =
    { LoginUsername: string
      LoginPassword: string
      LoginAttempt: Deferred<AuthApi.LoginResult>
      NewAccountEmail : string
      NewAccountUsername : string
      NewAccountPassword : string
      NewAccountSignUpCode : string 
      CreateAccountAttempt: Deferred<AuthApi.CreateAccountResult>
      ShowLoginForm: bool
      }

type Msg =
    | LoginUsernameChanged of string
    | LoginPasswordChanged of string
    | Login of AsyncOperationStatus<AuthApi.LoginResult>
    | NewAccountEmailChanged of string
    | NewAccountUsernameChanged of string
    | NewAccountPasswordChanged of string
    | NewAccountSignUpCodeChanged of string
    | CreateAccount of AsyncOperationStatus<AuthApi.CreateAccountResult>
    | ShowLogin
    | ShowCreateAccount

let init() =
    { LoginUsername = ""
      LoginPassword = ""
      LoginAttempt = HasNotStartedYet
      NewAccountEmail = ""
      NewAccountUsername = ""
      NewAccountPassword = ""
      NewAccountSignUpCode = ""
      CreateAccountAttempt = HasNotStartedYet
      ShowLoginForm = true
      }, Cmd.none

let (|UserLoggedIn|_|) = function
    | Msg.Login (Finished (AuthApi.LoginResult.LoggedIn user)) -> Some user
    | _ -> None

let (|UserLoginError|_|) = function
    | Msg.Login (Finished (AuthApi.LoginResult.UsernameOrPasswordIncorrect errorMsg)) -> Some errorMsg
    | _ -> None

let (|AccountCreatedSucessfully|_|) = function
    | Msg.CreateAccount (Finished (AuthApi.CreateAccountResult.CreatedSucessfully sucessMsg)) -> Some sucessMsg
    | _ -> None

let (|ErrorCreatingAccount|_|) = function
    | Msg.CreateAccount (Finished (AuthApi.CreateAccountResult.ErrorCreatingAccount errorMsg)) -> Some errorMsg
    | _ -> None


let update (msg: Msg) (state: State) =
    match msg with
    | ShowLogin ->
        { state with ShowLoginForm = true  }, Cmd.none

    | ShowCreateAccount ->
        { state with ShowLoginForm = false }, Cmd.none

    | LoginUsernameChanged usernameInput ->
        { state with LoginUsername = usernameInput  }, Cmd.none

    | LoginPasswordChanged passwordInput ->
        { state with LoginPassword = passwordInput  }, Cmd.none

    | NewAccountEmailChanged emailInput ->
        { state with NewAccountEmail = emailInput  }, Cmd.none

    | NewAccountUsernameChanged usernameInput ->
        { state with NewAccountUsername = usernameInput  }, Cmd.none

    | NewAccountPasswordChanged passwordInput ->
        { state with NewAccountPassword = passwordInput  }, Cmd.none

    | NewAccountSignUpCodeChanged signUpCodeInput ->
        { state with NewAccountSignUpCode = signUpCodeInput  }, Cmd.none

    | Login Started ->
        let newState = {state with LoginAttempt = InProgress}
        let login =  async {
            let! loginResult = AuthApi.login state.LoginUsername state.LoginPassword
            return Login (Finished loginResult)
        }
        newState, Cmd.fromAsync login

    | CreateAccount Started ->
        let newState = {state with CreateAccountAttempt = InProgress}
        let createAccount =  async {
            let! createAccountResult = AuthApi.createAccount state.NewAccountEmail state.NewAccountUsername state.NewAccountPassword state.NewAccountSignUpCode
            return CreateAccount (Finished createAccountResult)
        }
        newState, Cmd.fromAsync createAccount

    | Login (Finished loginResult) ->
        let newState = {state with LoginAttempt = Resolved loginResult}
        newState, Cmd.none
        
    | CreateAccount (Finished message) ->
        let newState = {state with CreateAccountAttempt = Resolved message}
        newState, Cmd.none

