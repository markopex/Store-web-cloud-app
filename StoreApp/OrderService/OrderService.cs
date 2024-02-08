using Common.Dto;
using Common.Events;
using Common.Models;
using Common.Services;
using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Core;
using System.Fabric;
using Common.Models;
using OrderService.Utils;
using OrderService.Models;
using Common.Dto.Order;
using PayPalHttp;

namespace OrderService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class OrderService : StatefulService, IOrderService
    {
        private PayPalEnvironment environment;
        private PayPalHttpClient client;
        private readonly string productValidatorServicePath = @"fabric:/StoreApp/ProductCatalogService";
        private IProductValidationService? _productValidationService
        {
            get
            {
                return ServiceProxy.Create<IProductValidationService>(new Uri(productValidatorServicePath));
            }
        }
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var orders = await StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("order");
            await orders.ClearAsync().ConfigureAwait(false);
        }

        public OrderService(StatefulServiceContext context)
            : base(context)
        {
            environment = new SandboxEnvironment(
                "AUVle8qulaOdCGWzbEy9trmYmo9brhbkMzOWdt5Z0wFPxcLLfYs-2rTMUAbmquEpt7UDGzqjlbfgZkq8",
                "EBYlwFmZXVKpNwAVpPmDVa5bpH1ITK2mbHmc0G2K4zh5Yc44abRaPNO5P-n0PJpy8tBeXJL1YZ9KyFWq"
                );
            client = new PayPalHttpClient(environment);
        }
        private async Task ChangeOrderStatus(string orderId, EOrderStatus newStatus)
        {
            var orders = await StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("order");
            using (var tx = StateManager.CreateTransaction())
            {
                try
                {
                    var orderResult = await orders.TryGetValueAsync(tx, orderId);
                    if (orderResult.HasValue)
                    {
                        var order = orderResult.Value;
                        order.Status = newStatus;
                        await orders.TryRemoveAsync(tx, orderId);
                        await orders.SetAsync(tx, orderId, order);
                        await tx.CommitAsync();
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    tx.Abort();
                }
                
            }
        }
        public async Task<OrderCreatedSuccessfullyDto> CreateOrder(string customerEmail, CreateOrderDto orderDto)
        {
            var paymentMethod = EPaymentMethod.CASH_ON_DELIVERY;
            switch (orderDto.PaymentMethod)
            {
                case "caseondelivery":
                    paymentMethod = EPaymentMethod.CASH_ON_DELIVERY;
                    break;
                case "paypal":
                    paymentMethod = EPaymentMethod.PAYPAL;
                    break;
                default:
                    break;
            }
            List<OrderDetail> products = await _productValidationService.ReserveProducts(orderDto.OrderDetails);
            var order = new Order
            {
                // Assuming there's a mechanism to generate unique IDs
                Id = Guid.NewGuid().ToString(),
                Customer = customerEmail,
                UTCTimeOrderCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                OrderDetails = products,//MapOrderDetails(orderDto.OrderDetails),
                Comment = orderDto.Comment,
                Address = orderDto.Address,
                PaymentMethod = paymentMethod,
                Status = (paymentMethod == EPaymentMethod.CASH_ON_DELIVERY) ? EOrderStatus.SUCCESS : EOrderStatus.PENDING
            };

            string? approvalUrl = null;
            var orderId = order.Id;
            if (paymentMethod == EPaymentMethod.PAYPAL)
            {
                // generate paypal order
                var paypalOrder = await CreatePaypalOrder(order);
                // You can then return the approval link to the client
                approvalUrl = paypalOrder.Links.FirstOrDefault(link => link.Rel.Equals("approve")).Href;
                orderId = paypalOrder.Id;
                order.PaypalOrderId = orderId;
            }

            // Save order to a Reliable Dictionary
            var orders = await StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("order");
            using (var tx = StateManager.CreateTransaction())
            {
                await orders.AddAsync(tx, order.Id, order);
                await tx.CommitAsync();
            }

            
            // Send event to product catalog to check inventory
            //PublishMessage(new OrderCreatedEvent { User = customerEmail, OrderId = order.Id, Items = order.OrderDetails });

            return new OrderCreatedSuccessfullyDto()
            {
                OrderId = orderId,
                RedirectUrl = approvalUrl
            };
        }
        public async Task CaptureOrder(string token)
        {
            var request = new PayPalCheckoutSdk.Orders.OrdersGetRequest(token);
            var response = await client.Execute(request);
            var order = response.Result<PayPalCheckoutSdk.Orders.Order>();

            // Process the order based on the response, e.g., save it to your database
            var status = PayPalUtils.MapStringToOrderStatus(order.Status);
            var orderId = order.PurchaseUnits.First().ReferenceId;
            if (status == PayPalOrderStatus.Completed)
            {
                await ChangeOrderStatus(orderId, EOrderStatus.SUCCESS);
            }
            if (status == PayPalOrderStatus.Voided)
            {
                await ChangeOrderStatus(orderId, EOrderStatus.FAILURE);
                // release products
                // todo
            }
        }

        public async Task<PayPalCheckoutSdk.Orders.Order> CreatePaypalOrder(Common.Models.Order order)
        {
            var request = new PayPalCheckoutSdk.Orders.OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(BuildOrderRequestBody(order));
            var response = await client.Execute(request);
            var paypalOrder = response.Result<PayPalCheckoutSdk.Orders.Order>();

            return paypalOrder;
        }

        private PayPalCheckoutSdk.Orders.OrderRequest BuildOrderRequestBody(Common.Models.Order order)
        {
            return new PayPalCheckoutSdk.Orders.OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PayPalCheckoutSdk.Orders.PurchaseUnitRequest>
        {
            new PayPalCheckoutSdk.Orders.PurchaseUnitRequest
            {
                ReferenceId = order.Id.ToString(),
                AmountWithBreakdown = new PayPalCheckoutSdk.Orders.AmountWithBreakdown
                {
                    CurrencyCode = "USD",
                    Value = order.Price.ToString(),
                }
            }
        },
                ApplicationContext = new PayPalCheckoutSdk.Orders.ApplicationContext
                {
                    ReturnUrl = "http://localhost:8080/order/capture",
                    CancelUrl = "http://localhost:8080/order/cancel"
                }
            };
        }

        public async Task<Order> GetOrder(string id)
        {
            var orders = await StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("order");
            using (var tx = StateManager.CreateTransaction())
            {
                var order = await orders.TryGetValueAsync(tx, id);
                return order.HasValue ? order.Value : null;
            }
        }

        public async Task<List<Order>> GetOrdersByUser(string userEmail)
        {
            var orders = await StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("order");
            var result = new List<Order>();
            using (var tx = StateManager.CreateTransaction())
            {
                var allOrders = await orders.CreateEnumerableAsync(tx);
                var enumerator = allOrders.GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    var current = enumerator.Current;
                    if (current.Value.Customer == userEmail)
                    {
                        result.Add(current.Value);
                    }
                }
            }
            return result;
        }
        public async Task CancelOrder(string token)
        {
            var request = new PayPalCheckoutSdk.Orders.OrdersGetRequest(token);
            var response = await client.Execute(request);
            var payPalorder = response.Result<PayPalCheckoutSdk.Orders.Order>();

            // Process the order based on the response, e.g., save it to your database
            var status = PayPalUtils.MapStringToOrderStatus(payPalorder.Status);
            var orderId = payPalorder.PurchaseUnits.First().ReferenceId;
            var order = await GetOrder(orderId);
            await _productValidationService.CancelReservationOnProducts(order.OrderDetails);
            await ChangeOrderStatus(orderId, EOrderStatus.CANCELLED);
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
