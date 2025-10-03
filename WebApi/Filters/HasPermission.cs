using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Dll.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Dll.Models.ViewModels;
using System.Text.Json;

namespace WebApi.Filters
{
    public class HasPermission : Attribute, IAsyncActionFilter
    {
        private readonly IHttpContextAccessor _iHttpContextAccessor;
        private readonly Serilog.ILogger _logger;
        private readonly DotNetContext _dotNetContext;
        public HasPermission(IHttpContextAccessor iHttpContextAccessor, Serilog.ILogger logger, DotNetContext dotNetContext)
        {
            _iHttpContextAccessor = iHttpContextAccessor;
            _logger = logger;
            _dotNetContext = dotNetContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                if (context.Controller is not Controller controller)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Action",
                        controller = "Error"
                    }));
                    return;
                }
                string actionName = controller.ControllerContext.ActionDescriptor.ActionName.ToString();
                string controllerName = controller.ControllerContext.ActionDescriptor.ControllerName.ToString();
                string menuURL = controllerName + "/" + actionName;

                var sessionInfo = _iHttpContextAccessor.HttpContext?.Session.Keys.ToList();

                string? userId = null;
                if (sessionInfo != null && sessionInfo.Contains("Id"))
                {
                    userId = _iHttpContextAccessor.HttpContext?.Session.GetString("Id");
                }
                var permissionString = _iHttpContextAccessor.HttpContext?.Session.GetString("RoleWisePermission");
                var roleWisePermissions = new List<PermissionInfoViewModel>();
                if (permissionString != null)
                {
                    roleWisePermissions = JsonSerializer.Deserialize<List<PermissionInfoViewModel>>(permissionString ?? string.Empty);
                }
                else
                {
                    List<int> roleIds = new List<int> { };
                    var userRoleInfo = await _dotNetContext.ApUserRoles.FirstOrDefaultAsync(x => x.UserId == Convert.ToInt64(userId));
                    if (userRoleInfo != null)
                    {
                        roleIds.Add(userRoleInfo.RoleId);
                    }
                    var data = (from role in _dotNetContext.ApRoles
                                join rolePermission in _dotNetContext.ApRolePermissions on role.Id equals rolePermission.RoleId
                                join permission in _dotNetContext.ApPermissions on rolePermission.PermissionId equals permission.Id
                                join menu in _dotNetContext.ApMenus on permission.MenuId equals menu.Id
                                where roleIds.Contains(role.Id)
                                select new PermissionInfoViewModel
                                {
                                    PermissionId = permission.Id,
                                    MenuId = menu.Id,
                                    ParentMenuId = menu.Parent,
                                    MenuName = menu.Name,
                                    MenuUrl = menu.Url,
                                    Icon = "fa fa-times",
                                    ControllerName = menu.Url != null ? menu.Url.Substring(0, menu.Url.IndexOf('/')) : "",
                                    ActionName = menu.Url != null ? menu.Url.Substring(menu.Url.IndexOf('/') + 1) : "",
                                    IsAction = menu.IsAction,
                                    Order = menu.MenuOrder
                                }).Distinct();
                    roleWisePermissions = data.ToList();
                }
                if (roleWisePermissions == null || !roleWisePermissions.Any(a => a.ControllerName!.ToLower() == controllerName.ToLower()) || !roleWisePermissions.Any(a => a.ActionName!.ToLower() == actionName.ToLower()))
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Action",
                        controller = "Error"
                    }));
                }
                ActionExecutedContext resultContext = await next();

                if (resultContext.Controller is Controller controller_result)
                {
                    if (roleWisePermissions == null || !roleWisePermissions.Any(a => a.ControllerName!.ToLower() == controllerName.ToLower()) || !roleWisePermissions.Any(a => a.ActionName!.ToLower() == actionName.ToLower()))
                    {
                        context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            action = "Action",
                            controller = "Error"
                        }));
                    }
                }
                else
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "Action",
                        controller = "Error"
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "Action",
                    controller = "Error"
                }));
            }
        }
    }
}
