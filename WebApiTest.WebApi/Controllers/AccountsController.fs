module WebApiTest.WebApi.Controllers.AccountsController

open Microsoft.AspNetCore.Http
open Giraffe
open WebApiTest.WebApi.CompositionRoot

let getAccountBalance : HttpHandler = text (y.ToString()) //(accountsService.LoadAccount({ Name = "Isaac" }).Balance.ToString())