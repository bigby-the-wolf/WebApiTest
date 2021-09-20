module WebApiTest.WebApi.Startup

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe

open WebApiTest.WebApi.Controllers
open WebApiTest.WebApi.CompositionRoot

let webApp =
    choose [
        route "/ping"   >=> AccountsController.getAccountBalance
        route "/pong"   >=> AccountsController.getAccountBalance2
        route "/"       >=> htmlFile "/pages/index.html" ]

type Startup(configuration : IConfiguration) =
    member __.ConfigureServices (services : IServiceCollection) =
        
        configureDependencies configuration services |> ignore

        services.AddGiraffe() |> ignore

    member __.Configure (app : IApplicationBuilder)
                        (env : IHostEnvironment)
                        (loggerFactory : ILoggerFactory) =
        // Add Giraffe to the ASP.NET Core pipeline
        app.UseGiraffe webApp