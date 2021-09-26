namespace WebApiTest.Infrastructure.Utils

module ResultExtensions =
    type Microsoft.FSharp.Core.Result<'T, 'TError> with
        static member bindAsync (binder:'T -> Async<Result<'U,'TError>>) (result:Async<Result<'T,'TError>>) : Async<Result<'U,'TError>> = 
            async {
                let! res = result
                match res with
                | Ok(value) -> return! binder value
                | Error(err) -> return Error(err)
            }
        
        static member mapAsync (mapping:('T -> 'U)) (result:Async<Result<'T,'TError>>) =
            async {
                let! res = result
                return res |> Result.map (mapping)
            }
