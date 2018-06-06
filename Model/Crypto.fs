namespace DragonServer.Model

module Crypto = 
    open Microsoft.AspNetCore.Cryptography.KeyDerivation
    open System.Security.Cryptography
    
    let generateSalt = 
        let salt = Array.zeroCreate (128 / 8)
        use rng = RandomNumberGenerator.Create()
        rng.GetBytes(salt)
        salt
    
    let hashPassword password salt = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA1, 10000, 256 / 8)
