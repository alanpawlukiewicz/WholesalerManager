using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using WholesaleManager.Infrastructure.Repositories;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Domain.PersistenceContracts;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.ServiceContracts;
using WholesalerManager.Core.ServiceContracts.CategoriesServiceContracts;
using WholesalerManager.Core.ServiceContracts.CustomerServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.DeliveryServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.OrderServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.ServiceContracts.SupplierServiceContracts;
using WholesalerManager.Core.ServiceContracts.UserServiceContracts;
using WholesalerManager.Core.Services;
using WholesalerManager.Core.Services.CategoriesServices;
using WholesalerManager.Core.Services.CustomerServices;
using WholesalerManager.Core.Services.DeliveryItemServices;
using WholesalerManager.Core.Services.DeliveryServices;
using WholesalerManager.Core.Services.OrderItemServices;
using WholesalerManager.Core.Services.OrderServices;
using WholesalerManager.Core.Services.ProductServices;
using WholesalerManager.Core.Services.SupplierServices;
using WholesalerManager.Core.Services.UserServices;
using WholesalerManager.Infrastructure.DatabaseContext;
using WholesalerManager.Infrastructure.Persistence;
using WholesalerManager.Infrastructure.Repositories;
using WholesalerManager.UI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((hostingContext, services, configuration) =>
{
    configuration.ReadFrom.Configuration(hostingContext.Configuration).ReadFrom.Services(services);

});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

#region Repositories
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ISuppliersRepository, SuppliersRepository>();
builder.Services.AddScoped<IDeliveriesRepository, DeliveriesRepository>();
builder.Services.AddScoped<IDeliveryItemsRepository, DeliveryItemsRepository>();
builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IOrderItemsRepository, OrderItemsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

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
builder.Services.AddScoped<IDeliveryRegistrationService, DeliveryRegistrationService>();
builder.Services.AddScoped<IDeliveryUpdateControllerService, DeliveryUpdateControllerService>();

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
builder.Services.AddScoped<IOrdersStockCheckerService, OrdersStockCheckerService>();
builder.Services.AddScoped<IOrderRegistrationService, OrderRegistrationService>();
builder.Services.AddScoped<IOrderUpdateCoordinatorService, OrderUpdateCoordinatorService>();

// OrderItems
builder.Services.AddScoped<IOrderItemsGetterService, OrderItemsGetterService>();
builder.Services.AddScoped<IOrderItemsAdderService, OrderItemsAdderService>();
builder.Services.AddScoped<IOrderItemsUpdaterService, OrderItemsUpdaterService>();

// Username and password generation
builder.Services.AddScoped<IUserNameGeneratorService, UserNameGeneratorService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();

// Email sending
builder.Services.AddScoped<IEmailService, EmailService>();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Indetity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    options.AddPolicy("RequireNotAuthenticated", policy => policy.RequireAssertion(context => !context.User?.Identity?.IsAuthenticated ?? false));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IUsersGetterService, UsersGetterService>();
builder.Services.AddScoped<IUsersUpdaterService, UsersUpdaterService>();
builder.Services.AddScoped<IUsersRegistrationService, UsersRegistrationService>();
builder.Services.AddScoped<IUsersDeleterService, UsersDeleterService>();

builder.Services.AddHttpContextAccessor();

// Logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

#endregion



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseHttpLogging();

// Rotativa configuration for making PDF files from views
Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (httpContext.Request.Method == "GET" && elapsed < 500)
            return LogEventLevel.Debug;

        return LogEventLevel.Information;
    };
});

app.MapControllers();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
