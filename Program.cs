﻿using BalloonWorld.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BalloonWorld.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BalloonWorldContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BalloonWorldContext") ?? throw new InvalidOperationException("Connection string 'BalloonWorldContext' not found.")));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope()) 
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] {"User", "Admin"};

    foreach(var role in roles) 
    {
        if (!await roleManager.RoleExistsAsync(role)) 
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

using (var scope = app.Services.CreateScope()) 
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string email = "admin@balloonworld.com";
    string password = "BALLOONSworld123!";

    if (await userManager.FindByEmailAsync(email) == null) 
    {
        var adminUser = new IdentityUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, password);

        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();
