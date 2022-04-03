# F# WebApiTest

Web API written in F# that showcases the language's strengths via a functional style.

## Background

As a .NET developer with experience in C# and OOP I wanted to take a plunge in the functional world. After learning Haskell and F# I wanted to see how an enterprise level API solution would be structured in a functional setting.

I've read multiple posts from both Mark Seemann's [blog](https://blog.ploeh.dk/about/) and Scott Wlaschin's [blog](https://fsharpforfunandprofit.com/about/) to further this goal and pick up best practices and principles.

## Goal

The focus is creating a sustainable software solution using .NET and F#.

## Technologies and Tools

The solution is built on .NET 5 in VS2019.

For DB access I chose [FSharp.Data.SqlClient](https://fsprojects.github.io/FSharp.Data.SqlClient/) as it's a type provider and showcases the language's strengths.

[Giraffe](https://github.com/giraffe-fsharp/Giraffe) is used on top of ASP.NET for it's funtional first features.

For package management I selected [Paket](https://fsprojects.github.io/Paket/).

## Concepts

- Railway Oriented Programming (see [this](https://fsharpforfunandprofit.com/rop/))
- Dependency Rejection (see [this](https://blog.ploeh.dk/2017/02/02/dependency-rejection/) and [this](https://fsharpforfunandprofit.com/posts/dependencies/))
- Parse don't validate (see [this](https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/))
- Async
