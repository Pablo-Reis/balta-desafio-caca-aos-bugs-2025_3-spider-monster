using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BugStore.Requests.Customers;

public class UpdateCustomerRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(maximumLength: 50, ErrorMessage = "Tamanho do campo deve ser entre 3 e 50 caracteres", MinimumLength = 3)]
    public string Name { get; set; }
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Required(ErrorMessage = "Email é obrigatório")]
    public string Email { get; set; }
    [Phone(ErrorMessage = "Telefone inválido")]
    [Required(ErrorMessage = "Telefone é obrigatório")]
    public string Phone { get; set; }
    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    [DataType(DataType.Date, ErrorMessage = "Data de nascimento inválida")]
    public DateTime BirthDate { get; set; }
}