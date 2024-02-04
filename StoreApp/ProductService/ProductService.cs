using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using AutoMapper;
using ProductService.Interfaces;
using ProductService.Mapping;
using ProductService.Services;
using StatelessService = Microsoft.ServiceFabric.Services.Runtime.StatelessService;
using System.Globalization;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Common.Services;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;

namespace ProductService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ProductService : StatelessService
    {
        public ProductService(StatelessServiceContext context)
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
                        
                        // Add services to the container.
                        builder.Services.AddDbContext<ProductsDbContext>(options =>
                            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                        builder.Services.AddControllers();
                        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();

                        var mapperConfig = new MapperConfiguration(mc =>
                        {
                            mc.AddProfile(new MappingProfile());
                        });

                        IMapper mapper = mapperConfig.CreateMapper();
                        builder.Services.AddSingleton(mapper);
                        builder.Services.AddScoped<IProductsService, ProductsService>();
                        builder.Services.AddScoped<ICategoryService, CategoryService>();

                        var app = builder.Build();
                        
                        // Configure the HTTP request pipeline.
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }
                        
                        app.UseAuthorization();
                        
                        app.MapControllers();
                        
                        
                        return app;

                    })),
                new ServiceInstanceListener((c) =>
                 {
                     var factory = new ProductsDbContextFactory();
                     string[] args = { };
                     return new FabricTransportServiceRemotingListener(c, new ProductValidationService(factory.CreateDbContext(args)));
                 })     
            };
        }

        //private ICommunicationListener CreateWcfCommunicationListener(StatelessServiceContext context)
        //{
        //    string host = context.NodeContext.IPAddressOrFQDN;
        //    var endpointConfig = context.CodePackageActivationContext.GetEndpoint("WcfServiceEndpoint");
        //    int port = endpointConfig.Port;
        //    var scheme = endpointConfig.Protocol.ToString();
        //    string uri = string.Format(CultureInfo.InvariantCulture, "net.{0}://{1}:{2}/ServiceEndpoint", scheme, host, port);

        //    return new WcfCommunicationListener<IProductValidationService>(
        //        serviceContext: context,
        //        wcfServiceType: this,
        //        listenerBinding: WcfUtility.CreateTcpClientBinding(),
        //        address: new System.ServiceModel.EndpointAddress(uri)
        //        );
        //}
    }
}
