using System;
using System.Collections.Generic;

namespace Order.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string  BuyerId { get; set; }

        public Adress Adress { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        public OrderStatus Status { get; set; }

        public int FailMessage { get; set; }
    }

    public enum OrderStatus
    {
        Suspend,
        Success,
        Fail
    }
}
