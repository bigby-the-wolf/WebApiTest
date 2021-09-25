module WebApiTest.Repositories.SqlServer.AccountRepository

open WebApiTest.Domain.Types.AccountTypes
open FSharp.Data
open System
open System.Data.SqlClient

[<AutoOpen>]
module private DB =
    let [<Literal>] Conn = @"Data Source=(localdb)\MSSQLLocalDB;Database=BankAccountDb;Integrated Security=True;Connect Timeout=60"
    //let [<Literal>] Conn = @"Data Source=localhost,11433;Initial Catalog=BankAccountDb;User ID=sa;Password=Pass123!;Connect Timeout=60"
    type AccountsDb = SqlProgrammabilityProvider<Conn>
    type GetAccountId = SqlCommandProvider<"SELECT TOP 1 AccountId FROM dbo.Account WHERE Owner = @owner", Conn, SingleRow = true>
    type FindTransactions = SqlCommandProvider<"SELECT Timestamp, OperationId, Amount FROM dbo.AccountTransaction WHERE AccountId = @accountId", Conn>
    type FindTransactionsByOwner = SqlCommandProvider<"SELECT a.AccountId, at.Timestamp, at.OperationId, at.Amount FROM dbo.Account a LEFT JOIN dbo.AccountTransaction at on a.AccountId = at.AccountId WHERE Owner = @owner", Conn>
    type DbOperations = SqlEnumProvider<"SELECT Description, OperationId FROM dbo.Operation", Conn>

let toBankOperation operationId =
    match operationId with
        | DbOperations.Deposit -> Deposit
        | DbOperations.Withdraw -> Withdraw
        | _ -> failwith "Unknown operation!"

let fromBankOperation bankOperation =
    match bankOperation with
        | Deposit -> DbOperations.Deposit
        | Withdraw -> DbOperations.Withdraw

let getAccountAndTransactions (connectionString:string) (name:Name._T) : Result<(Guid * string * Transaction seq), string> =
    let results = 
        name
        |> Name.apply (FindTransactionsByOwner.Create(connectionString).Execute)
        |> List.ofSeq

    let name = Name.value name
    
    match results with
    | [] -> Error "Owner not found"
    | [ row ] when row.Amount.IsNone -> Ok (row.AccountId, name, Seq.empty)
    | (firstRow :: _) as results ->
        let accountId = firstRow.AccountId
        let transactions =
            results
            |> List.choose (fun r -> 
                match r.Amount, r.Timestamp, r.OperationId with
                    | Some amount, Some timestamp, Some operationId -> Some { Timestamp = timestamp; Amount = Amount.Amount amount; Operation = toBankOperation operationId }
                    | _ -> None)
            |> List.toSeq
        Ok (accountId, name, transactions)

let writeTransaction (connectionString:string) (transaction:Transaction) (account:RatedAccount) =
    let operationId = fromBankOperation transaction.Operation
    let amount = Amount.value transaction.Amount
    
    use connection = new SqlConnection(connectionString)
    use transactions = new AccountsDb.dbo.Tables.AccountTransaction()
    
    transactions.AddRow(account.Id, transaction.Timestamp, operationId, amount)
    transactions.Update(connection) |> ignore
    