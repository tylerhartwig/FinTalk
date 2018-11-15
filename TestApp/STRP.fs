module TestApp.STRP

type ErrorHandler = ErrorHandler with
   static member ($) (_:ErrorHandler, ((result:Result< ^f, Result< ^a, ^b>>), (f: ^a -> ^c ))) =
       match result with
       | Ok x -> Ok (x.ToString())
       | Error x -> (ErrorHandler $ (x, f))

   static member ($) (_:ErrorHandler, ((result:Result< ^a, ^b >), (f: ^a -> ^c))) =
       match result with
       | Ok x -> Ok (f x)
       | Error y -> Error y

let inline doThing x = ErrorHandler $ x

let format (x:string) = x.ToString()
let formatInt (x:int) = x.ToString()

let result : Result<string,_> = doThing (Result<int, Result<string, string>>.Error(Ok "5"), format)
let result : Result<int,_> = doThing (Result<int, Result<int, string>>.Error(Ok 5), formatInt)