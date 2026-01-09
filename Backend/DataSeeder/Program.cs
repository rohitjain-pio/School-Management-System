using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SMSDataContext.Data;
using SMSDataContext.Helpers;
using SMSDataModel.Model.Models;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("../SMSPrototype1/appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("Starting data seeding...");

try
{
    await SeedData.SeedRoles(serviceProvider);
    Console.WriteLine("✓ Roles seeded successfully");
    
    await SeedData.SeedUsersAndSchool(serviceProvider);
    Console.WriteLine("✓ Users and school seeded successfully");
    
    Console.WriteLine("\n===========================================");
    Console.WriteLine("Login Credentials:");
    Console.WriteLine("===========================================");
    Console.WriteLine("SuperAdmin: superadmin@school.com / Password@123");
    Console.WriteLine("Admin:      admin@school.com / Password@123");
    Console.WriteLine("Teacher:    teacher@school.com / Password@123");
    Console.WriteLine("Student:    student@school.com / Password@123");
    Console.WriteLine("Parent:     parent@school.com / Password@123");
    Console.WriteLine("===========================================\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
