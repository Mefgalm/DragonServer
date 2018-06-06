namespace DragonServer.Requests

open System
open System.Text.RegularExpressions
open DragonServer.Model

type CreatePunchRequest = {
    heroId: Guid
    dragonId: Guid
}

type CreateHeroRequest = {
    confirmPassword: string
    password: string
    name: string
}

type LoginHeroRequest = {
    name: string
    password: string
}