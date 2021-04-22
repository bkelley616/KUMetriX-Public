module AuthApi

open Fable.SimpleHttp
open Thoth.Json

open Shared.Types

type LoginResult =
    | UsernameOrPasswordIncorrect of string
    | LoggedIn of AccountSession

type CreateAccountResult =
    | CreatedSucessfully of string
    | ErrorCreatingAccount of string

let login (username: string) (password: string) =
    async {
        let requestBody = { Username = username; Password = password}
        let! response =
            Http.request "/auth/login"
            |> Http.method POST
            |> Http.content (BodyContent.Text(Encode.Auto.toString(4,requestBody)))
            |> Http.header (Headers.contentType "application/json")
            |> Http.header (Headers.authorization "kumetrix")
            |> Http.send

        if response.statusCode = 200 then
            let tryDecodeNewAccountSession = Decode.Auto.fromString<AccountSession>(response.responseText)
            match tryDecodeNewAccountSession with
            | Ok user -> return LoggedIn user
            | Error msg -> return UsernameOrPasswordIncorrect msg
        else
            return UsernameOrPasswordIncorrect response.responseText
    }

let createAccount (email: string) (username: string) (password: string) (signUpCode: string) =
    async {
        let requestBody = { Email = email; Username = username; Password = password; SignUpCode = signUpCode}
        let! response =
            Http.request "/auth/signup"
            |> Http.method POST
            |> Http.content (BodyContent.Text(Encode.Auto.toString(4,requestBody)))
            |> Http.header (Headers.contentType "application/json")
            |> Http.header (Headers.authorization "kumetrix")
            |> Http.send

        if response.statusCode = 201 then
            return CreatedSucessfully (response.responseText.Replace ("\"", ""))
        else
            return ErrorCreatingAccount (response.responseText.Replace ("\"", ""))
    }
