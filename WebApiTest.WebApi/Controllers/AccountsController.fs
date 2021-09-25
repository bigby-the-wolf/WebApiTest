module WebApiTest.WebApi.Controllers.AccountsController

open FSharpx.Control.AsyncExtensions
open FSharp.Control.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open System

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Domain.Operations.AccountOperations
open WebApiTest.Repositories.SqlServer.AccountRepository
open WebApiTest.WebApi.Models

//type AsyncResult<'T, 'U> = FSharpx.Control.Subject<Result<'T,'U>>

//type AsyncSwitch<'T, 'U> = 'T -> AsyncResult<'T, 'U>

//type Switch<'T, 'U> = 'T -> Result<'T, 'U>

//let convertAsync<'T, 'U> =
//    FSharpx.Control.Observable.create 


//type AsyncRailway<'T, 'U> =
//    let private sitches : 

let bindAsync<'t,'s,'terr> (binder:'t -> Async<Result<'s,'terr>>) (result:Async<Result<'t,'terr>>) : Async<Result<'s,'terr>> = 
    async {
        let! res = result
        match res with
        | Ok(value) -> return! binder value
        | Error(err) -> return Error(err)
    }

let mapAsync (mapping:('T -> 'U)) (result:Async<Result<'T,'TError>>) =
    async {
        let! res = result
        return res |> Result.map (mapping)
    }

let mapAsync1 (mapping:('T -> Async<'U>)) (result:Async<Result<'T,'TError>>) : Async<Result<'U, 'TError>> =
    async {
        let! res = result
        match res with
        | Error err -> return Error err
        | Ok value  ->
            let! r = mapping value
            return Ok r
    }

let toAsync a =
    async {
        return a 
    }

let getAccount(name:string) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let config = ctx.GetService<IConfiguration>()
            let connectionString = config.["ConnectionStrings:AccountsDb"]

            let resultAsync =
                Name.parse(name)
                |> toAsync
                |> bindAsync(fun x -> getAccountAndTransactions connectionString x)
                |> mapAsync(fun x -> buildAccount x)

            let result = resultAsync |> Async.RunSynchronously

            match result with
            | Ok account -> return! Successful.OK account next ctx
            | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx

            //match Name.parse(name) with
            //| Error err -> return! RequestErrors.BAD_REQUEST err next ctx
            //| Ok name   -> 
            //    let! accountAndTransactions = getAccountAndTransactions connectionString name
                
            //    match accountAndTransactions with
            //    | Error err -> return! RequestErrors.BAD_REQUEST err next ctx
            //    | Ok res    ->
            //        let account = buildAccount res
            //        return! Successful.OK account next ctx
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

                //let result =
                //    (Ok deposit.Owner)
                //    |>toAsync
                //    |>bindAsync(fun x -> getAccountAndTransactions connectionString x)
                //    |>mapAsync(fun x -> buildAccount x)
                //    |>mapAsync1(fun x -> writeTransaction connectionString transaction x)
                //    //|>Async.RunSynchronously

                let result =
                    (Ok deposit.Owner)
                    |>toAsync
                    |>bindAsync(fun x -> getAccountAndTransactions connectionString x)
                    |>mapAsync(fun x -> buildAccount x)
                    |>mapAsync(fun x -> writeTransaction connectionString transaction x |> Async.RunSynchronously)
                    |>Async.RunSynchronously
                    
                match result with
                | Ok _ -> return! Successful.OK "" next ctx
                | Error err  -> return! RequestErrors.BAD_REQUEST err next ctx

            //match depositPost.Parse() with
            //| Error err  -> return! RequestErrors.BAD_REQUEST err next ctx
            //| Ok deposit ->
            //    let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Deposit; Amount = deposit.Amount }

            //    let! accountAndTransactions = getAccountAndTransactions connectionString deposit.Owner

            //    match accountAndTransactions with
            //    | Error err -> return! RequestErrors.BAD_REQUEST err next ctx
            //    | Ok res    ->
            //        let account = buildAccount res

            //        do! writeTransaction connectionString transaction account

            //        return! Successful.OK "" next ctx
        }

        //let config = ctx.GetService<IConfiguration>()
        //let connectionString = config.["ConnectionStrings:AccountsDb"]

        //match depositPost.Parse() with
        //| Error err  -> RequestErrors.BAD_REQUEST err next ctx
        //| Ok deposit ->
        //    let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Deposit; Amount = deposit.Amount }
        //    let wt = writeTransaction connectionString transaction

        //    let result =
        //        deposit.Owner
        //        |> getAccountAndTransactions connectionString
        //        |> Async.map(Result.map(buildAccount))
        //        |> bindAsync(wt)
        //        |> Async.RunSynchronously
        
        //    match result with
        //    | Ok _ -> Successful.OK "" next ctx
        //    | Error err  -> RequestErrors.BAD_REQUEST err next ctx

//let withdrawFromAccount(withrawalPost : IModelParser<WithdrawalPost, Withdrawal>) : HttpHandler =
//    fun (next : HttpFunc) (ctx : HttpContext) ->
//        let config = ctx.GetService<IConfiguration>()
//        let connectionString = config.["ConnectionStrings:AccountsDb"]

//        match withrawalPost.Parse() with
//        | Error err  -> RequestErrors.BAD_REQUEST err next ctx
//        | Ok withdrawal ->
//            let transaction = { Timestamp = DateTime.UtcNow; Operation = BankOperation.Withdraw; Amount = withdrawal.Amount }

//            let result =
//                withdrawal.Owner
//                |> getAccountAndTransactions connectionString
//                |> Result.map(buildAccount)
//                |> Result.bind(tryWithdraw transaction)
//                |> Result.map(writeTransaction connectionString transaction)
        
//            match result with
//            | Ok _ -> Successful.OK "" next ctx
//            | Error err  -> RequestErrors.BAD_REQUEST err next ctx