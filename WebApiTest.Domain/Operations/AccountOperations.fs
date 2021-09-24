module WebApiTest.Domain.Operations.AccountOperations

open System
open WebApiTest.Domain.Types.AccountTypes

let private classifyAccount account =
    if account.Balance >= 0M then (InCredit(CreditAccount account))
    else Overdrawn account

/// Withdraws an amount of an account (if there are sufficient funds)
let withdraw amount (CreditAccount account) =
    { account with Balance = account.Balance - amount }
    |> classifyAccount

/// Deposits an amount into an account
let deposit amount account =
    let account =
        match account with
        | Overdrawn account -> account
        | InCredit (CreditAccount account) -> account
    { account with Balance = account.Balance + amount }
    |> classifyAccount

/// Runs some account operation such as withdraw or deposit with auditing.
let auditAs bankOperation audit operation amount account accountId owner =
    let updatedAccount = operation amount account
    let transaction = { Operation = bankOperation; Amount = amount; Timestamp = DateTime.UtcNow }
    audit accountId owner.Name transaction
    updatedAccount

/// Creates an account from a historical set of transactions
let buildAccount (accountId, owner, transactions) =
    let openingAccount = classifyAccount { AccountId = accountId; Balance = 0M; Owner = { Name = owner } }

    transactions
    |> Seq.sortBy(fun txn -> txn.Timestamp)
    |> Seq.fold(fun account txn ->
        match txn.Operation, account with
        | Deposit, _ -> account |> deposit txn.Amount.asDecimal
        | Withdraw, InCredit account -> account |> withdraw txn.Amount.asDecimal
        | Withdraw, Overdrawn _ -> account) openingAccount

let tryWithdraw transaction (account:RatedAccount) =
    match transaction.Operation with
    | Deposit -> Ok account
    | Withdraw ->
        if account.Balance <= 0M then Error "Can't withdraw when balance is bellow 0!"
        else Ok account