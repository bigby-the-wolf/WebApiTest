module WebApiTest.Infrastructure.Utils.HelperFunctions

let flip f a b = f b a

let toAsync a =
    async {
        return a 
    }