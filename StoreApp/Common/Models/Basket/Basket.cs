using System.Collections.Generic;

namespace Common.Models
{
    public class Basket
    {
        public string Id { get; set; }
        public List<BasketItem> BasketItems { get; set; }
    }
}
