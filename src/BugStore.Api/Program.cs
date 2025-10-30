using BugStore.Common;
using BugStore.Data;
using BugStore.Endpoints;
using BugStore.Handlers.Customers;
using BugStore.Handlers.Products;
using BugStore.Interfaces.Handlers;
using BugStore.IoC;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependecyInjection();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapEndpoints();

app.AddDocumentation();

app.Run();
