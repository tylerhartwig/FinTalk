module BusinessApi

type Business = {
    Name : string
    Rating : float
    NumReviews : int
}

type IBusinessApi = {
    searchRadius : string -> float -> Async<Business list>
}

