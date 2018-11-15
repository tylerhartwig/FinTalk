// Learn more about F# at http://fsharp.org

open System


open Npgsql

let createNpgsqlConnection str =
    new NpgsqlConnection(str)

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
let localConnectionString = "Host=localhost;Username=test;Password=test;Database=test;"

[<Literal>]
let jsonDataFile = "/Users/tylerhartwig/FinTalkDemo/data/yelp_academic_dataset_business.json"

[<Literal>]
let sampleJson = "/Users/tylerhartwig/FinTalkDemo/data/sample.json"

let readLines (filePath:string) = 
    seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do 
            yield sr.ReadLine()
    }
    

type Data = NpgsqlConnection<localConnectionString>
type BusinessSource = FSharp.Data.JsonProvider<sampleJson, SampleIsList=true>

//
//readLines jsonDataFile
//    |> Seq.mapi (fun i s -> BusinessSource.Parse s)
//    |> Seq.filter (fun x -> 
//        match x.Latitude, x.Longitude with 
//        | Some _, Some _ -> true
//        | _ -> false)
//    |> Seq.map (fun b -> string b.Name)
//    |> Seq.iter(fun s -> printfn "%s" s)

    
[<EntryPoint>]
let main argv =
//    let nextBusinessId conn tx =
//        use cmd = Data.CreateCommand<"""
//            SELECT NEXTVAL(pg_get_serial_sequence('"FinTalk"."Businesses"', 'id'));
//        """, SingleRow = true, XCtor = true>(conn, tx)
//        let value = cmd.Execute () |> Option.flatten |> Option.map int
//        printfn "%A" value
//       value

    
    let unwrapOption = function 
        | Some v -> Ok v
        | None -> Error ["Expected a value, but received \"None\""]
        
    let addRow conn (businesses:Data.FinTalk.Tables.Businesses) (idx, rawData:BusinessSource.Root) =
        result { 
            let! latitude = unwrapOption rawData.Latitude
            let! longitude = unwrapOption rawData.Longitude
            return businesses.AddRow(
                    Some idx,
                    name = string rawData.Name,
                    latitude = float latitude,
                    longitude = float longitude,
                    stars = float rawData.Stars,
                    num_reviews = rawData.ReviewCount,
                    neighborhood = rawData.Neighborhood,
                    address = rawData.Address,
                    city = Some rawData.City,
                    state = Some rawData.State,
                    postal_code = Some (string rawData.PostalCode))
        }

    let bulkInsert data connection =
        let businesses = new Data.FinTalk.Tables.Businesses()
        result {
            let! result = data |> Result.traverse (addRow connection businesses)
            businesses.BinaryImport(connection)
            return result
        }

    let rowsInserted =
        readLines jsonDataFile 
            |> Seq.map BusinessSource.Parse
            |> Seq.filter (fun x -> 
                match x.Latitude, x.Longitude with 
                | Some _, Some _ -> true
                | _ -> false)
            |> Seq.zip (Seq.initInfinite id)
            |> Seq.chunkBySize 500
            |> Result.traverse (fun s ->
                printfn "Inserting ~500 records"
                use conn = createNpgsqlConnection localConnectionString
                conn.Open()
                bulkInsert s conn)

    match rowsInserted with 
    | Ok n -> printfn "Inserted rows successfully" 
    | Error e -> printfn "Failed with: %A" e 
            
    printfn "Hello World from F#!"
    0 // return an integer exit code
