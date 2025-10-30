using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BugStore.Requests.Products;

public class UpdateProductRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
    [Required(ErrorMessage = "T�tulo � obrigat�rio.")]
    public string Title { get; set; }
    [Required(ErrorMessage = "Descri��o � obrigat�ria.")]
    public string Description { get; set; }
    public string Slug { get; set; }
    [Required(ErrorMessage = "Pre�o � obrigat�rio.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
}