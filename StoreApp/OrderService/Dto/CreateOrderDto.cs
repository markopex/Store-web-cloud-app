using System;
using System.Collections.Generic;

namespace OrderService.Dto
{
    public class CreateOrderDto
    {
        public String CustomerId { get; set; }
        public List<CreateOrderDetailDto> OrderDetails { get; set; }
        public String Comment { get; set; }
        public String Address { get; set; }
        public String PaymentMethod { get; set; }
    }
}
