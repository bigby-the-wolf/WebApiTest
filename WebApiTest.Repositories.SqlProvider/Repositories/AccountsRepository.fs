module WebApiTest.Repositories.SqlProvider.Repositories.AccountsRepository

open System
open FSharp.Data.Sql

open WebApiTest.Domain.Types.AccountTypes

[<AutoOpen>]
module private DB =
    //let [<Literal>] Conn = @"Data Source=(localdb)\MSSQLLocalDB;Database=BankAccountDb;Integrated Security=True;Connect Timeout=60"
    let [<Literal>] Conn = @"Data Source=localhost,11433;Initial Catalog=BankAccountDb;User ID=sa;Password=Pass123!;Connect Timeout=60"
    type AccountsDb = SqlDataProvider<ConnectionString = Conn, UseOptionTypes = true>
    let staticContext = AccountsDb.GetDataContext()
    let getContext (connectionString:string) = AccountsDb.GetDataContext connectionString
    

//[<AutoOpen>]
//module private DB =
//    let [<Literal>] Conn = @"Data Source=(localdb)\MSSQLLocalDB;Database=BankAccountDb;Integrated Security=True;Connect Timeout=60"
//    type AccountsDb = SqlProgrammabilityProvider<Conn>
//    type GetAccountId = SqlCommandProvider<"SELECT TOP 1 AccountId FROM dbo.Account WHERE Owner = @owner", Conn, SingleRow = true>
//    type FindTransactions = SqlCommandProvider<"SELECT Timestamp, OperationId, Amount FROM dbo.AccountTransaction WHERE AccountId = @accountId", Conn>
//    type FindTransactionsByOwner = SqlCommandProvider<"SELECT a.AccountId, at.Timestamp, at.OperationId, at.Amount FROM dbo.Account a LEFT JOIN dbo.AccountTransaction at on a.AccountId = at.AccountId WHERE Owner = @owner", Conn>
//    type DbOperations = SqlEnumProvider<"SELECT Description, OperationId FROM dbo.Operation", Conn>

let toBankOperation operationId =
    let operation = staticContext.Dbo.Operation
    if   (operationId = operation.Individuals.``1``) then Withdraw
    elif (operationId = operation.Individuals.``2``) then Deposit
    else failwith "Unknown operation!"

let fromBankOperation bankOperation =
    let operation = staticContext.Dbo.Operation
    match bankOperation with
        | Withdraw -> operation.Individuals.``1``
        | Deposit  -> operation.Individuals.``2``

let getAccountAndTransactions (connectionString:string) (owner:string) : (Guid * Transaction seq) option =
    let context = getContext connectionString
    let results =
        query {
            for accountTransaction in context.Dbo.AccountTransaction do
            jo
        }
    
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

//let writeTransaction (connectionString:string) (accountId:Guid) (owner:string) (transaction:Transaction) =
//    let result = GetAccountId.Create(connectionString).Execute(owner)
//    use connection = new SqlConnection(connectionString)
//    connection.Open()
//    if result.IsNone then
//        use accounts = new AccountsDb.dbo.Tables.Account()
//        accounts.AddRow(owner, accountId)
//        accounts.Update(connection) |> ignore

//    let operationId = fromBankOperation transaction.Operation
    
//    use transactions = new AccountsDb.dbo.Tables.AccountTransaction()
//    transactions.AddRow(accountId, transaction.Timestamp, operationId, transaction.Amount)
//    transactions.Update(connection) |> ignore
//    ()
let x = 100