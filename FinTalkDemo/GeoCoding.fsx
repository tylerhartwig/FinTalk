// #load "../packages/SwaggerProvider/SwaggerProvider.fsx"

// open SwaggerProvider

// let [<Literal>] swaggerSchema = __SOURCE_DIRECTORY__ + "/Geocoding/opencage.yaml"
// let key = "2ce68dfa781549c3afd47ae64a6e4308"

// type GeoCoding = SwaggerProvider<swaggerSchema>

// let geoCoding = GeoCoding()

// let result = geoCoding.GetVVersion(1L, "json", "Dallas, TX", key)


// let location = result.Results |> Array.head

// printfn "Lat: %A" location.Geometry.Lat
// printfn "Lng: %A" location.Geometry.Lng


#r "../packages/FSharp.Data/lib/netstandard2.0/FSharp.Data.dll"
#r "Facades/netstandard"

open FSharp.Data
open FSharp.Data

let key = "2ce68dfa781549c3afd47ae64a6e4308"
let data =
    FSharp.Data.Http.RequestString(
        (sprintf "https://api.opencagedata.com/geocode/v1/json?q=%s&key=%s&pretty=1" "Dallas" key)
    )

let parsed =
    FSharp.Data.JsonValue.Parse(data)

let getRecordValue str = function 
    | FSharp.Data.JsonValue.Record r -> r |> Array.find (fun (name, _) -> name = str) |> snd
    | _ -> failwith "Expected a record, but JsonValue was not a record"

let getArrayValue idx = function 
    | FSharp.Data.JsonValue.Array a -> a.[idx]
    | _ -> failwith "Expected an array, but JsonValue was not an array"

let getNumberValue = function 
    | FSharp.Data.JsonValue.Number n -> n 
    | _ -> failwith "Expected a number, but JsonValue was not a number"

let geometry = parsed |> getRecordValue "results" |> getArrayValue 0 |> getRecordValue "geometry"
geometry

let lat = geometry |> getRecordValue "lat" |> getNumberValue
let lng = geometry |> getRecordValue "lng" |> getNumberValue