# This script will create a simple seeding program
$code = @"
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SMSDataContext.Data;
using SMSDataContext.Helpers;
using SMSDataModel.Model.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

var serviceProvider = services.BuildServiceProvider();

await SeedData.SeedRoles(serviceProvider);
await SeedData.SeedUsersAndSchool(serviceProvider);

Console.WriteLine("Data seeded successfully!");
Console.WriteLine("");
Console.WriteLine("Login Credentials:");
Console.WriteLine("==================");
Console.WriteLine("SuperAdmin: superadmin@school.com / Password@123");
Console.WriteLine("Admin: admin@school.com / Password@123");
Console.WriteLine("Teacher: teacher@school.com / Password@123");
Console.WriteLine("Student: student@school.com / Password@123");
Console.WriteLine("Parent: parent@school.com / Password@123");
"@

Set-Content -Path "SMSPrototype1\SeedRunner.cs" -Value $code

# Run the seeding using dotnet script would require additional setup
Write-Host "Seed code created. Now attempting to run application normally..."
