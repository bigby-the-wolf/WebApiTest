module WebApiTest.WebApi.Controllers.AccountsController

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Giraffe

open WebApiTest.WebApi.CompositionRoot

let getAccountBalance : HttpHandler = text (accountsService.LoadAccount({ Name = "Isaac" }).Balance.ToString())

let getAccountBalance2 : HttpHandler =
   fun (next : HttpFunc) (ctx : HttpContext) ->
       let config = ctx.GetService<IConfiguration>()
       text (config.["ConnectionStrings:AccountsDb"]) next ctx