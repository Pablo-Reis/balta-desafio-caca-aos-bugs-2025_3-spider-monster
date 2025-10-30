using BugStore.Models;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BugStore.Requests.OrderLines
{
    public class CreateOrderLineRequest
    {
        public int Quantity { get; set; } = 0;
        [JsonIgnore]
        public decimal Total => Product is null ? 0 : Product.Price * Quantity;
        public Guid ProductId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; } = new Product();
    }
}
