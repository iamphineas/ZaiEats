using ZaiEats.Data;
using ZaiEats.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using ZaiEats.Services;
using PayFast;


var builder = WebApplication.CreateBuilder(args);

// DB setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity setup with roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)

    .AddRoles<IdentityRole>() // Add this to configure roles
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // No need for AddIdentity here

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

// Add PayFast configuration from appsettings.json
builder.Services.Configure<PayFastSettings>(builder.Configuration.GetSection("PayFastSettings"));
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Cart>();

var app = builder.Build();


// 🌟 ROLE + ADMIN SEEDING - now in an async block
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>(); // RoleManager should now be available
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();


    await SeedRolesAndAdminAsync(roleManager, userManager);
}

async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)

{
    string[] roles = { "Admin", "Customer", "Driver", "KitchenStaff", "Manager" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var customerEmail = "customer@zaieats.com";
    var customerPassword = "Customer@123";

    var existingCustomer = await userManager.FindByEmailAsync(customerEmail);
    if (existingCustomer == null)
    {
        var customerUser = new ApplicationUser
        {
            UserName = customerEmail,
            Email = customerEmail,
            EmailConfirmed = true,
            FullName = "Test Customer"
        };

        var result = await userManager.CreateAsync(customerUser, customerPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(customerUser, "Customer");
        }
    }

    var adminEmail = "admin@zaieats.com";
    var adminPassword = "Admin@123"; 

    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdmin == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Admin" 
        };


        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
var supportedCultures = new[] { new CultureInfo("af-ZA") }; // or "fr-FR" if preferred
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("af-ZA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
