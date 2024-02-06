using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ApiGateway : StatelessService
    {
        private readonly string _cors = "cors";
        public ApiGateway(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                     .ConfigureAppConfiguration((hostingContext, config) =>
                                    {
                                        config
                                            .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                                            .AddJsonFile("appsettings.json", true, true)
                                            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                                            //.AddJsonFile("ocelot.json", false, false)
                                            .AddEnvironmentVariables();
                                    })
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        //var ocelotBuilder = new ConfigurationBuilder();
                        //ocelotBuilder
                        //        .SetBasePath(Directory.GetCurrentDirectory())
                        //       //add configuration.json  
                        //       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        //       .AddEnvironmentVariables();

                        //var ocelotConfiguration = ocelotBuilder.Build();

                        //builder.Services.AddOcelot();


                        builder.Services.AddAuthentication(opt => {
                            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                       .AddJwtBearer(options =>
                       {
                           options.TokenValidationParameters = new TokenValidationParameters //Podesavamo parametre za validaciju pristiglih tokena
                           {
                               ValidateIssuer = false, //Validira izdavaoca tokena
                               ValidateAudience = false, //Kazemo da ne validira primaoce tokena
                               ValidateLifetime = true,//Validira trajanje tokena
                               ValidateIssuerSigningKey = true, //validira potpis token, ovo je jako vazno!
                               //ValidIssuer = "http://localhost:5001", //odredjujemo koji server je validni izdavalac
                               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecretKey"]))//navodimo privatni kljuc kojim su potpisani nasi tokeni
                           };
                       });
                        builder.Services.AddControllers();
                        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiGateway", Version = "v1" });
                            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                            {
                                In = ParameterLocation.Header,
                                Description = "Please enter token",
                                Name = "Authorization",
                                Type = SecuritySchemeType.Http,
                                BearerFormat = "JWT",
                                Scheme = "bearer"
                            });
                            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type=ReferenceType.SecurityScheme,
                                            Id="Bearer"
                                        }
                                    },
                                    new string[]{}
                                }
                            });
                        });

                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy(name: _cors, builder => {
                                builder.WithOrigins("http://localhost:4200")//Ovde navodimo koje sve aplikacije smeju kontaktirati nasu,u ovom slucaju nas Angular front
                                       .AllowAnyHeader()
                                       .AllowAnyMethod()
                                       .AllowCredentials();
                            });
                        });

                        var app = builder.Build();
                        
                        // Configure the HTTP request pipeline.
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }
                        app.UseHttpsRedirection();

                        app.UseAuthentication();
                        app.UseCors(_cors);
                        app.UseAuthorization();
                        app.MapControllers();
                        //app.UseOcelot().Wait();


                        return app;

                    }))
            };
        }
    }
}
