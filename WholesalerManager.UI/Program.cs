using Microsoft.EntityFrameworkCore;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.ProductServices;
using WholesaleManager.Infrastructure.Repositories;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;
using WholesalerManager.Core.Services.CategoriesServices;
using WholesalerManager.Infrastructure.Repositories;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.Core.Services.SupplierServices;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.Services.DeliveryServices;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.Services.DeliveryItemServices;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.Services.CustomerServices;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.Services.OrderServices;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;
using WholesalerManager.Infrastructure.DatabaseContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

#region Repositories
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ISuppliersRepository, SuppliersRepository>();
builder.Services.AddScoped<IDeliveriesRepository, DeliveriesRepository>();
builder.Services.AddScoped<IDeliveryItemsRepository, DeliveryItemsRepository>();
builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IOrderItemsRepository, OrderItemsRepository>();

#endregion

#region Services

// Products
builder.Services.AddScoped<IProductsGetterService, ProductsGetterService>();
builder.Services.AddScoped<IProductsAdderService, ProductsAdderService>();
builder.Services.AddScoped<IProductsDeleterService, ProductsDeleterService>();
builder.Services.AddScoped<IProductsUpdaterService, ProductsUpdaterService>();

// Categories
builder.Services.AddScoped<ICategoriesGetterService, CategoriesGetterService>();

// Suppliers
builder.Services.AddScoped<ISuppliersGetterService, SuppliersGetterService>();

// Deliveries
builder.Services.AddScoped<IDeliveriesGetterService, DeliveriesGetterService>();
builder.Services.AddScoped<IDeliveriesAdderService, DeliveriesAdderService>();
builder.Services.AddScoped<IDeliveriesUpdaterService, DeliveriesUpdaterService>();
builder.Services.AddScoped<IDeliveriesDeleterService, DeliveriesDeleterService>();

// DeliveryItems
builder.Services.AddScoped<IDeliveryItemsGetterService, DeliveryItemsGetterService>();
builder.Services.AddScoped<IDeliveryItemsAdderService, DeliveryItemsAdderService>();
builder.Services.AddScoped<IDeliveryItemsUpdaterService, DeliveryItemsUpdaterService>();

// Customers
builder.Services.AddScoped<ICustomersGetterService, CustomersGetterService>();
builder.Services.AddScoped<ICustomersAdderService, CustomersAdderService>();
builder.Services.AddScoped<ICustomersUpdaterService, CustomersUpdaterService>();
builder.Services.AddScoped<ICustomersDeleterService, CustomersDeleterService>();

// Orders
builder.Services.AddScoped<IOrdersGetterService, OrdersGetterService>();
builder.Services.AddScoped<IOrdersDeleterService, OrdersDeleterService>();
builder.Services.AddScoped<IOrdersUpdaterService, OrdersUpdaterService>();
builder.Services.AddScoped<IOrdersAdderService, OrdersAdderService>();

// OrderItems
builder.Services.AddScoped<IOrderItemsGetterService, OrderItemsGetterService>();
builder.Services.AddScoped<IOrderItemsAdderService, OrderItemsAdderService>();
builder.Services.AddScoped<IOrderItemsUpdaterService, OrderItemsUpdaterService>();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

#endregion


var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();

app.Run();
