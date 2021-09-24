namespace WebApiTest.Domain.Types.AccountTypes

open System

type BankOperation = Deposit | Withdraw

type Amount = Amount of decimal
    with member this.asDecimal = match this with (Amount amount) -> amount
type Name = Name of string
type ConnectionString = ConnectionString of string

type Deposit    = { Owner : Name; Amount : Amount }
type Withdrawal = { Owner : Name; Amount : Amount }

/// A customer of the bank.
type Customer = { Name : string }
/// An account held at the bank.
type Account = { AccountId : Guid; Owner : Customer; Balance : decimal }
/// A single transaction that has occurred.
type Transaction = { Timestamp : DateTime; Operation : BankOperation; Amount : Amount }

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