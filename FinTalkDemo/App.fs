module FinTalkDemo.App

open Elmish 
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props



// Model 

type Model = int

type Msg = 
    | Increment
    | Decrement
    
let init() : Model = 0



// Update


let update msg model = 
    match msg with 
    | Increment -> model + 1
    | Decrement -> model - 1
    
    
// View 

let view model dispatch =
    div [] 
        [ button [ OnClick (fun _ -> dispatch Increment) ] [ str "+" ] 
          div [] [ str (string model) ]
          button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-" ] ]
          


Program.mkSimple init update view 
    |> Program.withReact "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run
