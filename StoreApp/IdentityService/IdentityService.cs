using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using IdentityService.Interfaces;
using IdentityService.Services;
using AutoMapper;
using IdentityService.Mapping;
using StatelessService = Microsoft.ServiceFabric.Services.Runtime.StatelessService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Fabric.Query;

namespace IdentityService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class IdentityService : StatelessService
    {
        public IdentityService(StatelessServiceContext context)
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
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
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


                        // Add services to the container.
                        
                        builder.Services.AddControllers();
                        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityApi", Version = "v1" });
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
                        var mapperConfig = new MapperConfiguration(mc =>
                        {
                            mc.AddProfile(new MappingProfile());
                        });

                        IMapper mapper = mapperConfig.CreateMapper();
                        builder.Services.AddSingleton(mapper);
                        builder.Services.AddScoped<IUserService, UserService>();
                        
                        var app = builder.Build();
                        
                        // Configure the HTTP request pipeline.
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }


                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.MapControllers();
                        
                        
                        return app;

                    }))
            };
        }
    }
}
