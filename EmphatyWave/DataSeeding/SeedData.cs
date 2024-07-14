using EmphatyWave.Domain;
using EmphatyWave.Persistence.DataContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmphatyWave.Persistence.DataSeeding
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider, SuperAdminDto dto)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDataContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            Migrate(db);
            Seed(userManager,db, dto);
        }
        private static void Migrate(UserDataContext context)
        {
            context.Database.Migrate();
        }
        public static void Seed(UserManager<User> userManager, UserDataContext context, SuperAdminDto dto)
        {
            var seeded = false;
            SeedRoles(context, ref seeded);
            SeedSuperAdmin(userManager,context, dto, ref seeded);
        }
        private static void SeedRoles(UserDataContext context, ref bool seeded)
        {
            var roles = new List<string> { "Admin", "User" };
            int counter = 0;
            foreach (var role in roles)
            {
                if (!context.Roles.Any(i => i.Name == role))
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context), null, null, null, null);
                    var roleToStore = new IdentityRole(role);
                    var result = roleManager.CreateAsync(roleToStore).Result;
                    if (result.Succeeded)
                        counter++;
                }
            }
            if (counter == 2)
                seeded = true;
        }
        private static void SeedSuperAdmin(UserManager<User> userManager, IdentityDbContext<User> context, SuperAdminDto dto, ref bool seeded)
        {

            if (!context.Users.Any(i => i.Email == dto.Email))
            {
                var superAdmin = new User
                {
                    Email = dto.Email,
                    UserName = dto.UserName,
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(superAdmin, dto.Password).Result;
                if (result.Succeeded)
                {
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context), null, null, null, null);
                    var roleToStore = "Admin";
                    if (!context.Roles.Any(i => i.Name == roleToStore))
                    {
                        var adminRole = new IdentityRole(roleToStore);
                        var roleResult = roleManager.CreateAsync(adminRole).Result;
                        if (!roleResult.Succeeded)
                            return;
                        var assing = userManager.AddToRoleAsync(superAdmin, roleToStore).Result;
                        if (assing.Succeeded)
                            seeded = true;
                    }
                    else
                    {
                        var assing = userManager.AddToRoleAsync(superAdmin, roleToStore).Result;
                        if (assing.Succeeded)
                            seeded = true;
                    }
                }
            }
        }
    }
}
