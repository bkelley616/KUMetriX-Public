module App

open System
open Elmish
open Feliz
open Feliz.Router
open Fable.Import
open Thoth.Json

open Dashboard
open HelpersUtil.TypedCSS
open HelpersUtil.Images
open Shared.Types
open SharedView





[<RequireQualifiedAccess>]
type Url =
    | Index
    | Home
    | Login 
    | Dashboard
    | Stock of string
    | NotFound
    | Logout

[<RequireQualifiedAccess>]
type Page =
    | Index
    | Home
    | Login of Login.State
    | Dashboard of Dashboard.State
    | Stock of Stock.State
    | NotFound


let parseUrl =
    function
    | [] -> Url.Index
    | [ "home" ] -> Url.Home
    | [ "login" ] -> Url.Login
    | [ "dashboard" ] -> Url.Dashboard
    | [ "stock"; ticker ] -> Url.Stock ticker
    | [ "logout" ] -> Url.Logout
    | _ -> Url.NotFound


type State =
    { CurrentUrl: Url
      CurrentPage: Page
      User: ApplicationUser
      SearchBarValue: string
      SearchBarInFocus: bool
      Loading: bool
      LastKeyPressed: option<float>
      Success: option<string> 
      Error: option<string> }

type Msg = 
    | UrlChanged of Url
    | SharedViewMsg of SharedView.Msg
    | LoginMsg of Login.Msg
    | DashboardMsg of Dashboard.Msg
    | StockMsg of Stock.Msg
    | SearchBarValueChangedMsg of string
    | SearchBarFocusedMsg of bool
    | KeyPressMsg of float
    


let init () =
    let intialURL = parseUrl (Router.currentUrl ())
    let currentTime = DateTime.UtcNow.ToString("o")
    let defaultState =
        { CurrentUrl = intialURL
          CurrentPage = Page.Home
          User = Anonymous
          SearchBarValue = ""
          SearchBarInFocus = false
          Loading = false
          LastKeyPressed = None
          Success = None
          Error = None }
      
    let defaultRouter = 
        match intialURL with
        | Url.Index -> defaultState , Cmd.navigate("home")
        | Url.Home -> defaultState, Cmd.none
        | Url.Login -> 
            let loginState, loginCmd = Login.init()
            { defaultState with CurrentPage = Page.Login loginState}, Cmd.map LoginMsg loginCmd
        | Url.Dashboard -> defaultState, Cmd.navigate("home", HistoryMode.ReplaceState)
        | Url.Stock ticker -> defaultState, Cmd.navigate("home", HistoryMode.ReplaceState)
        | Url.NotFound -> { defaultState with CurrentPage = Page.NotFound }, Cmd.none
        | Url.Logout -> defaultState, Cmd.navigate("home")

    let tryLoadSession = Decode.Auto.fromString<AccountSession>(Browser.WebStorage.localStorage.getItem("kumetrix"))

    match tryLoadSession with
    | Ok user ->
        match currentTime < user.SessionExpiration with
        | true ->
            printfn "Loading Profile"
            let dashboard, dashboardCmd = Dashboard.init(user)
            
            let loadedProfileState = { defaultState with CurrentPage = (Page.Dashboard dashboard); User = LoggedIn user;  }
            
            match intialURL with
            | Url.Index -> loadedProfileState , Cmd.navigate("dashboard", HistoryMode.ReplaceState)
            | Url.Home -> loadedProfileState, Cmd.navigate("dashboard", HistoryMode.ReplaceState)
            | Url.Login -> loadedProfileState, Cmd.navigate("dashboard", HistoryMode.ReplaceState)
            | Url.Dashboard -> loadedProfileState, Cmd.map DashboardMsg dashboardCmd
            | Url.Stock _ -> 
              let stock, stockCmd = Stock.init(user)
              let loadedProfileStateStockPage = { defaultState with CurrentPage = (Page.Stock stock); User = LoggedIn user }
              loadedProfileStateStockPage, Cmd.map StockMsg stockCmd
            | Url.NotFound -> { loadedProfileState with CurrentPage = Page.NotFound }, Cmd.none
            | Url.Logout -> defaultState, Cmd.navigate("home")
        | false -> 
            printfn "Session Expired"
            defaultRouter
    | _ -> 
        printfn "Session Not Found"
        defaultRouter
        



