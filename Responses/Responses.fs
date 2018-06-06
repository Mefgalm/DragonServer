namespace DragonServer.Responses
open System

type LoginHeroResponse = {
    token: string
    expired: DateTime
}