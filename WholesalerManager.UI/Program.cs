using Microsoft.EntityFrameworkCore;
using WholesaleManager.Infrastructure.DatabaseContext;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Repositories
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ISuppliersRepository, SuppliersRepository>();
builder.Services.AddScoped<IDeliveriesRepository, DeliveriesRepository>();
builder.Services.AddScoped<IDeliveryItemsRepository, DeliveryItemsRepository>();

//Services
builder.Services.AddScoped<IProductsGetterService, ProductsGetterService>();
builder.Services.AddScoped<IProductsAdderService, ProductsAdderService>();
builder.Services.AddScoped<IProductsDeleterService, ProductsDeleterService>();
builder.Services.AddScoped<IProductsUpdaterService, ProductsUpdaterService>();

builder.Services.AddScoped<ICategoriesGetterService, CategoriesGetterService>();

builder.Services.AddScoped<ISuppliersGetterService, SuppliersGetterService>();

builder.Services.AddScoped<IDeliveriesGetterService, DeliveriesGetterService>();
builder.Services.AddScoped<IDeliveriesAdderService, DeliveriesAdderService>();
builder.Services.AddScoped<IDeliveriesUpdaterService, DeliveriesUpdaterService>();

builder.Services.AddScoped<IDeliveryItemsGetterService, DeliveryItemsGetterService>();
builder.Services.AddScoped<IDeliveryItemsAdderService, DeliveryItemsAdderService>();
builder.Services.AddScoped<IDeliveryItemsUpdaterService, DeliveryItemsUpdaterService>();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


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
