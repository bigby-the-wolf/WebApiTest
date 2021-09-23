module WebApiTest.WebApi.Controllers.AccountsController

open FSharp.Control.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open System

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Domain.Services.AccountService
open WebApiTest.Domain.Operations.AccountOperations
open WebApiTest.Repositories.SqlServer.AccountRepository

let flip f x y = f y x

type IModelParser<'From, 'To> =
    abstract member Parse : 'From -> Result<'To, string>

type Name = Name of string
type Amount = Amount of decimal

type Deposit =
    {
        Owner : Name
        Amount : Amount
    }

[<CLIMutable>]
type DepositPost = 
    {
        OwnerName : string
        Amount : int
    }
    member this.HasErrors() =
        if String.IsNullOrWhiteSpace(this.OwnerName) then Error "Name can't be empty!"
        elif this.Amount <= 0 then Error "Amount must be grater than 0!"
        else Ok this

    interface IModelValidation<DepositPost> with
        member this.Validate() =
            this.HasErrors()
            |> Result.mapError(fun err -> RequestErrors.badRequest (text err))

    //interface IModelParser<DepositDto

let getAccount(name:string) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]
        
        (name
        |> getAccountAndTransactions connectionString
        |> Option.map(buildAccount name)
        |> Successful.OK) next ctx

let depositInAccount(depositPost : DepositPost) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]
        
        (depositPost.OwnerName
        |> getAccountAndTransactions connectionString
        |> Option.map(buildAccount depositPost.OwnerName)
        |> Option.map(writeTransaction connectionString  { Timestamp = DateTime.UtcNow; Operation = BankOperation.Deposit; Amount = Convert.ToDecimal(depositPost.Amount) })
        |> Successful.OK) next ctx