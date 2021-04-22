namespace Shared


open System.Text.RegularExpressions


module Validators =

    let private (|IsValidUserName|_|) input =
        let usernameLength = String.length input
        if usernameLength > 6 && usernameLength < 18 then
            Some ()
        else
            None


    let private (|IsValidPassword|_|) input =
        let passwordLength = String.length input
        if passwordLength > 8 && passwordLength < 32 then
            Some ()
        else
            None


    let private (|IsValidEmail|_|) input =
        if Regex.Match(input, "^[^@\s]+@[^@\s\.]+\.[^@\.\s]+$").Success then
            Some ()
        else
            None

    let validateSignUp (name:string) (pwd:string) (email:string) =
        match name with
        | IsValidUserName ->
            match pwd with
            | IsValidPassword -> 
                match email with
                | IsValidEmail -> Ok ()
                | _ -> Error "Invalid Email Format: email@example.com"
            | _ -> Error "Invalid Password Format: Minmum Length > 8 and Maximum Length < 32"
        | _ -> Error "Invalid Username Format: Minmume Length > 6 and Maximum Length < 18"
