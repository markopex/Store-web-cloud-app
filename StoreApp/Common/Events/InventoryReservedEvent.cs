using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class InventoryReservedEvent
    {
        public int OrderId { get; set; }
        public List<OrderDetail> Items { get; set; } = new List<OrderDetail>();
    }

    public class InventoryReservationFailedEvent
    {
        public int OrderId { get; set; }
    }
}
