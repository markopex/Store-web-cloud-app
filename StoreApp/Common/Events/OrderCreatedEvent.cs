using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class OrderCreatedEvent
    {
        public string User { get; set; }
        public int OrderId { get; set; }
        public List<OrderDetail> Items { get; set; } = new List<OrderDetail>();
    }
}
