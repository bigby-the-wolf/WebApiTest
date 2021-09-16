module WebApiTest.WebApi.CompositionRoot

open WebApiTest.Domain.Services.AccountService
open WebApiTest.Repositories.SqlProvider.Repositories.AccountsRepository
open WebApiTest.Repositories.
open WebApiTest.WebApi.AppSettingsParser

let y = x + 100 + z

//let accountsService = 
//    let sqlConnectionString = appSettings.ConnectionStrings.AccountsDb
//    buildAccountsService (AccountRepository.getAccountAndTransactions sqlConnectionString) (AccountRepository.writeTransaction sqlConnectionString)