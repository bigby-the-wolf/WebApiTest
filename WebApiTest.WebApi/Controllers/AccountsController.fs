module WebApiTest.WebApi.Controllers.AccountsController

open FSharp.Control.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open System

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Domain.Operations.AccountOperations
open WebApiTest.Infrastructure.Utils.HelperFunctions
open WebApiTest.Infrastructure.Utils.ResultExtensions
open WebApiTest.Repositories.SqlServer.AccountRepository
open WebApiTest.WebApi.Models

let getAccount(name:string) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let config = ctx.GetService<IConfiguration>()
            let connectionString = config.["ConnectionStrings:AccountsDb"]

            let result =
                Name.parse(name)
                |> toAsync
                |> Result.bindAsync(getAccountAndTransactions connectionString)
                |> Result.mapAsync(buildAccount)
                |> Async.RunSynchronously

            match result with
            | Ok account -> return! Successful.OK account next ctx
            | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx
        }

let depositInAccount(depositPost : IModelParser<DepositPost, Deposit>) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let config = ctx.GetService<IConfiguration>()
            let connectionString = config.["ConnectionStrings:AccountsDb"]

            match depositPost.Parse() with
            | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx
            | Ok deposit ->
                let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Deposit; Amount = deposit.Amount }

                let result =
                    deposit.Owner
                    |> Ok
                    |> toAsync
                    |> Result.bindAsync(getAccountAndTransactions connectionString)
                    |> Result.mapAsync(buildAccount)
                    |> Result.mapAsync(writeTransaction connectionString transaction >> Async.RunSynchronously)
                    |> Async.RunSynchronously
                    
                match result with
                | Ok _ -> return! Successful.OK "" next ctx
                | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx
        }

let withdrawFromAccount(withrawalPost : IModelParser<WithdrawalPost, Withdrawal>) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let config = ctx.GetService<IConfiguration>()
            let connectionString = config.["ConnectionStrings:AccountsDb"]

            match withrawalPost.Parse() with
            | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx
            | Ok withdrawal ->
                let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Withdraw; Amount = withdrawal.Amount }

                let result =
                    withdrawal.Owner
                    |> Ok
                    |> toAsync
                    |> Result.bindAsync(getAccountAndTransactions connectionString)
                    |> Result.mapAsync(buildAccount)
                    |> Result.bindAsync(tryWithdraw transaction >> toAsync)
                    |> Result.mapAsync(writeTransaction connectionString transaction >> Async.RunSynchronously)
                    |> Async.RunSynchronously
            
                match result with
                | Ok _ -> return! Successful.OK "" next ctx
                | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx
        }
        