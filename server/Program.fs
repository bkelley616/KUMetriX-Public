namespace KumetrixServer

open Saturn.Application
open Saturn.Pipeline
open Saturn.Endpoint
open Newtonsoft.Json
open Giraffe
open Microsoft.AspNetCore.Cors.Infrastructure

open KumetrixServer.Handlers

module ServerApplication =
    
    let setHeader =
        pipeline { set_header "x-app-name" "kumetrix" } //SET ON CLIENT, configure cors

    let apiRouter =
        router {
            postf "/data/summary/%s" Api.getTickerDataSummary
            postf "/data/%s" Api.getTickerData
            post "/pinned/add" Api.addPinnedTicker
            post "/pinned/remove" Api.removeUserPinnedTicker
        }

    let appRouter =
        router {
            pipe_through setHeader
            post "/auth/signup" Auth.signup
            post "/auth/login" Auth.login
            post "/auth/logout" Auth.logout
            forward "/api" apiRouter

        }

    type RequireObjectPropertiesContractResolver() =
        inherit Newtonsoft.Json.Serialization.DefaultContractResolver()
         override this.CreateProperty(memberInfo : System.Reflection.MemberInfo, memberSerialization : MemberSerialization) =
            let property = base.CreateProperty(memberInfo, memberSerialization)
            //if Reflection.FSharpType.IsRecord property.PropertyType then
            property.Required <- Required.Always
            property


    let configuredJsonSettings = new JsonSerializerSettings()
    configuredJsonSettings.ContractResolver <- new RequireObjectPropertiesContractResolver()


    let configure_cors (builder: CorsPolicyBuilder) =
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .WithHeaders [|"kumetrix"|]
        |> ignore


    let kumetrixApp =
        application {
            use_json_settings configuredJsonSettings
            use_endpoint_router appRouter
            use_cors "CORS_policy" configure_cors
            use_gzip
        }

    run kumetrixApp
