namespace DragonServer.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open DragonServer.Requests
open Microsoft.AspNetCore.Mvc
open DragonServer.Model.Dragon


[<Route("api/[controller]")>]
type PunchesController (punchService : IPunchService) =
    inherit Controller()
            
    [<HttpPost>]
    member this.Create([<FromBody>] createPunchRequest: CreatePunchRequest) =
        punchService.CreatePunch createPunchRequest
        
        