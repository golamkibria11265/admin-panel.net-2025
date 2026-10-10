using Dll.Data;
using Dll.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Bll.Services.ApService
{
    /// <summary>
    /// Single source of truth for "is this user allowed to do X".
    /// Permissions are read from the database on every request (cached per request only),
    /// so revoking a role permission or disabling a user takes effect immediately.
    /// </summary>
    public interface IAccessControlService
    {
        Task<bool> IsUserActiveAsync(long userId);
        Task<IReadOnlyList<PermissionInfoViewModel>> GetPermissionsAsync(long userId);
        Task<bool> HasPermissionAsync(long userId, string controllerName, string actionName);
    }

    public class AccessControlService : IAccessControlService
    {
        private readonly DotNetContext _dotNetContext;
        private readonly Dictionary<long, IReadOnlyList<PermissionInfoViewModel>> _permissionCache = new();
        private readonly Dictionary<long, bool> _activeCache = new();

        public AccessControlService(DotNetContext dotNetContext)
        {
            _dotNetContext = dotNetContext;
        }

        public async Task<bool> IsUserActiveAsync(long userId)
        {
            if (userId <= 0)
            {
                return false;
            }
            if (_activeCache.TryGetValue(userId, out var cached))
            {
                return cached;
            }
            var isActive = await _dotNetContext.ApUsers
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.IsActive)
                .FirstOrDefaultAsync();
            var result = UserStatusHelper.IsActive(isActive);
            _activeCache[userId] = result;
            return result;
        }

        public async Task<IReadOnlyList<PermissionInfoViewModel>> GetPermissionsAsync(long userId)
        {
            if (userId <= 0)
            {
                return Array.Empty<PermissionInfoViewModel>();
            }
            if (_permissionCache.TryGetValue(userId, out var cached))
            {
                return cached;
            }

            // All active roles of the user (a user can have more than one role).
            var roleIds = await _dotNetContext.ApUserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && (ur.IsActive == null || ur.IsActive == 1))
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToListAsync();

            if (roleIds.Count == 0)
            {
                _permissionCache[userId] = Array.Empty<PermissionInfoViewModel>();
                return _permissionCache[userId];
            }

            var rows = await (from role in _dotNetContext.ApRoles
                              join rolePermission in _dotNetContext.ApRolePermissions on role.Id equals rolePermission.RoleId
                              join permission in _dotNetContext.ApPermissions on rolePermission.PermissionId equals permission.Id
                              join menu in _dotNetContext.ApMenus on permission.MenuId equals menu.Id
                              where roleIds.Contains(role.Id)
                                    && (role.IsActive == null || role.IsActive == 1)
                                    && (rolePermission.IsActive == null || rolePermission.IsActive == 1)
                                    && (permission.IsActive == null || permission.IsActive == 1)
                                    && (menu.IsActive == null || menu.IsActive == 1)
                              select new
                              {
                                  PermissionId = permission.Id,
                                  MenuId = menu.Id,
                                  menu.Parent,
                                  menu.Name,
                                  menu.Url,
                                  menu.IsAction,
                                  menu.MenuOrder
                              })
                              .Distinct()
                              .ToListAsync();

            var permissions = rows
                .Select(r =>
                {
                    MenuUrlHelper.TrySplit(r.Url, out var controllerName, out var actionName);
                    return new PermissionInfoViewModel
                    {
                        PermissionId = r.PermissionId,
                        MenuId = r.MenuId,
                        ParentMenuId = r.Parent,
                        MenuName = r.Name,
                        MenuUrl = r.Url,
                        Icon = "fa fa-times",
                        ControllerName = controllerName,
                        ActionName = actionName,
                        IsAction = r.IsAction,
                        Order = r.MenuOrder
                    };
                })
                // A menu can be reachable through several roles; keep one entry per menu.
                .GroupBy(p => p.MenuId)
                .Select(g => g.First())
                .ToList();

            _permissionCache[userId] = permissions;
            return permissions;
        }

        public async Task<bool> HasPermissionAsync(long userId, string controllerName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(controllerName) || string.IsNullOrWhiteSpace(actionName))
            {
                return false;
            }
            if (!await IsUserActiveAsync(userId))
            {
                return false;
            }
            var permissions = await GetPermissionsAsync(userId);
            // Controller AND action must match on the SAME permission entry.
            return permissions.Any(p =>
                string.Equals(p.ControllerName, controllerName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class UserStatusHelper
    {
        /// <summary>Only users explicitly marked active (IsActive = 1) may sign in.</summary>
        public static bool IsActive(int? isActive) => isActive == 1;
    }

    public static class MenuUrlHelper
    {
        /// <summary>Splits a menu URL like "menu/index" or "/Menu/Index/5" into controller and action.</summary>
        public static bool TrySplit(string? url, out string controllerName, out string actionName)
        {
            controllerName = string.Empty;
            actionName = string.Empty;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            var parts = url.Trim().Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return false;
            }
            controllerName = parts[0];
            actionName = parts[1];
            return true;
        }
    }
}
