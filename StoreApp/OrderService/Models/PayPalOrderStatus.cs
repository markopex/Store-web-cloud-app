namespace OrderService.Models
{
    public enum PayPalOrderStatus
    {
        Created,
        Saved,
        Approved,
        Voided,
        Completed,
        PayerActionRequired
    }
}
