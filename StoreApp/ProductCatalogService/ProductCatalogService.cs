using AutoMapper;
using Azure;
using Azure.Data.Tables;
using Common.Dto;
using Common.Models;
using Common.Services;
using Microsoft.OpenApi.Models;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ProductCatalogService.Interfaces;
using ProductCatalogService.Mapping;
using ProductCatalogService.Models;
using ProductCatalogService.Services;
using System.Fabric;

namespace ProductCatalogService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ProductCatalogService : StatelessService, IProductValidationService
    {
        private readonly TableClient _tableClient;
        public ProductCatalogService(StatelessServiceContext context)
            : base(context)
        {
            string connectionString = "UseDevelopmentStorage=true;"; // For Azure Storage Emulator
            string tableName = "Products";
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<bool> CheckIsBasketValid(BasketDto basket)
        {
            var productIds = basket.BasketItems.Select(x => x.ProductId).ToList();
            var validProductIds = new List<int>();

            // Assuming RowKey is used as the product ID
            foreach (var product in basket.BasketItems)
            {
                var productId = product.ProductId;
                var response = _tableClient.QueryAsync<ProductEntity>(filter => filter.RowKey == productId.ToString());
                await foreach (var entity in response)
                {
                    if (entity.Quantity < product.Quantity) return false;
                    validProductIds.Add(int.Parse(entity.RowKey));
                }
            }
            return true;
        }

        public async Task<List<Product>> GetProductsByIds(List<int> productIds)
        {
            var products = new List<Product>();

            // Fetch each product by ID
            foreach (var productId in productIds)
            {
                var response = _tableClient.QueryAsync<ProductEntity>(filter => filter.RowKey == productId.ToString());
                await foreach (var entity in response)
                {
                    // Assuming you have configured AutoMapper or a similar mapping tool
                    var productDto = new Product // This should be mapped from entity to DTO
                    {
                        Id = int.Parse(entity.RowKey),
                        Name = entity.Name,
                        Price = entity.Price,
                        Description = entity.Description,
                        CategoryId = entity.CategoryId,
                        ImageUrl = entity.ImageUrl,
                        Quantity = entity.Quantity,
                    };
                    products.Add(productDto);
                }
            }

            return products;
        }

        public async Task<List<OrderDetail>> ReserveProducts(List<CreateOrderDetailDto> orderDetailDtos)
        {
            var partitionKey = "0";

            var batch = new List<TableTransactionAction>();
            var orderDetails = new List<OrderDetail>();

            foreach (var detail in orderDetailDtos)
            {
                try
                {
                    // Attempt to retrieve the entity
                    var productEntity = await _tableClient.GetEntityAsync<ProductEntity>(partitionKey, detail.ProductId.ToString());

                    if (productEntity.Value == null)
                    {
                        throw new Exception($"Product with ID {detail.ProductId} does not exist.");
                    }
                    if (productEntity.Value.Quantity < detail.Quantity)
                    {
                        throw new Exception($"Product with ID {detail.ProductId} quantity insufficient.");
                    }
                    // Assuming you have a Quantity property to update
                    // For example, let's say we are decrementing stock quantity
                    productEntity.Value.Quantity -= detail.Quantity;
                    var entity = productEntity.Value;
                    var orderDetail = new OrderDetail // This should be mapped from entity to DTO
                    {
                        ProductId = int.Parse(entity.RowKey),
                        ProductName = entity.Name,
                        ProductPrice = entity.Price,
                        Quantity = detail.Quantity,
                    };
                    orderDetails.Add(orderDetail);

                    // Assuming you handle quantity updates within your ProductEntity logic
                    var updateAction = new TableTransactionAction(TableTransactionActionType.UpdateReplace, productEntity.Value);
                    batch.Add(updateAction);
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // Product not found
                    return new List<OrderDetail>();
                }

                // Submit the batch in chunks of 100 actions or less (Azure Table Storage limitation)
                if (batch.Count == 100)
                {
                    await _tableClient.SubmitTransactionAsync(batch);
                    batch.Clear();
                }
            }

            // Submit any remaining actions
            if (batch.Count > 0)
            {
                await _tableClient.SubmitTransactionAsync(batch);
            }

            return orderDetails;
        }
        public async Task<List<OrderDetail>> CancelReservationOnProducts(List<OrderDetail> orderDetailDtos)
        {
            return await ReserveProducts(orderDetailDtos.Select(i => new CreateOrderDetailDto()
            {
                ProductId = i.ProductId,
                Quantity = -i.Quantity
            }).ToList());
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners().Concat(new ServiceInstanceListener[]
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
                        builder.Services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductApi", Version = "v1" });
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
                        builder.Services.AddScoped<IProductsService, ProductService>();
                        builder.Services.AddScoped<ICategoryService, CategoryService>();
                        builder.Services.AddControllers();
                        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();

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
                //new ServiceInstanceListener(context =>
                //    new FabricTransportServiceRemotingListener(context, this), "RemotingListener")
            });
        }

        
    }
}
