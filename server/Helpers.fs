namespace KumetrixServer

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe


module Helpers =
    let config = Env.combinedVariables ()

    let tryBindJsonAsync<'T> (ctx: HttpContext) =
        task {
            try
                let! payload = ctx.BindJsonAsync<'T>()

                match obj.ReferenceEquals(payload, null) with
                | true -> return None
                | false -> return Some payload

            with _ -> return None
        }
