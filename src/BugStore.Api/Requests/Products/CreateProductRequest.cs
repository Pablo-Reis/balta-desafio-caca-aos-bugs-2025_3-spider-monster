using System.ComponentModel.DataAnnotations;

namespace BugStore.Requests.Products;

public class CreateProductRequest
{
    [Required(ErrorMessage = "T�tulo � obrigat�rio.")]
    public string Title { get; set; }
    [Required(ErrorMessage = "Descri��o � obrigat�ria.")]
    public string Description { get; set; }
    public string Slug { get; set; }
    [Required(ErrorMessage = "Pre�o � obrigat�rio.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
}