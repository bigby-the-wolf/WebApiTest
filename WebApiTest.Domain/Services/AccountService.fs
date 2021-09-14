/// Provides access to the banking API.
module WebApiTest.Domain.Services.AccountService

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Domain.Operations.AccountOperations
open System

/// Loads an account from disk. If no account exists, an empty one is automatically created.
let private loadAccount getAccountAndTransactions customer =
    customer.Name
    |> getAccountAndTransactions
    |> Option.map (buildAccount customer.Name)
    |> defaultArg <|
        InCredit(CreditAccount { AccountId = Guid.NewGuid()
                                 Balance = 0M
                                 Owner = customer })
/// Deposits funds into an account.
let private deposit loadAccount writeTranscation amount customer =
    let (ratedAccount: RatedAccount) = loadAccount customer
    let accountId = ratedAccount.GetField (fun a -> a.AccountId)
    let owner = ratedAccount.GetField(fun a -> a.Owner)
    auditAs Deposit writeTranscation deposit amount ratedAccount accountId owner

/// Withdraws funds from an account that is in credit.
let private withdraw loadAccount writeTranscation amount customer =
    let account = loadAccount customer
    match account with
    | InCredit (CreditAccount account as creditAccount) -> auditAs Withdraw writeTranscation withdraw amount creditAccount account.AccountId account.Owner
    | account -> account

/// Loads the transaction history for an owner.
let private loadTransactionHistory getAccountAndTransactions customer =
    customer.Name
    |> getAccountAndTransactions
    |> Option.map(fun (_,txns) -> txns)
    |> defaultArg <| Seq.empty

type IAccountsService =
    abstract member LoadAccount : customer:Customer -> RatedAccount
    abstract member Deposit : amount:Decimal -> customer:Customer -> RatedAccount
    abstract member Withdraw : amount:Decimal -> customer:Customer -> RatedAccount
    abstract member LoadTransactionHistory : customer:Customer -> Transaction seq

let buildAccountsService loadAccountHistory saveTransaction =
    { new IAccountsService with
        member this.LoadAccount(customer) =  loadAccount loadAccountHistory customer
        member this.Deposit amount customer = deposit this.LoadAccount saveTransaction amount customer
        member this.Withdraw amount customer = withdraw this.LoadAccount saveTransaction amount customer
        member this.LoadTransactionHistory(customer) = loadTransactionHistory loadAccountHistory customer }