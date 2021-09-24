module WebApiTest.WebApi.Models

open WebApiTest.Domain.Types.AccountTypes

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
            |> Result.bind(fun f -> Amount.parse(this.Amount) |> Result.map f)
            

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
            |> Result.bind(fun f -> Amount.parse(this.Amount) |> Result.map f)