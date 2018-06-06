// Learn more about F# at http://fsharp.org
// 1d3
// 1d6 + attackModifier
// 3d20
// return an integer exit code
namespace DragonServer.Model

module Dragon = 
    open DragonServer.Model
    open DragonServer.Requests
    open DragonServer.Responses    
    open System
    open System.Text.RegularExpressions    
             
    let (<?>) r1 r2 =
        match r1, r2 with 
        | Some l1, Some l2 -> Some (l1 @ l2)
        | Some l1, None -> Some l1
        | None, Some l2 -> Some l2
        | _ -> None
    
    let (?==) errors success =
        match errors with
        | Some x -> success
        | _ -> Fail []
    
    let bind f1 f2 = 
        match f2 with
        | Ok v -> f1 v
        | Fail l -> Fail l
    
    let (>>=) f1 f2 = bind f2 f1
    
    [<AllowNullLiteral>]
    type IEntity = 
        abstract GetId : unit -> Guid
    
    module StringUtils = 
        let upperFirst strSeq = 
            match strSeq with
            | [] -> String.Empty
            | x :: xs -> ((Char.ToUpper x) :: xs) |> String.Concat
    
    module Generator = 
        let heroAttackModifier = (System.Random()).Next(1, 4) //1d3
        let punchAttack = (System.Random().Next(1, 6)) + (System.Random().Next(1, 6))
        let dragonMaxHitpoints = (System.Random()).Next(60, 100)
        let idGenerator = Guid.NewGuid()
        
        let rangeGenerator fromRange toRange = 
            seq { 
                for i in fromRange..toRange do
                    yield i
            }
        
        let nameGenerator length = 
            let chars = 
                rangeGenerator ('a' |> int) ('z' |> int)
                |> Seq.map (char)
                |> Seq.toArray
            
            let random = new Random()
            seq { 
                for i in 0..length do
                    yield chars.[random.Next(chars.Length)]
            }
            |> Seq.toList
            |> StringUtils.upperFirst
    
    
    [<AllowNullLiteral>]
    type Hero(name : string, passwordHash: byte[], passwordSalt: byte[], attackModifier : int) = 
        let _id = Guid.NewGuid()
        let mutable _name = name
        
        interface IEntity with
            member this.GetId() = _id
        
        member this.Id = (this :> IEntity).GetId()
        member this.AttackModifier = attackModifier
        
        member this.PasswordHash with get() = passwordHash
        member this.PasswordSalt with get() = passwordSalt
        
        member this.Name 
            with get () = _name
            and set (value) = _name <- value
        
        override this.ToString() = 
            String.Format("Hero id:{0} aliasId:{1} attack:{2} name:{3}", _id, attackModifier, _name)
    
    [<AllowNullLiteral>]
    type Punch(attack : int, heroId : Guid, dragonId : Guid) = 
        let _id = Guid.NewGuid()
        let _attack = attack
        let _heroId = heroId
        let _dragonId = dragonId
        
        interface IEntity with
            member this.GetId() = _id
        
        member this.Id = (this :> IEntity).GetId()
        member this.HeroId = _heroId
        member this.DragonId = _dragonId
        member this.Attack = _attack
        override this.ToString() = 
            String.Format("Punch id:{0} attack:{1} heroId:{2} dragonId:{3}", _id, _attack, _heroId, _dragonId)
    
    [<AllowNullLiteral>]
    type Dragon(name : string, maxHitpoints : int) = 
        let _id = Guid.NewGuid()
        let _name = name
        let _maxHitpoints = maxHitpoints
        
        interface IEntity with
            member this.GetId() = _id
        
        member this.Id = (this :> IEntity).GetId()
        member this.Name = _name
        member this.MaxHitpoints = _maxHitpoints
        override this.ToString() = String.Format("Hero id:{0} name:{1} maxHitpoints:{2}", _id, _name, _maxHitpoints)
    
    type IRepository<'T when 'T :> IEntity> = 
        abstract Filter : ('T -> bool) -> list<'T>
        abstract All : list<'T>
        abstract Get : ('T -> bool) -> 'T
        abstract Add : 'T -> unit
        abstract Delete : 'T -> unit
        abstract Update : 'T -> unit
    
    type BaseRepository<'T when 'T :> IEntity>() = 
        let thisLock = new Object()
        let mutable entityList : 'T list = []
        
        let isEntityEqualsWith (entity1 : 'T) = 
            let compareSecond (entity2 : 'T) = entity1.GetId() = entity2.GetId()
            compareSecond
        
        let skipValueFromList value list : 'T list = list |> List.filter (not << isEntityEqualsWith value)
        
        interface IRepository<'T> with
            member this.Filter predicate = entityList |> List.filter predicate
            member this.All = entityList
            member this.Get predicate = entityList |> List.find predicate
            member this.Add entity = entityList <- (entity :: entityList)
            member this.Delete entity = entityList <- (skipValueFromList entity entityList)
            member this.Update entity = entityList <- (entity :: (skipValueFromList entity entityList))
        
        member this.Filter predicate = (this :> IRepository<'T>).Filter predicate
        member this.All = (this :> IRepository<'T>).All
        member this.Get predicate = (this :> IRepository<'T>).Get predicate
        member this.Add entity = lock thisLock (fun _ -> (this :> IRepository<'T>).Add entity)
        member this.Delete entity = lock thisLock (fun _ -> (this :> IRepository<'T>).Delete entity)
        member this.Update entity = lock thisLock (fun _ -> (this :> IRepository<'T>).Update entity)
    
    type HeroRepository() = 
        inherit BaseRepository<Hero>()
    
    type PunchRepository() = 
        inherit BaseRepository<Punch>()
    
    type DragonRepository() = 
        inherit BaseRepository<Dragon>()
    
    type UnitOfWork() = 
        let _heroRepository = new HeroRepository()
        let _punchRepository = new PunchRepository()
        let _dragonRepository = new DragonRepository()
        member this.HeroRepository = _heroRepository
        member this.PunchRepository = _punchRepository
        member this.DragonRepository = _dragonRepository
    
    //controller repositories
    type PunchDTO = 
        { id : Guid
          attack : int
          heroName : string
          dragonName : string }
    
    type DragonDTO = 
        { id : Guid
          name : string
          maxHitpoints : int
          hitpoints : int
          isAlive : bool }
    
    type IHeroServiceRepository =         
        abstract Find : (Hero -> bool) -> Hero list
        abstract CreateHero : Hero -> Guid
        abstract GetList : Hero list
        abstract GetPunchesByHero : Guid -> PunchDTO list
    
    type IPunchServiceRepository = 
        abstract CreatePunch : Punch -> Guid
        abstract GetHero : Guid -> Hero option
    
    type IDragonServiceRepository = 
        abstract CreateDragon : Dragon -> Guid
        abstract Get : Guid -> DragonDTO option
        abstract GetList : DragonDTO list
        abstract GetFilterList : string -> int -> bool -> DragonDTO list
        abstract GetPunchesByDragon : Guid -> PunchDTO list
    
    
    
    type IHeroService = 
        abstract CreateHero : CreateHeroRequest -> MResult<Guid>
        abstract GetList : MResult<Hero list>
        abstract GetPunchesByHero : Guid -> MResult<PunchDTO list>
    
    type IPunchService = 
        abstract CreatePunch : CreatePunchRequest -> MResult<Guid>
    
    type IDragonService = 
        abstract CreateDragon : MResult<Guid>
        abstract Get : Guid -> MResult<DragonDTO option>
        abstract GetList : MResult<DragonDTO list>
        abstract GetFilterList : string -> int -> bool -> MResult<DragonDTO list>
        abstract GetPunchesByDragon : Guid -> MResult<PunchDTO list>
    
    type HeroService(heroServiceRepository : IHeroServiceRepository) =                 
        interface IHeroService with
            
            member this.CreateHero createHeroRequest =
                let validatePasswords = if createHeroRequest.confirmPassword <> createHeroRequest.password then Some [ "passwords doesn't match" ] else None
                                         
                let validate (createHeroRequest : CreateHeroRequest) =                    
                            (HeroName.isInvalid createHeroRequest.name
                        <?> HeroPassword.isInvalid createHeroRequest.password
                        <?> validatePasswords)
                        ?== Ok createHeroRequest
                
                let createHero (createHero: CreateHeroRequest) = 
                    let salt = Crypto.generateSalt
                    Ok (new Hero(createHero.name, 
                                 Crypto.hashPassword createHero.password salt, 
                                 salt, 
                                 Generator.heroAttackModifier))
                   
                let saveHero (hero: Hero) =
                    match heroServiceRepository.Find (fun h -> h.Name = hero.Name) |> List.tryHead with
                    | Some e -> Fail ["Hero already exists"]
                    | None -> Ok (heroServiceRepository.CreateHero hero)
                                                            
                createHeroRequest |> validate
                    >>= createHero
                    >>= saveHero
            
            member this.GetList = heroServiceRepository.GetList |> Ok
            member this.GetPunchesByHero heroId = 
                heroServiceRepository.GetPunchesByHero heroId |> Ok
    
    type PunchService(punchServiceRepository : IPunchServiceRepository) = 
        interface IPunchService with
            member this.CreatePunch createPunchRequest = 
                let hero = punchServiceRepository.GetHero createPunchRequest.heroId
                match hero with
                | None -> Fail ["hero not found"]
                | Some x -> 
                    punchServiceRepository.CreatePunch
                        (new Punch(Generator.punchAttack + x.AttackModifier, createPunchRequest.heroId, 
                                   createPunchRequest.dragonId)) |> Ok
    
    type DragonService(dragonServiceRepository : IDragonServiceRepository) = 
        interface IDragonService with
            member this.CreateDragon = 
                dragonServiceRepository.CreateDragon
                    (new Dragon(Generator.nameGenerator 6, Generator.dragonMaxHitpoints)) |> Ok
            member this.Get dragonId = dragonServiceRepository.Get dragonId |> Ok
            member this.GetList = dragonServiceRepository.GetList |> Ok
            member this.GetFilterList filter hitPoints isAlive = 
                dragonServiceRepository.GetFilterList filter hitPoints isAlive |> Ok
            member this.GetPunchesByDragon dragonId = 
                dragonServiceRepository.GetPunchesByDragon dragonId |> Ok
    
    type HeroServiceReposotory(unitOfWork : UnitOfWork) = 
        let _unitOfWork = unitOfWork
        interface IHeroServiceRepository with
            
            member this.CreateHero hero = 
                _unitOfWork.HeroRepository.Add hero
                hero.Id
                
            member this.Find predicate = _unitOfWork.HeroRepository.All |> List.filter predicate
            
            member this.GetList = _unitOfWork.HeroRepository.All
            member this.GetPunchesByHero heroId = 
                query { 
                    for punch in _unitOfWork.PunchRepository.All do
                        join dragon in _unitOfWork.DragonRepository.All on (punch.DragonId = dragon.Id)
                        join hero in _unitOfWork.HeroRepository.All on (punch.HeroId = hero.Id)
                        where (hero.Id = heroId)
                        select ({ id = punch.Id
                                  attack = punch.Attack
                                  heroName = hero.Name
                                  dragonName = dragon.Name })
                }
                |> Seq.toList
    
    type PunchServiceRepository(unitOfWork : UnitOfWork) = 
        let _unitOfWork = unitOfWork
        interface IPunchServiceRepository with
            
            member this.CreatePunch punch = 
                _unitOfWork.PunchRepository.Add punch
                punch.Id
            
            member this.GetHero heroId = _unitOfWork.HeroRepository.Filter(fun x -> x.Id = heroId) |> List.tryHead
    
    type DragonServiceRepository(unitOfWork : UnitOfWork) = 
        let _unitOfWork = unitOfWork
        interface IDragonServiceRepository with
            
            member this.CreateDragon dragon = 
                _unitOfWork.DragonRepository.Add dragon
                dragon.Id
            
            member this.GetPunchesByDragon dragonId = 
                query { 
                    for punch in _unitOfWork.PunchRepository.All do
                        join dragon in _unitOfWork.DragonRepository.All on (punch.DragonId = dragon.Id)
                        join hero in _unitOfWork.HeroRepository.All on (punch.HeroId = hero.Id)
                        where (dragon.Id = dragonId)
                        select ({ id = punch.Id
                                  attack = punch.Attack
                                  heroName = hero.Name
                                  dragonName = dragon.Name })
                }
                |> Seq.toList
            
            member this.Get dragonId = 
                query { 
                    for dragon in unitOfWork.DragonRepository.All do
                        leftOuterJoin punch in unitOfWork.PunchRepository.All on (dragon.Id = punch.DragonId) into 
                                                   _punch
                        for punch in _punch do
                            where (dragon.Id = dragonId)
                            groupValBy punch dragon into gd
                            let _damage = 
                                gd
                                |> Seq.map Option.ofObj
                                |> Seq.map (Option.map (fun x -> x.Attack))
                                |> (Seq.filter Option.isSome)
                                |> Seq.sumBy (fun x -> x.Value)
                            
                            let _hitpoints = gd.Key.MaxHitpoints - _damage
                            select ({ id = gd.Key.Id
                                      name = gd.Key.Name
                                      hitpoints = _hitpoints
                                      maxHitpoints = gd.Key.MaxHitpoints
                                      isAlive = _hitpoints > 0 })
                }
                |> Seq.tryHead
            
            member this.GetList = 
                query { 
                    for dragon in unitOfWork.DragonRepository.All do
                        leftOuterJoin punch in unitOfWork.PunchRepository.All on (dragon.Id = punch.DragonId) into 
                                                   _punch
                        for punch in _punch do
                            groupValBy punch dragon into gd
                            let _damage = 
                                gd
                                |> Seq.map Option.ofObj
                                |> Seq.map (Option.map (fun x -> x.Attack))
                                |> (Seq.filter Option.isSome)
                                |> Seq.sumBy (fun x -> x.Value)
                            
                            let _hitpoints = gd.Key.MaxHitpoints - _damage
                            select ({ id = gd.Key.Id
                                      name = gd.Key.Name
                                      hitpoints = _hitpoints
                                      maxHitpoints = gd.Key.MaxHitpoints
                                      isAlive = _hitpoints > 0 })
                }
                |> Seq.toList
            
            member this.GetFilterList filter hitPoints isAlive = 
                query { 
                    for dragon in unitOfWork.DragonRepository.All do
                        leftOuterJoin punch in unitOfWork.PunchRepository.All on (dragon.Id = punch.DragonId) into 
                                                   _punch
                        for punch in _punch do
                            groupValBy punch dragon into gd
                            let _damage = 
                                gd
                                |> Seq.map Option.ofObj
                                |> Seq.map (Option.map (fun x -> x.Attack))
                                |> (Seq.filter Option.isSome)
                                |> Seq.sumBy (fun x -> x.Value)
                            
                            let _hitPoints = gd.Key.MaxHitpoints - _damage
                            let _isAlive = _hitPoints > 0
                            where (gd.Key.Name.Contains(filter) && _hitPoints > hitPoints && isAlive = isAlive)
                            select ({ id = gd.Key.Id
                                      name = gd.Key.Name
                                      hitpoints = _hitPoints
                                      maxHitpoints = gd.Key.MaxHitpoints
                                      isAlive = _isAlive })
                }
                |> Seq.toList
