module WebApiTest.Repositories.SqlServer.AccountRepository

open WebApiTest.Domain.Services.AccountService
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
    type DbOperations = SqlEnumProvider<"SELECT Description, OperationId FROM dbo.Operation ORDER BY OperationId", Conn>

let toBankOperation operationId =
    match operationId with
        | DbOperations.Deposit  -> Deposit
        | DbOperations.Withdraw -> Withdraw
        | _ -> failwith "Unknown operation!"

let fromBankOperation bankOperation =
    match bankOperation with
        | Deposit  -> DbOperations.Deposit
        | Withdraw -> DbOperations.Withdraw

let getAccountAndTransactions (connectionString:string) (owner:string) : (Guid * Transaction seq) option =
    let results = FindTransactionsByOwner.Create(connectionString).Execute(owner)
                    |> List.ofSeq
    
    match results with
    | [] -> None
    | [ row ] when row.Amount.IsNone -> Some (row.AccountId, Seq.empty)
    | (firstRow :: _) as results ->
        let accountId = firstRow.AccountId
        let transactions =
            results
            |> List.choose (fun r -> 
                match r.Amount, r.Timestamp, r.OperationId with
                    | Some amount, Some timestamp, Some operationId -> Some { Timestamp = timestamp; Amount = amount; Operation = toBankOperation operationId }
                    | _ -> None)
            |> List.toSeq
        Some (accountId, transactions)

let writeTransaction (connectionString:string) (accountId:Guid) (owner:string) (transaction:Transaction) =
    let result = GetAccountId.Create(connectionString).Execute(owner)
    use connection = new SqlConnection(connectionString)
    connection.Open()
    if result.IsNone then
        use accounts = new AccountsDb.dbo.Tables.Account()
        accounts.AddRow(owner, accountId)
        accounts.Update(connection) |> ignore

    let operationId = fromBankOperation transaction.Operation
    
    use transactions = new AccountsDb.dbo.Tables.AccountTransaction()
    transactions.AddRow(accountId, transaction.Timestamp, operationId, transaction.Amount)
    transactions.Update(connection) |> ignore
    ()

let buildAccountsRepository connectionString =
    { new IAccountsRepository with
        member this.GetAccountAndTransactions(owner) =  getAccountAndTransactions connectionString owner
        member this.WriteTransaction accountId owner transaction = writeTransaction connectionString accountId owner transaction }
