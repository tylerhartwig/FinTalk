module BusinessApi

type Business = {
    Name : string
}

type IBusinessApi = {
    searchRadius : string -> float -> Async<Business list>
}

