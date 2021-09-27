namespace WebApiTest.WebApi

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Giraffe

open WebApiTest.WebApi.Controllers
open WebApiTest.WebApi.Models

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
        app.UseGiraffe WebApp.webApp
