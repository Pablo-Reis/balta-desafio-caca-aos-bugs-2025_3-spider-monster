using BugStore.Data;
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Customers;

public class CustomerHandler(AppDbContext context) : ICustomerHandler
{
    public async Task<Response<Customer>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.BirthDate
        };
        try
        {
            var createdModel = await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            return new Response<Customer>(createdModel.Entity);
        }
        catch (Exception ex)
        {
            return new Response<Customer>($"Ocorreu um erro ao criar cliente: {ex.Message}");
        }
    }

    public async Task<Response<Customer>> DeleteCustomerAsync(DeleteCustomerRequest request)
    {
        var customer = await context.Customers.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

        if (customer == null)
        {
            return new Response<Customer>("Customer not found.");
        }
        try
        {
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
            return new Response<Customer>(customer);
        }
        catch (Exception ex)
        {
            return new Response<Customer>($"Ocorreu um erro ao deletar cliente: {ex.Message}");
        }
        

    }

    public async Task<Response<List<Customer>>> GetCustomerAsync(GetCustomersRequest request)
        => new Response<List<Customer>>(await context.Customers.AsNoTracking().ToListAsync());

    public async Task<Response<Customer>> GetCustomerByIdAsync(GetCustomerByIdRequest request)
    {
        var data = await context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id);

        return new Response<Customer>(data);
    }

    public async Task<Response<Customer>> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        var customer = await context.Customers.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

        if (customer == null)
        {
            return new Response<Customer>("Customer not found.");
        }

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.BirthDate = request.BirthDate;

        try
        {
            context.Customers.Update(customer);
            await context.SaveChangesAsync();
            return new Response<Customer>(customer);
        }
        catch (Exception ex)
        {
            return new Response<Customer>($"Ocorreu um erro ao atualizar cliente: {ex.Message}");
        }

    }
}