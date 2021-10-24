﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        public List<CartItem> Products { get; set; }
        public decimal Total { get; set; }
    }
}
