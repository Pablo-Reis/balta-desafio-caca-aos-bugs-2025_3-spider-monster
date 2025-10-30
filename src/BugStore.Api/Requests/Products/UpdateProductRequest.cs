using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BugStore.Requests.Products;

public class UpdateProductRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Título é obrigatório.")]
    public string Title { get; set; }
    [Required(ErrorMessage = "Descrição é obrigatória.")]
    public string Description { get; set; }
    public string Slug { get; set; }
    [Required(ErrorMessage = "Preço é obrigatório.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
}