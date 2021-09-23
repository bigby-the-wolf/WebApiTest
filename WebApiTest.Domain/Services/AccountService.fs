/// Provides access to the banking API.
module WebApiTest.Domain.Services.AccountService

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Domain.Operations.AccountOperations
open System

/// Withdraws funds from an account that is in credit.
let withdraw loadAccount writeTranscation amount customer =
    let account = loadAccount customer
    match account with
    | InCredit (CreditAccount account as creditAccount) -> auditAs Withdraw writeTranscation withdraw amount creditAccount account.AccountId account.Owner
    | account -> account

/// Loads the transaction history for an owner.
let loadTransactionHistory getAccountAndTransactions customer =
    customer.Name
    |> getAccountAndTransactions
    |> Option.map(fun (_,txns) -> txns)
    |> defaultArg <| Seq.empty

//type IAccountsService =
//    abstract member LoadAccount : customer:Customer -> RatedAccount
//    abstract member Deposit : amount:Decimal -> customer:Customer -> RatedAccount
//    abstract member Withdraw : amount:Decimal -> customer:Customer -> RatedAccount
//    abstract member LoadTransactionHistory : customer:Customer -> Transaction seq

//let buildAccountsService loadAccountHistory saveTransaction =
//    { new IAccountsService with
//        member this.LoadAccount(customer) =  loadAccount loadAccountHistory customer
//        member this.Deposit amount customer = deposit this.LoadAccount saveTransaction amount customer
//        member this.Withdraw amount customer = withdraw this.LoadAccount saveTransaction amount customer
//        member this.LoadTransactionHistory(customer) = loadTransactionHistory loadAccountHistory customer }