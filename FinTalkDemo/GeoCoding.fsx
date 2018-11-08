#load "../packages/SwaggerProvider/SwaggerProvider.fsx"

open SwaggerProvider

let [<Literal>] swaggerSchema = __SOURCE_DIRECTORY__ + "/Geocoding/opencage.yaml"
let key = "2ce68dfa781549c3afd47ae64a6e4308"

type GeoCoding = SwaggerProvider<swaggerSchema>

let geoCoding = GeoCoding()

let result = geoCoding.GetVVersion(1L, "json", "Dallas, TX", key)


let location = result.Results |> Array.head

printfn "Lat: %A" location.Geometry.Lat
printfn "Lng: %A" location.Geometry.Lng
