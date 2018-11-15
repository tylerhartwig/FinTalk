module FinTalkDemo.App

open BusinessApi
open Elmish 
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Remoting.Client

let businessesApi =
    Remoting.createApi()
        |> Remoting.withRouteBuilder (sprintf "/api/%s/%s")
        |> Remoting.buildProxy<IBusinessApi>

// Model
type Model = { 
    searchTerm: string 
    businesses: Business list
}

type Msg = 
    | UpdateSearchTerm of string
    | UpdateBusinesses of Business list
    | Search 
    
let init() = { searchTerm = ""; businesses = List.Empty }, Cmd.none


// Update
let update msg model = 
    match msg with 
    | UpdateSearchTerm newSearchTerm -> { model with searchTerm = newSearchTerm }, Cmd.none
    | UpdateBusinesses businesses -> { model with businesses = businesses }, Cmd.none
    | Search ->
        let cmd =
            Cmd.ofAsync
                (fun () -> async { return! businessesApi.searchRadius model.searchTerm 50. } )
                ()
                (fun businesses -> UpdateBusinesses businesses)
                (fun err -> UpdateBusinesses List.empty)
        model, cmd
        
    
let mapBusinessView (business:Business) = 
    li [] [ str business.Name ]
    
// View 
let view model dispatch =
    div [] 
        [ 
            div [] [
                input [ OnChange (fun v -> UpdateSearchTerm v.Value |> dispatch) ]
                button [ OnClick (fun _ -> Search |> dispatch) ] [ str "Search" ] ]
            div [] [ ul [] (model.businesses |> List.map mapBusinessView) ]
        ]

        
          
Program.mkProgram init update view 
    |> Program.withReact "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run