let update (msg: Msg) (state: State) =
    match msg, state.CurrentPage with
    | SearchBarValueChangedMsg inputValue, _ ->
        { state with SearchBarValue = inputValue }, Cmd.none
    | SearchBarFocusedMsg focusState, _ ->
        { state with SearchBarInFocus = focusState }, Cmd.none

    | KeyPressMsg charCode, _ ->

        { state with LastKeyPressed = Some charCode}, Cmd.none
    | SharedViewMsg msg, _ ->
        match msg with
        | ClearSuccess -> { state with Success = None }, Cmd.none
        | ClearError -> { state with Error = None }, Cmd.none
        | SharedViewSearchBarFocusedMsg focusState-> { state with SearchBarInFocus = focusState }, Cmd.none

    | LoginMsg loginMsg, Page.Login loginState ->
        match loginMsg with
        | Login.UserLoggedIn user ->
            do Browser.WebStorage.localStorage.setItem ("kumetrix", Encode.Auto.toString( 0, user))
            { state with User = LoggedIn user }, Cmd.navigate("dashboard")

        | Login.UserLoginError errorMsg ->
            let loginState, loginCmd = Login.update loginMsg loginState
            { state with Error = Some errorMsg; CurrentPage = Page.Login loginState }, Cmd.map LoginMsg loginCmd

        | Login.AccountCreatedSucessfully sucessMessage ->
            let loginState, loginCmd = Login.update loginMsg loginState
            { state with Success = Some sucessMessage; CurrentPage = Page.Login {loginState with ShowLoginForm = true} }, Cmd.map LoginMsg loginCmd

        | Login.ErrorCreatingAccount errorMessage ->
            let loginState, loginCmd = Login.update loginMsg loginState
            { state with Error = Some errorMessage; CurrentPage = Page.Login loginState }, Cmd.map LoginMsg loginCmd

        | loginMsg ->
            let loginState, loginCmd = Login.update loginMsg loginState
            { state with CurrentPage = Page.Login loginState }, Cmd.map LoginMsg loginCmd

    | DashboardMsg dashboardMsg, Page.Dashboard dashboardState ->
      match dashboardMsg with 
      | Dashboard.LoadStockSummaryData ->
        let dashboardState, dashboardCmd = Dashboard.update dashboardMsg dashboardState
        { state with CurrentPage = Page.Dashboard dashboardState }, Cmd.map DashboardMsg dashboardCmd

      | Dashboard.LoadedStockSummary (ticker, result)  ->
        match result = SummaryDataError "Error: SessionId does not match or Expired"  with
        | true ->
          do Browser.WebStorage.localStorage.setItem ("kumetrix", "")
          { state with Error = Some "Session Expired, please login" }, Cmd.navigate("login")
        | false ->
          let dashboardState, dashboardCmd = Dashboard.update dashboardMsg dashboardState
          { state with CurrentPage = Page.Dashboard dashboardState }, Cmd.map DashboardMsg dashboardCmd

    | StockMsg stockMsg, Page.Stock stockState ->
      match stockMsg with
      | Stock.LoadStockGraphData _ ->
        let stockState, stockCmd = Stock.update stockMsg stockState
        { state with CurrentPage = Page.Stock stockState; Loading = true }, Cmd.map StockMsg stockCmd

      | Stock.LoadedStockGraphData result ->

        match result with
        | Ok _ ->
          let stockState, stockCmd = Stock.update stockMsg stockState
          { state with CurrentPage = Page.Stock stockState; Loading = false }, Cmd.map StockMsg stockCmd
        | Error errMsg ->
          match errMsg = "Error: SessionId does not match or Expired" with
          | true ->
            do Browser.WebStorage.localStorage.setItem ("kumetrix", "")
            { state with Error = Some "Session Expired, please login"; Loading = false }, Cmd.navigate("login")
          | false ->
            { state with Error = Some errMsg; Loading = false }, Cmd.navigate("dashboard")
        
      | Stock.StartPinAction _ ->
        let stockState, stockCmd = Stock.update stockMsg stockState
        { state with CurrentPage = Page.Stock stockState }, Cmd.map StockMsg stockCmd

      | Stock.PinActionResult result ->
        match result with
        | Ok updatedPins ->
          let stockState, stockCmd = Stock.update stockMsg stockState
          let user = 
            match state.User with
            | LoggedIn currentUser ->
              let updatedProfile = {currentUser with Pinned = updatedPins.Pinned}
              do Browser.WebStorage.localStorage.setItem ("kumetrix", Encode.Auto.toString( 0, updatedProfile))
              LoggedIn updatedProfile
            | Anonymous -> Anonymous
          
          {state with User = user; CurrentPage = Page.Stock stockState}, Cmd.map StockMsg stockCmd
        | Error msg ->
          match msg = "Error: SessionId does not match or Expired"  with
          | true ->
            do Browser.WebStorage.localStorage.setItem ("kumetrix", "")
            { state with Error = Some "Session Expired, please login" }, Cmd.navigate("login")
          | false ->
            let stockState, stockCmd = Stock.update stockMsg stockState
            { state with CurrentPage = Page.Stock stockState; Error = Some msg }, Cmd.map StockMsg stockCmd


    | UrlChanged nextUrl, _ ->
        let show page = { state with CurrentPage = page; CurrentUrl = nextUrl; Loading = false }

        match nextUrl with
        | Url.Index -> state, Cmd.navigate("home")
        | Url.Home -> show Page.Home, Cmd.none
        | Url.Login -> 
            let login, loginCmd = Login.init()
            show (Page.Login login), Cmd.map LoginMsg loginCmd
        | Url.Dashboard -> 
            match state.User with
            | Anonymous -> state, Cmd.navigate("login", HistoryMode.ReplaceState)
            | LoggedIn user -> 
                let dashboard, dashboardCmd = Dashboard.init(user)
                show (Page.Dashboard dashboard), Cmd.map DashboardMsg dashboardCmd
        | Url.Stock ticker -> 
          match state.User with
          | Anonymous -> state, Cmd.navigate("login", HistoryMode.ReplaceState)
          | LoggedIn user -> 
              let stock, stockCmd = Stock.init(user)
              show (Page.Stock stock), Cmd.map StockMsg stockCmd
        | Url.NotFound -> show Page.NotFound, Cmd.none
        | Url.Logout ->
            do Browser.WebStorage.localStorage.setItem ("kumetrix", "")
            { state with User = Anonymous }, Cmd.navigate("home")

    | _, _ ->
        state, Cmd.none

