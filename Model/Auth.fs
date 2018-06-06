namespace DragonServer.Model    
    module Token =
        open Microsoft.IdentityModel.Tokens
        open System.IdentityModel.Tokens.Jwt
        open System.Text
        open System
        open System.Security.Claims
    
        type Info = {                            
            audience: string
            issuer: string
            issuerSecurityKey: string
            lifetime: float
            
            validateAudience: bool
            validateIssuer: bool
            validateIssuerSecurityKey: bool
            validateLifetime: bool
        }
        
        type TokenInfo = {
            token: string
            expireAt: Nullable<DateTime>
        }
    
        let generateEndless tokenInfo claims =
            new JwtSecurityToken(
                            tokenInfo.issuer, 
                            tokenInfo.audience, 
                            claims,
                            Nullable(),                 
                            Nullable(), 
                            new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenInfo.issuerSecurityKey)), SecurityAlgorithms.HmacSha256))
                            
        let generateExpired tokenInfo fromDateTime (toDateTime: DateTime) claims =
             new JwtSecurityToken(
                            tokenInfo.issuer, 
                            tokenInfo.audience, 
                            claims,
                            Nullable fromDateTime,                 
                            Nullable (toDateTime.AddMinutes(tokenInfo.lifetime)), 
                            new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenInfo.issuerSecurityKey)), SecurityAlgorithms.HmacSha256))                   
    
        let generate tokenInfo fromDateTime claims =
            let jwtSecToken = new JwtSecurityTokenHandler()
            
            match fromDateTime with
            | Some t -> {
                            token = jwtSecToken.WriteToken(generateExpired tokenInfo t (t.AddMinutes(tokenInfo.lifetime)) claims)
                            expireAt = Nullable (t.AddMinutes(tokenInfo.lifetime))
                        }
            | None ->   {
                            token = jwtSecToken.WriteToken(generateEndless tokenInfo claims)
                            expireAt = Nullable()
                        }                                                                           

            
                     
                     
                        
       
        
        
            
    