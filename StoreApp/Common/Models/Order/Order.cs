namespace Common.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string Customer { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public String Comment { get; set; }
        public String Address { get; set; }
        public double Price => OrderDetails.Sum(i => i.Price);
        public EPaymentMethod PaymentMethod { get; set; }
        public EOrderStatus Status { get; set; }
        public long UTCTimeOrderCreated { get; set; }
        public string? PaypalOrderId { get; set; }
    }
}
