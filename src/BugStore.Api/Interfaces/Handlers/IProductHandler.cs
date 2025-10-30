using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Responses;

namespace BugStore.Interfaces.Handlers
{
    public interface IProductHandler
    {
        Task<Response<Product>> CreateProductAsync(CreateProductRequest request);
        Task<Response<Product>> UpdateProductAsync(UpdateProductRequest request);
        Task<Response<Product>> DeleteProductAsync(DeleteProductRequest request);
        Task<Response<List<Product>>> GetProductsAsync(GetProductsRequest request);
        Task<Response<Product>> GetProductByIdAsync(GetProductByIdRequest request);
    }
}
