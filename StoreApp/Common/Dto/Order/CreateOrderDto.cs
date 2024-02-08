using System;
using System.Collections.Generic;

namespace Common.Dto
{
    public class CreateOrderDto
    {
        public List<CreateOrderDetailDto> OrderDetails { get; set; }
        public String Comment { get; set; }
        public String Address { get; set; }
        public String PaymentMethod { get; set; }
    }
}
