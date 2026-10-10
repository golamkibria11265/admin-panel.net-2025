using Dll.Data;
using Dll.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Utility;

namespace Bll.Services.CommonService
{
    public interface IDataSeedingService
    {
        /// <summary>
        /// Creates the first admin user, the admin role, the default menus and their permissions.
        /// Does nothing if a user named "admin" already exists, so running it twice is safe.
        /// </summary>
        Task SeedDataAsync();
    }

    public class DataSeedingService : IDataSeedingService
    {
        private const string AdminUserName = "admin";
        private const string AdminRoleName = "admin";
        private readonly DotNetContext _dotNetContext;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;

        public DataSeedingService(DotNetContext dotNetContext, IConfiguration configuration, Serilog.ILogger logger)
        {
            _dotNetContext = dotNetContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            if (await _dotNetContext.ApUsers.AnyAsync(u => u.UserName == AdminUserName))
            {
                _logger.Information("Data seeding skipped: user '{UserName}' already exists", AdminUserName);
                return;
            }

            // The initial password is never stored in the code. Provide it once through the environment variable
            // Seed__AdminPassword and change it after the first login.
            string? adminPassword = _configuration["Seed:AdminPassword"];
            if (string.IsNullOrEmpty(adminPassword) || adminPassword.Length < 12 ||
                !Regex.IsMatch(adminPassword, "[A-Za-z]") || !Regex.IsMatch(adminPassword, "[0-9]"))
            {
                throw new InvalidOperationException(
                    "Set Seed:AdminPassword (environment variable Seed__AdminPassword) to at least 12 characters " +
                    "containing letters and digits before seeding.");
            }

            DateTime now = DateTime.UtcNow;
            await using var transaction = await _dotNetContext.Database.BeginTransactionAsync();

            var admin = new ApUser
            {
                UserId = 0,
                UserName = AdminUserName,
                FirstName = "admin",
                LastName = "user",
                Email = "admin@example.com",
                Password = ProjectHelper.GenerateHashPassword(adminPassword),
                IsPasswordReset = true, // must be changed after the first login
                Type = 1,
                Status = 1,
                MetaData = "{}",
                OtpRequest = false,
                IsActiveDirectoryEnabled = false,
                IsActive = 1,
                CreatedBy = 1,
                CreatedDate = now
            };
            var adminRole = new ApRole
            {
                Name = AdminRoleName,
                Description = "Administrator role with full permissions",
                IsActive = 1,
                CreatedBy = 1,
                CreatedDate = now
            };
            _dotNetContext.ApUsers.Add(admin);
            _dotNetContext.ApRoles.Add(adminRole);
            await _dotNetContext.SaveChangesAsync();

            _dotNetContext.ApUserRoles.Add(new ApUserRole { UserId = admin.Id, RoleId = adminRole.Id, IsActive = 1, CreatedBy = 1, CreatedDate = now });

            // Menus. Rows with IsAction = 1 are buttons (not shown in the sidebar) but still need a permission.
            ApMenu NewMenu(string name, string? url, int order, int isAction = 0, string? image = null) => new()
            {
                Name = name,
                Url = url,
                MenuOrder = order,
                IsAction = isAction,
                IsActive = 1,
                Image = image,
                CreatedBy = 1,
                CreatedDate = now
            };
            var dashboard = NewMenu("Dashboard", "home/index", 1, image: "dashboard.png");
            var userManagement = NewMenu("User Management", null, 2, image: "dashboard.png");
            _dotNetContext.ApMenus.AddRange(dashboard, userManagement);
            await _dotNetContext.SaveChangesAsync();

            var menu = NewMenu("Menu", "menu/index", 1);
            var rolePermission = NewMenu("Role Permission", "rolePermission/index", 2);
            var permission = NewMenu("Permission", "permission/index", 3);
            var createUser = NewMenu("Create User", "user/signup", 4);
            foreach (var child in new[] { menu, rolePermission, permission, createUser })
            {
                child.Parent = userManagement.Id;
            }
            _dotNetContext.ApMenus.AddRange(menu, rolePermission, permission, createUser);
            await _dotNetContext.SaveChangesAsync();

            var add = NewMenu("Add", "menu/add", 1, isAction: 1);
            var edit = NewMenu("Edit", "menu/edit", 2, isAction: 1);
            var delete = NewMenu("Delete", "menu/delete", 3, isAction: 1);
            foreach (var action in new[] { add, edit, delete })
            {
                action.Parent = menu.Id;
            }
            _dotNetContext.ApMenus.AddRange(add, edit, delete);
            await _dotNetContext.SaveChangesAsync();

            // One permission per menu, all granted to the admin role.
            var allMenus = new[] { dashboard, userManagement, menu, rolePermission, permission, createUser, add, edit, delete };
            var permissions = allMenus.Select(m => new ApPermission
            {
                Name = m.Name,
                MenuId = m.Id,
                IsActive = 1,
                CreatedBy = 1,
                CreatedDate = now
            }).ToList();
            _dotNetContext.ApPermissions.AddRange(permissions);
            await _dotNetContext.SaveChangesAsync();

            _dotNetContext.ApRolePermissions.AddRange(permissions.Select(p => new ApRolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id,
                IsActive = 1,
                CreatedBy = 1,
                CreatedDate = now
            }));
            await _dotNetContext.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.Information("Data seeding completed: admin user, role, {MenuCount} menus and permissions created", allMenus.Length);
        }
    }
}
