namespace Shared

open System
open MongoDB.Bson
open MongoDB.Driver

module Types =

    type Account =
        { _id: BsonObjectId
          Username: string
          Password: string
          Email: string
          Pinned: string seq
          SessionId: string
          SessionExpiration: string }

    type AccountSession =
        { Username: string
          Pinned: string list
          SessionId: string
          SessionExpiration: string }

    [<CLIMutable>]
    type SignupPayload =
        { Username: string
          Password: string
          Email: string
          SignUpCode: string }

    [<CLIMutable>]
    type LoginPayload = { Username: string; Password: string }

    [<CLIMutable>]
    type RequestPayload = { Username: string; SessionId: string }

    [<CLIMutable>]
    type PinnedRequestPayload =
        { Username: string
          SessionId: string
          Ticker: string }

    type PinnedResponsePayload = { Pinned: string list }

    type SummaryResponse = { Open: float; Close: float }

    type StockDataResponse = {
      ClosePrices: float array
      Dates: DateTime array
    }

    type RequestResult = 
      | ErrorEvent
      | SucessEvent

    type ApplicationUser =
      | Anonymous
      | LoggedIn of AccountSession
