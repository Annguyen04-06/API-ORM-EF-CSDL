using System;
using System.Collections.Generic;

namespace btap_api_orm.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
