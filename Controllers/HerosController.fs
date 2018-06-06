namespace DragonServer.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open DragonServer.Model.Dragon
open Microsoft.AspNetCore.Mvc
open DragonServer.Requests


[<Route("api/[controller]")>]
type HeroesController (heroService : IHeroService) =
    inherit Controller()
    
    [<HttpGet>]
    member this.Get() =
        heroService.GetList    
            
    [<HttpPost>]
    member this.Create([<FromBody>] value: CreateHeroRequest) =
        heroService.CreateHero value
        
    [<HttpGet("{id}/punches")>]    
    member this.GetPunches(id) =
        heroService.GetPunchesByHero id

    [<HttpPost("test")>]
    member this.CreateTest([<FromBody>]heroRequest: CreateHeroRequest) =
        "test"