using BugStore.Data;
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Responses;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Products;

public class ProductHandler(AppDbContext context) : IProductHandler
{
    public async Task<Response<Product>> CreateProductAsync(CreateProductRequest request)
    {
        var Product = new Product
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Slug = request.Slug
        };
        try
        {
            var createdModel = await context.Products.AddAsync(Product);
            await context.SaveChangesAsync();

            return new Response<Product>(createdModel.Entity);
        }
        catch (Exception ex)
        {
            return new Response<Product>($"Ocorreu um erro ao criar produto: {ex.Message}");
        }
    }

    public async Task<Response<Product>> DeleteProductAsync(DeleteProductRequest request)
    {
        var Product = await context.Products.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

        if (Product == null)
        {
            return new Response<Product>("Product not found.");
        }
        try
        {
            context.Products.Remove(Product);
            await context.SaveChangesAsync();
            return new Response<Product>(Product);
        }
        catch (Exception ex)
        {
            return new Response<Product>($"Ocorreu um erro ao deletar produto: {ex.Message}");
        }
        

    }

    public async Task<Response<List<Product>>> GetProductsAsync(GetProductsRequest request)
        => new Response<List<Product>>(await context.Products.AsNoTracking().ToListAsync());

    public async Task<Response<Product>> GetProductByIdAsync(GetProductByIdRequest request)
    {
        var data = await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id);

        return new Response<Product>(data);
    }

    public async Task<Response<Product>> UpdateProductAsync(UpdateProductRequest request)
    {
        var Product = await context.Products.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

        if (Product == null)
        {
            return new Response<Product>("Product not found.");
        }

        Product.Title = request.Title;
        Product.Description = request.Description;
        Product.Price = request.Price;
        Product.Slug = request.Slug;

        try
        {
            context.Products.Update(Product);
            await context.SaveChangesAsync();
            return new Response<Product>(Product);
        }
        catch (Exception ex)
        {
            return new Response<Product>($"Ocorreu um erro ao deletar produto: {ex.Message}");
        }

    }
}