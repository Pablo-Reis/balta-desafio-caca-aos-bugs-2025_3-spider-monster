using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BugStore.Requests.Customers;

public class UpdateCustomerRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Nome � obrigat�rio")]
    [StringLength(maximumLength: 50, ErrorMessage = "Tamanho do campo deve ser entre 3 e 50 caracteres", MinimumLength = 3)]
    public string Name { get; set; }
    [EmailAddress(ErrorMessage = "Email inv�lido")]
    [Required(ErrorMessage = "Email � obrigat�rio")]
    public string Email { get; set; }
    [Phone(ErrorMessage = "Telefone inv�lido")]
    [Required(ErrorMessage = "Telefone � obrigat�rio")]
    public string Phone { get; set; }
    [Required(ErrorMessage = "Data de nascimento � obrigat�ria")]
    [DataType(DataType.Date, ErrorMessage = "Data de nascimento inv�lida")]
    public DateTime BirthDate { get; set; }
}