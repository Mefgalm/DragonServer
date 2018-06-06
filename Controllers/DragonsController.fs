namespace DragonServer.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open DragonServer.Model.Dragon
open Microsoft.AspNetCore.Mvc


[<Route("api/[controller]")>]
type DragonsController (dragonService : IDragonService) =
    inherit Controller()
    
    [<HttpGet>]
    member this.Get() =
        dragonService.GetList
        
    [<HttpPost>]    
    member this.Create() =
        dragonService.CreateDragon
        
    [<HttpGet("{id}")>]    
    member this.Get(id: Guid) =    
        dragonService.Get id
        
    [<HttpGet("filter")>]    
    member this.Get(filter, hitPoints, isAlive) =
        dragonService.GetFilterList filter hitPoints isAlive
        
    [<HttpGet("{id}/punches")>]    
    member this.GetPunches(id) =
        dragonService.GetPunchesByDragon id    