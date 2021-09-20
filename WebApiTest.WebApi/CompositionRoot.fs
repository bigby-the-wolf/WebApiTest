module WebApiTest.WebApi.CompositionRoot

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

open WebApiTest.Domain.Services.AccountService
open WebApiTest.Repositories.SqlServer.AccountRepository
open WebApiTest.WebApi.AppSettingsParser

let configureDependencies (configuration : IConfiguration) (services : IServiceCollection) =
    let dbConnectionString = configuration.["ConnectionStrings:AccountsDb"]
    services.AddTransient<IAccountsRepository>(fun sp -> buildAccountsRepository dbConnectionString)
    //services.AddTransient<IAccountsService>(fun sp -> buildAccountsService)

let accountsService = 
    let sqlConnectionString = appSettings.ConnectionStrings.AccountsDb
    buildAccountsService (getAccountAndTransactions sqlConnectionString) (writeTransaction sqlConnectionString)
    