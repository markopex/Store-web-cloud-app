namespace BasketService.Dto
{
    public class BasketDto
    {
        public List<BasketItemDto> BasketItems { get; set; }
    }
    public class BasketItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
