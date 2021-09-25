namespace WebApiTest.Domain.Types.AccountTypes

open System

type BankOperation = Deposit | Withdraw

module Amount =

    type _T = Amount of decimal

    let parse (s:string) =
        match Decimal.TryParse(s) with
        | (false, _) -> Error "Amount must be decimal!"
        | (_, amount) when amount <= 0M -> Error "Amount must be greather than 0!"
        | (_, amount) -> Ok (Amount amount)

    // unwrap with continuation
    let apply f (Amount a) = f a

    // unwrap directly
    let value a = apply id a

module Name =

    type _T = Name of string

    let parse (s:string) =
        if String.IsNullOrWhiteSpace(s) 
        then Error "Name can't be empty!"
        else Ok (Name s)

    // unwrap with continuation
    let apply f (Name n) = f n

    // unwrap directly
    let value n = apply id n

type ConnectionString = ConnectionString of string

type Deposit    = 
    { 
        Owner : Name._T
        Amount : Amount._T 
    }
    with static member createDeposit o a = { Owner = o; Amount = a}

type Withdrawal = 
    { 
        Owner : Name._T
        Amount : Amount._T 
    }
    with static member createWithdrawal o a = { Owner = o; Amount = a}

/// A customer of the bank.
type Customer = { Name : string }
/// An account held at the bank.
type Account = { AccountId : Guid; Owner : Customer; Balance : decimal }
/// A single transaction that has occurred.
type Transaction = { Timestamp : DateTime; Operation : BankOperation; Amount : Amount._T }

/// Represents a bank account that is known to be in credit.
type CreditAccount = CreditAccount of Account
/// A bank account which can either be in credit or overdrawn.
type RatedAccount =
    /// Represents an account that is known to be in credit.
    | InCredit of Account:CreditAccount
    /// Represents an account that is known to be overdrawn.
    | Overdrawn of Account:Account
    member internal this.GetField getter =
        match this with
        | InCredit (CreditAccount account) -> getter account
        | Overdrawn account -> getter account
    /// Gets the current balance of the account.
    member this.Balance = this.GetField(fun a -> a.Balance)
    member this.Id = this.GetField(fun a -> a.AccountId)