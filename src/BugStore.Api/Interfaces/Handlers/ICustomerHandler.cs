using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;

namespace BugStore.Interfaces.Handlers
{
    public interface ICustomerHandler
    {
        Task<Response<Customer>> CreateCustomerAsync(CreateCustomerRequest request);
        Task<Response<Customer>> UpdateCustomerAsync(UpdateCustomerRequest request);
        Task<Response<Customer>> DeleteCustomerAsync(DeleteCustomerRequest request);
        Task<Response<List<Customer>>> GetCustomerAsync(GetCustomersRequest request);
        Task<Response<Customer>> GetCustomerByIdAsync(GetCustomerByIdRequest request);
    }
}
