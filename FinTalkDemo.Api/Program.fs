namespace FinTalkDemo.Api

open BusinessApi
open Fable.Remoting
open Fable.Remoting.AspNetCore
open Fable.Remoting.Server
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

module Program =

    let exitCode = 0
    
    let webApp =
        Remoting.createApi()
        |> Remoting.withRouteBuilder (sprintf "/api/%s/%s")
        |> Remoting.fromValue businessApi
        
    let configureApp (app : IApplicationBuilder) =
        app.UseRemoting webApp

    let CreateWebHostBuilder args =
        WebHost
            .CreateDefaultBuilder(args)
//            .UseStartup<Startup>();

    [<EntryPoint>]
    let main args =
//        CreateWebHostBuilder(args)
        WebHostBuilder()
            .UseKestrel()
            .Configure(Action<IApplicationBuilder> configureApp)
            .Build()
            .Run()

        exitCode
