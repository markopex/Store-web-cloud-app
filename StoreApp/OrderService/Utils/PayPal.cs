using OrderService.Models;

namespace OrderService.Utils
{
    public static class PayPalUtils
    {
        public static PayPalOrderStatus MapStringToOrderStatus(string status)
        {
            switch (status.ToUpper())
            {
                case "CREATED":
                    return PayPalOrderStatus.Created;
                case "SAVED":
                    return PayPalOrderStatus.Saved;
                case "APPROVED":
                    return PayPalOrderStatus.Approved;
                case "VOIDED":
                    return PayPalOrderStatus.Voided;
                case "COMPLETED":
                    return PayPalOrderStatus.Completed;
                case "PAYER_ACTION_REQUIRED":
                    return PayPalOrderStatus.PayerActionRequired;
                default:
                    throw new ArgumentException("Unknown order status: " + status);
            }
        }
    }
}
