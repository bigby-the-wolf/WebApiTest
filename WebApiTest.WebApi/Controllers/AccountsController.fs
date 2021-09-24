module WebApiTest.WebApi.Controllers.AccountsController

open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open System

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Domain.Operations.AccountOperations
open WebApiTest.Repositories.SqlServer.AccountRepository
open WebApiTest.WebApi.Models

let getAccount(name:string) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]
        
        let result =
            Name.parse(name)
            |> Result.bind(getAccountAndTransactions connectionString)
            |> Result.map(buildAccount)
        
        match result with
        | Ok account -> Successful.OK account next ctx
        | Error err  -> RequestErrors.BAD_REQUEST err next ctx

let depositInAccount(depositPost : IModelParser<DepositPost, Deposit>) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]

        match depositPost.Parse() with
        | Error err  -> RequestErrors.BAD_REQUEST err next ctx
        | Ok deposit ->
            let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Deposit; Amount = deposit.Amount }

            let result =
                deposit.Owner
                |> getAccountAndTransactions connectionString
                |> Result.map(buildAccount)
                |> Result.map(writeTransaction connectionString transaction)
        
            match result with
            | Ok _ -> Successful.OK "" next ctx
            | Error err  -> RequestErrors.BAD_REQUEST err next ctx

let withdrawFromAccount(withrawalPost : IModelParser<WithdrawalPost, Withdrawal>) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let config = ctx.GetService<IConfiguration>()
        let connectionString = config.["ConnectionStrings:AccountsDb"]

        match withrawalPost.Parse() with
        | Error err  -> RequestErrors.BAD_REQUEST err next ctx
        | Ok withdrawal ->
            let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Withdraw; Amount = withdrawal.Amount }

            let result =
                withdrawal.Owner
                |> getAccountAndTransactions connectionString
                |> Result.map(buildAccount)
                |> Result.bind(tryWithdraw transaction)
                |> Result.map(writeTransaction connectionString transaction)
        
            match result with
            | Ok _ -> Successful.OK "" next ctx
            | Error err  -> RequestErrors.BAD_REQUEST err next ctx