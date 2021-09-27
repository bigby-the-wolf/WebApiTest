module WebApiTest.Infrastructure.Utils.Reader

type Reader<'env, 'a> = Reader of action:('env -> 'a)

module Reader =
    /// Run a Reader with a given environment
    let run env (Reader action)  =
        action env  // simply call the inner function

    /// Create a Reader which returns the environment itself
    let ask = Reader id

    /// Map a function over a Reader
    let map f reader =
        Reader (fun env -> f (run env reader))

    /// flatMap a function over a Reader
    let bind f reader =
        let newAction env =
            let x = run env reader
            run env (f x)
        Reader newAction

type ReaderBuilder() =
        member __.Return(x) = Reader (fun _ -> x)
        member __.Bind(x,f) = Reader.bind f x
        member __.Zero() = Reader (fun _ -> ())

let reader = ReaderBuilder()