let searchSubmitPressed (keyPressed: Browser.Types.KeyboardEvent) (dispatch: Msg -> unit) (ticker:string) =
    if keyPressed.charCode = 13.0 && ticker.Length > 0 then
      SearchBarFocusedMsg false |> dispatch
      Router.navigate("stock", ticker.ToUpper ())

let setlastKeyPressed (keyPressed: float) (dispatch: Msg -> unit) =
  KeyPressMsg keyPressed |> dispatch


let ignoreTabKeyOnBlur (keyPressed: option<float>) (searchValue: string) (dispatch: Msg -> unit) = 
  match keyPressed with
    | Some charCode ->
        if charCode <> 9.0 && searchValue.Length < 1 then
            SearchBarFocusedMsg false |> dispatch
    | None -> 
      SearchBarFocusedMsg false |> dispatch

    
    

let navbar (state: State) (dispatch: Msg -> unit) =
  Html.nav [
    prop.classes [Bulma.navbar; "app-navbar"]
    prop.children [
      Html.div [
        prop.classes [ Bulma.navbar_item; "app-nav-container"; Bulma.container]
        prop.children [
          Html.a [
            prop.classes [ "app-nav-logo-wrapper"]
            prop.onClick (fun _ -> Router.navigate("dashboard"))
            prop.children [
              Html.img [
                prop.classes ["app-navbar-logo";]
                prop.src (kumetrixNavLogo())
              ]
            ]
          ]
          Html.div [
            prop.classes [Bulma.dropdown; "app-dropdown"; 
            
            if state.SearchBarInFocus = true && state.SearchBarValue.Length > 0 && (Browser.Dom.document.activeElement.className.Contains("search") || Browser.Dom.document.activeElement.className.Contains("dropdown"))
                then Bulma.is_active;     
            ]
            prop.children [
              Html.div [
                prop.classes ["app-dropdown-trigger"]
                prop.children [
                  Html.div [
                    prop.classes [Bulma.field]
                    prop.children [
                      Html.p [
                        prop.classes [Bulma.control; Bulma.is_expanded; Bulma.has_icons_left; "app-search-bar"]
                        prop.children [
                          Html.input [
                            prop.type'.search
                            prop.onKeyDown (fun key -> setlastKeyPressed key.which dispatch)
                            prop.onKeyPress (fun keyEvent -> searchSubmitPressed keyEvent dispatch state.SearchBarValue)
                            prop.classes [Bulma.input; "app-search-bar-input"]
                            prop.onFocus (fun _ -> SearchBarFocusedMsg true |> dispatch)
                            prop.onBlur (fun _ -> ignoreTabKeyOnBlur state.LastKeyPressed state.SearchBarValue dispatch)
                            prop.placeholder "GME"
                            prop.valueOrDefault state.SearchBarValue
                            prop.onChange (SearchBarValueChangedMsg >> dispatch)
                          ]
                          Html.span [
                            prop.classes [ Bulma.icon; Bulma.is_small; Bulma.is_left]
                            prop.children [
                              Html.i [
                                prop.classes [ Icon.fas; Icon.fa_search]
                              ]
                            ]
                          ]
                        ]
                      ]
                    ]
                  ]
                ]
              ]
              autocompleteOptions state.SearchBarValue state.LastKeyPressed (SharedViewMsg >> dispatch)
            ]
          ]
        ]
      ]
    ]
  ]

