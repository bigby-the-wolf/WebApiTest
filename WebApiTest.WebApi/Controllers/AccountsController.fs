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

//let flip f x y = f y x

type IModelParser<'From, 'To> =
    abstract member Parse : unit -> 'To

type Deposit =
    {
        Owner : Name
        Amount : Amount
    }

[<CLIMutable>]
type DepositPost = 
    {
        OwnerName : string
        Amount : string
    }
    member this.HasErrors() =
        let amountResult depositPost = 
            match Decimal.TryParse(depositPost.Amount) with
            | (false, _) -> Error "Amount must be decimal!"
            | (_, amount) when amount <= 0M -> Error "Amount must be greather than 0!"
            | _ -> Ok depositPost
        
        let validation =
            if String.IsNullOrWhiteSpace(this.OwnerName) then Error "Name can't be empty!"
            else Ok this

        validation
        |> Result.bind amountResult

    interface IModelValidation<DepositPost> with
        member this.Validate() =
            this.HasErrors()
            |> Result.mapError(fun err -> RequestErrors.badRequest (text err))

    interface IModelParser<DepositPost, Deposit> with
        member this.Parse() =
            { Owner = Name this.OwnerName; Amount = Amount (Decimal.Parse(this.Amount)) }

let getAccount(name:string) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]
        
        let result =
            (Name name)
            |> getAccountAndTransactions connectionString
            |> Result.map(buildAccount)
        
        match result with
        | Ok account -> Successful.OK account next ctx
        | Error err  -> RequestErrors.BAD_REQUEST err next ctx

let depositInAccount(depositPost : DepositPost) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]

        let deposit = (depositPost :> IModelParser<DepositPost, Deposit>).Parse()

        let result =
            deposit.Owner
            |> getAccountAndTransactions connectionString
            |> Result.map(buildAccount)
            |> Result.map(writeTransaction connectionString deposit.Amount)
        
        match result with
        | Ok _ -> Successful.OK "" next ctx
        | Error err  -> RequestErrors.BAD_REQUEST err next ctx