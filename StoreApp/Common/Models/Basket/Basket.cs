﻿using System.Collections.Generic;

namespace Common.Models.Basket
{
    public class Basket
    {
        public string Id { get; set; }
        public List<BasketItem> BasketItems { get; set; }
    }
}
