namespace Common.Models
{
    public class OrderDetail
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double ProductPrice { get; set; }
        public string ProductName { get; set; }
        public double Price { get {return ProductPrice * Quantity; } }
    }
}
