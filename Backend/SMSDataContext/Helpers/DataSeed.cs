using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SMSDataContext.Data;
using SMSDataModel.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSDataContext.Helpers
{
    public class SeedData
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            string[] roles = { "SuperAdmin", "Admin", "Teacher", "Student", "Parent" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }

        public static async Task SeedUsersAndSchool(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<DataContext>();

            // Create schools if they don't exist
            if (!context.Schools.Any())
            {
                var schools = new List<School>
                {
                    new School
                    {
                        Id = Guid.NewGuid(),
                        RegistrationNumber = "SCH001",
                        Name = "Greenwood High School",
                        Email = "contact@greenwoodhigh.com",
                        Phone = "9876543210",
                        Address = "123 Education Lane",
                        City = "Springfield",
                        State = "California",
                        PinCode = 900001,
                        Subscription = 1,
                        SubscriptionDate = DateOnly.FromDateTime(DateTime.Now),
                        IsSoftDeleted = false
                    },
                    new School
                    {
                        Id = Guid.NewGuid(),
                        RegistrationNumber = "SCH002",
                        Name = "Sunrise Academy",
                        Email = "info@sunriseacademy.com",
                        Phone = "9123456789",
                        Address = "456 Knowledge Street",
                        City = "Riverside",
                        State = "Texas",
                        PinCode = 750001,
                        Subscription = 1,
                        SubscriptionDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
                        IsSoftDeleted = false
                    },
                    new School
                    {
                        Id = Guid.NewGuid(),
                        RegistrationNumber = "SCH003",
                        Name = "Maple Leaf International School",
                        Email = "admin@mapleleaf.edu",
                        Phone = "9234567890",
                        Address = "789 Learning Boulevard",
                        City = "Portland",
                        State = "Oregon",
                        PinCode = 970001,
                        Subscription = 1,
                        SubscriptionDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
                        IsSoftDeleted = false
                    }
                };

                context.Schools.AddRange(schools);
                await context.SaveChangesAsync();

                // Use the first school for demo users
                var school = schools[0];

                // Create dummy users
                var users = new[]
                {
                    new { Email = "superadmin@school.com", UserName = "superadmin@school.com", Role = "SuperAdmin", SchoolId = school.Id },
                    new { Email = "admin@school.com", UserName = "admin@school.com", Role = "Admin", SchoolId = school.Id },
                    new { Email = "teacher@school.com", UserName = "teacher@school.com", Role = "Teacher", SchoolId = school.Id },
                    new { Email = "student@school.com", UserName = "student@school.com", Role = "Student", SchoolId = school.Id },
                    new { Email = "parent@school.com", UserName = "parent@school.com", Role = "Parent", SchoolId = school.Id }
                };

                foreach (var userInfo in users)
                {
                    var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
                    if (existingUser == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = userInfo.UserName,
                            Email = userInfo.Email,
                            EmailConfirmed = true,
                            SchoolId = userInfo.SchoolId,
                            CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                        };

                        var result = await userManager.CreateAsync(user, "Password@123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, userInfo.Role);
                        }
                    }
                }
            }
        }
    }

}

