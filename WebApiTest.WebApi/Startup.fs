namespace WebApiTest.WebApi

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Giraffe
open System

open WebApiTest.WebApi.Controllers
open WebApiTest.WebApi.Models

module Configurations =
     let errorHandler (ex : Exception) (logger : ILogger) =
         logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
         clearResponse
         >=> ServerErrors.INTERNAL_ERROR ex.Message

module WebApp =
    let private parsingError err = RequestErrors.BAD_REQUEST err

    let webApp =
        choose [
            route "/ping"   >=> text "pong"

            GET >=> choose [
                subRoute "/accounts" (choose [
                    routef "/%s" AccountsController.getAccount
                ])
            ]

            POST >=> choose [
                subRoute "/accounts" (choose [
                    route "/deposit"    >=> bindJson<DepositPost> (fun depositPost -> AccountsController.depositInAccount(depositPost))
                    route "/withdrawal" >=> bindJson<WithdrawalPost> (fun withdrawalPost -> AccountsController.withdrawFromAccount(withdrawalPost))
                ])
            ]

            RequestErrors.NOT_FOUND "Not Found"
        ]

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member _.ConfigureServices(services: IServiceCollection) =
        services.AddGiraffe() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            .UseGiraffeErrorHandler(Configurations.errorHandler)
            .UseGiraffe WebApp.webApp
