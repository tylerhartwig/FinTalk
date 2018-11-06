#r "../packages/Microsoft.Extensions.Configuration/lib/netstandard2.0/Microsoft.Extensions.Configuration.dll"
#r "../packages/Microsoft.Extensions.Configuration.Abstractions/lib/netstandard2.0/Microsoft.Extensions.Configuration.Abstractions.dll"
#r "../packages/Microsoft.Extensions.Configuration.Json/lib/netstandard2.0/Microsoft.Extensions.Configuration.Json.dll"
#r "../packages/Microsoft.Extensions.Configuration.EnvironmentVariables/lib/netstandard2.0/Microsoft.Extensions.Configuration.EnvironmentVariables.dll"
#r "../packages/Microsoft.Extensions.Configuration.UserSecrets/lib/netstandard2.0/Microsoft.Extensions.Configuration.UserSecrets.dll"
#r "../packages/Microsoft.Extensions.FileProviders.Abstractions/lib/netstandard2.0/Microsoft.Extensions.FileProviders.Abstractions.dll"
#r "../packages/System.Threading.Tasks.Extensions/lib/netstandard2.0/System.Threading.Tasks.Extensions.dll"
#r "../packages/System.Runtime.CompilerServices.Unsafe/lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll"

#r "../packages/Npgsql/lib/net451/Npgsql.dll"
#r "../packages/FSharp.Data.Npgsql/lib/netstandard2.0/FSharp.Data.Npgsql.dll"
#r "../packages/FSharp.Data/lib/netstandard2.0/FSharp.Data.dll"

#r "Facades/netstandard"

open FSharp.Data
open FSharp.Data.Npgsql

open System.IO

type ResultBuilder() = 
    member __.Bind(m, f) = Result.bind f m 
    member __.Return(x) = Ok x 


module Result =
    let apply fResult xResult =
        match fResult, xResult with 
        | Ok f, Ok x -> Ok (f x)
        | Ok _, Error e -> Error e
        | Error e, Ok _ -> Error e
        | Error e1, Error e2 -> Error (List.append e1 e2)

    let rec traverse f values = 
        let (<*>) = apply
        let retn = Ok
        let cons head tail = Seq.append (Seq.singleton head) tail

        let (|EmptySeq|_|) s = 
            if Seq.isEmpty s then Some s else None 

        let (|HeadTail|) s = 
            (Seq.head s, Seq.tail s)        

        match values with 
        | EmptySeq _ -> retn Seq.empty
        | HeadTail(head, tail) -> 
            retn cons <*> (f head) <*> (traverse f tail)

let result = ResultBuilder()

[<Literal>]
let localConnectionString = "Host=localhost;Username=test;Password=test;Database=test"

[<Literal>]
let jsonDataFile = "/Users/tylerhartwig/FinTalkDemo/data/yelp_academic_dataset_business.json"

let readLines (filePath:string) = 
    seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do 
            yield sr.ReadLine()
    }

type Data = NpgsqlConnection<Connection = localConnectionString, Fsx = true>
// let localConnection = new Npgsql.NpgsqlConnection<Connection = localConnectionString>(localConnectionString)
type BusinessSource = FSharp.Data.JsonProvider<jsonDataFile, SampleIsList=true>

let insertCmd = 
        new NpgsqlCommand<"""
                INSERT INTO "FinTalk"."Businesses" (name, latitude, longitude, stars, num_reviews)
                VALUES(@name, @latitude, @longitude, @stars, @numReviews)
        """, localConnectionString>(localConnectionString)

let insertData (rawData:BusinessSource.Root) = 
    let unwrapOption = function 
        | Some v -> Ok v
        | None -> Error ["Expected a value, but received \"None\""]

    result { 
        let! latitude = unwrapOption rawData.Latitude
        let! longitude = unwrapOption rawData.Longitude
        return insertCmd.Execute(string rawData.Name, float latitude, float longitude, (float rawData.Stars), rawData.ReviewCount)
    }


let rowsInserted = 
    readLines jsonDataFile 
        |> Seq.map BusinessSource.Parse
        |> Seq.filter (fun x -> 
            match x.Latitude, x.Longitude with 
            | Some _, Some _ -> true
            | _ -> false)
        |> Result.traverse insertData
        |> Result.bind (Seq.reduce (+) >> Ok)

match rowsInserted with 
| Ok n -> printfn "Inserted %i rows" n 
| Error e -> printfn "Failed with: %A" e