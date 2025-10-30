using BugStore.Models;
using BugStore.Requests.OrderLines;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BugStore.Requests.Orders;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "C�digo do cliente � obrigat�rio.")]
    public Guid CustomerId { get; set; }

    [JsonIgnore]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public DateTime UpdatedAt { get; set; }
    public List<CreateOrderLineRequest> Lines { get; set; } = null;
}