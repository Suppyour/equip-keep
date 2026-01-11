using InventoryManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<InventoryDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            // context.Database.EnsureCreated(); // Already called in Program.cs

            // Seed Roles
            string[] roles = new[] { "Admin", "DepartmentManager", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (context.Employees.Any())
            {
                return;
            }

            var employees = new Employee[]
            {
                new Employee{FullName="Порсев Михаил123", Department="IT", UserId = adminUser?.Id}, // Link Admin to this employee
                new Employee{FullName="Панова Татьяна", Department="Бухгалтерия"},
                new Employee{FullName="Галкин Григорий Михайлович", Department="Разработка"},
            };

            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
        }
    }
}
