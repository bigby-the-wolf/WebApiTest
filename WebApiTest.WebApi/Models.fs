module WebApiTest.WebApi.Models

open WebApiTest.Domain.Types.AccountTypes
open WebApiTest.Infrastructure.Utils.HelperFunctions

type IModelParser<'From, 'To> =
    abstract member Parse : unit -> Result<'To, string>


[<CLIMutable>]
type DepositPost = 
    {
        OwnerName : string
        Amount : string
    }

    interface IModelParser<DepositPost, Deposit> with
        member this.Parse() =
            Name.parse(this.OwnerName)
            |> Result.map(Deposit.createDeposit)
            |> Result.bind(flip Result.map (Amount.parse(this.Amount)))
            

[<CLIMutable>]
type WithdrawalPost = 
    {
        OwnerName : string
        Amount : string
    }

    interface IModelParser<WithdrawalPost, Withdrawal> with
        member this.Parse() =
            Name.parse(this.OwnerName)
            |> Result.map(Withdrawal.createWithdrawal)
            |> Result.bind(flip Result.map (Amount.parse(this.Amount)))