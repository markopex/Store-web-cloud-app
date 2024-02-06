namespace Common.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Customer { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public String Comment { get; set; }
        public String Address { get; set; }
        public double Price => OrderDetails.Sum(i => i.Price);
        public EPaymentMethod PaymentMethod { get; set; }
        public bool IsCancelled { get; set; } = false;
    }
}
