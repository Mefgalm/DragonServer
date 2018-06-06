namespace DragonServer.Model

type MResult<'a> = 
    | Ok of 'a
    | Fail of string list

module HeroName = 
    type T = 
        | HeroName of string
    
    let isInvalid (s : string) = 
        if System.Text.RegularExpressions.Regex.IsMatch(s.Trim(), @"^[A-Za-z ]{3,20}$") then None
        else Some [ "Hero name is wrong" ]
    
    let value (HeroName e) = e

module HeroPassword = 
    type T = 
        | HeroPassword of string
    
    let isInvalid (m : string) = 
        if System.Text.RegularExpressions.Regex.IsMatch(m.Trim(), @"^.{6,30}$") then None
        else Some [ "Password is incorrect" ]
    
    let value (HeroPassword m) = m