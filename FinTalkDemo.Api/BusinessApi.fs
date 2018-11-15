module FinTalkDemo.Api.BusinessApi

open BusinessApi
open FSharp.Data
open FSharp.Data.Npgsql
open OpenAPITypeProvider

let [<Literal>] postgresConnectionString = "Host=localhost;Username=test;Password=test;Database=test"
type BusinessData = NpgsqlConnection<postgresConnectionString>

let meterPerMile = 1609.344

type radiusCmd = 
    NpgsqlCommand<"""
        SET SEARCH_PATH TO public, "FinTalk";
        SELECT * FROM "FinTalk"."Businesses"
            WHERE public.earth_box(public.ll_to_earth(@latitude, @longitude), @radius) @> public.ll_to_earth(latitude, longitude);
    """, postgresConnectionString>
        
let radiusSearch radius lat long =
    async {
        use cmd = radiusCmd.Create(postgresConnectionString)
        let! results = cmd.AsyncExecute(lat, long, meterPerMile * radius)
        let mapRecord (record : radiusCmd.Record) =
            {
                Name = record.name
            }
            
        return results |> Seq.map mapRecord |> Seq.toList
    }

let [<Literal>] swaggerSchema = __SOURCE_DIRECTORY__ + "/Geocoding/opencage.yaml"
let [<Literal>] openApiSchema = __SOURCE_DIRECTORY__ + "/Geocoding/OpenApiOpenCage.yaml"
let key = "2ce68dfa781549c3afd47ae64a6e4308"

type GeoCoding = OpenAPIV3Provider<openApiSchema>
let geoCoding = new GeoCoding()



let geoCodingSearch term =
    let json =
        Http.RequestString("https://api.opencagedata.com/geocode/v1/json",
            query = ["q", term; "key", key])
    let response = GeoCoding.Schemas.Response.Parse json
    
    async { 
        match response.Results with 
        | None -> return None
        | Some [] -> 
            return None
        | Some(head::tail) -> 
            return head.Geometry 
                |> Option.map (fun g -> (g.Lat, g.Lng) ||> Option.map2 (fun lat long -> lat, long)) 
                |> Option.flatten
    }

let searchBusinesses searchTerm radius = 
    async {
        let! latLong = geoCodingSearch searchTerm
        match latLong with 
        | Some (lat, long) -> 
            let! results = radiusSearch radius (float lat) (float long)
            return results
        | None -> return List.empty
    }



let businessApi : IBusinessApi = {
    searchRadius = searchBusinesses
}