let render (state: State) (dispatch: Msg -> unit) =
    let activePage =
        match state.CurrentPage with
        | Page.Home -> Home.render
        | Page.Login login -> LoginView.render login (LoginMsg >> dispatch)
        | Page.Dashboard dashboard -> DashboardView.render dashboard (DashboardMsg >> dispatch)
        | Page.Stock stock -> StockView.render stock (StockMsg >> dispatch)
        | Page.NotFound -> NotFound.render
        | _ -> Html.h1 "Empty"
    React.router [
        router.onUrlChanged (parseUrl >> UrlChanged >> dispatch)
        router.children [
            Html.div [
                prop.children [ 
                    Html.div [
                        prop.classes ["pageloader" ; if state.Loading then Bulma.is_active]
                        prop.children [
                          Html.span [
                            prop.classes ["title"]
                            prop.text "Loading Data..."
                          ]
                        ]
                      ]
                    match state.Success with
                    | Some successMessage -> modal (SharedViewMsg >> dispatch) SucessEvent successMessage
                    | None -> ()

                    match state.Error with
                    | Some errorMessage -> modal (SharedViewMsg >> dispatch) ErrorEvent errorMessage
                    | None -> ()
                    
                    

                    match state.CurrentPage with
                    | Page.Dashboard dashboard -> 
                        navbar state dispatch
                        activePage 
                        footer
                        
                    | Page.Stock stock ->
                        navbar state dispatch
                        activePage 
                        footer
                    | _ -> 
                        activePage 
                ]
            ]
        ]
    ]
