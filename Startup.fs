namespace DragonServer

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open DragonServer.Model.Dragon
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Swashbuckle.AspNetCore.Swagger
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.AspNetCore.Mvc.ModelBinding.Binders
open System.Threading.Tasks
open DragonServer.Requests
open System.Reflection
open System.IO




type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.       

        services.AddMvc() |> ignore
        services.AddSingleton<UnitOfWork>() |> ignore
        services.AddSingleton<IHeroServiceRepository, HeroServiceReposotory>() |> ignore
        services.AddSingleton<IPunchServiceRepository, PunchServiceRepository>() |> ignore
        services.AddSingleton<IDragonServiceRepository, DragonServiceRepository>() |> ignore
        services.AddScoped<IHeroService, HeroService>() |> ignore
        services.AddScoped<IDragonService, DragonService>() |> ignore
        services.AddScoped<IPunchService, PunchService>() |> ignore
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(fun options ->
                 options.TokenValidationParameters <- TokenValidationParameters(
                     ValidateActor = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = "jwtwebapp.net",
                     ValidAudience = "jwtwebapp.net",
                     IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890")))
                 ) |> ignore
        services.AddAuthorization() |> ignore
        
        services.AddSwaggerGen(fun options -> options.SwaggerDoc("v1", new Info(Version = "v1", Title = "API"))) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        
        app.UseAuthentication() |> ignore
        app.UseMvc() |> ignore
        
        app.UseSwagger() |> ignore
        app.UseSwaggerUI(fun c ->
         c.DisplayRequestDuration() |> ignore
         c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API")) |> ignore

    member val Configuration : IConfiguration = null with get, set