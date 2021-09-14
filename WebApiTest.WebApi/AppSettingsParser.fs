module WebApiTest.WebApi.AppSettingsParser

open FSharp.Data
open System.IO
open System.Reflection

type AppSettings = JsonProvider<"appsettings.json">

let appSettings =
    let path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let file = path + "/appsettings.json"
    AppSettings.Load(file)