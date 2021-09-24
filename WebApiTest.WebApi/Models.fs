module WebApiTest.WebApi.Models

open System
open Giraffe

open WebApiTest.Domain.Types.AccountTypes

type IModelParser<'From, 'To> =
    abstract member Parse : unit -> 'To

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

[<CLIMutable>]
type WithdrawalPost = 
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

    interface IModelValidation<WithdrawalPost> with
        member this.Validate() =
            this.HasErrors()
            |> Result.mapError(fun err -> RequestErrors.badRequest (text err))

    interface IModelParser<WithdrawalPost, Withdrawal> with
        member this.Parse() =
            { Owner = Name this.OwnerName; Amount = Amount (Decimal.Parse(this.Amount)) }