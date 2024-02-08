using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto.Order
{
    public class OrderCreatedSuccessfullyDto
    {
        public string OrderId { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
