﻿using System;

namespace Drill4Net.Demo.OnlineStore.WebApi.Models
{
    public class ProductInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
    }
}
