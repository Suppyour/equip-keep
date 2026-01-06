using InventoryManager.Models;

namespace InventoryManager.Data
{
    public static class DbInitializer
    {
        public static void Initialize(InventoryDbContext context)
        {
            if (context.Employees.Any())
            {
                return;
            }

            var employees = new Employee[]
            {
                new Employee{FullName="Порсев Михаил123", Department="IT"},
                new Employee{FullName="Панова Татьяна", Department="Бухгалтерия"},
                new Employee{FullName="Галкин Григорий Михайлович", Department="Разработка"},
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();
        }
    }
}
