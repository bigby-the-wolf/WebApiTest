open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open WebApiTest.WebApi.Controllers

module WebApp =
    let parsingError err = RequestErrors.BAD_REQUEST err

    let webApp =
        choose [
            route "/ping"   >=> text "pong"

            GET >=> choose [
                subRoute "/accounts" (choose [
                    route "/test" >=> text "yes"
                    routef "/%s" AccountsController.getAccount
                ])
            ]

            POST >=> route "/accounts/deposita" >=> bindJson<AccountsController.DepositPost> (validateModel AccountsController.depositInAccount)

            RequestErrors.NOT_FOUND "Not Found"
        ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe WebApp.webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0