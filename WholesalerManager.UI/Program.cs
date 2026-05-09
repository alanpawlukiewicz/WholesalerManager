using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore;
using WholesaleManager.Infrastructure.DatabaseContext;
using WholesalerManager.Core.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.ProductServices;
using WholesaleManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Repositories
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();

//Services
builder.Services.AddScoped<IProductsGetterService, ProductsGetterService>();

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
