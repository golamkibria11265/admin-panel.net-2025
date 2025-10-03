using Bll.Services.CommonService;
using Dll.Data;
using Dll.Data.Entities;
using Dll.Models.ViewModels;
using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Utility;
using Utility.Exceptions;

namespace Bll.Services.ApService
{
    public interface IUserService
    {
        Task<ApiResponse> IsFirstAdminExist(string userName, string password);
        Task<ApiResponse> GetCurrentUserInfo();
        Task<ApiResponse> IsUsernameTaken(string userName);
        Task<ApiResponse> AddUser(UserViewModel userInfoViewModel);
        Task<ApiResponse> GetUserInfos();
        Task<ApiResponse> GetUserInfoById(long id);
        Task<ApiResponse> EditUser(UserViewModel userInfoViewModel);
        Task<ApiResponse> getRoleByUserName(string userName);
        Task<ApiResponse> GetUserRoleWisePermissionList(string roleName);
        ApiResponse GetUserNestedMenuList(List<PermissionInfoViewModel> PermissionInfoList);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        private readonly IFileWriterService _fileWriterService;
        private readonly IHttpContextAccessor _iHttpContextAccesor;
        private readonly DotNetContext _dotNetContext;
        public UserService(IUnitOfWork unitOfWork, IHttpContextAccessor iHttpContextAccesor, Serilog.ILogger logger, IFileWriterService fileWriterService, DotNetContext dotNetContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fileWriterService = fileWriterService;
            _iHttpContextAccesor = iHttpContextAccesor;
            _dotNetContext = dotNetContext;
        }
        public async Task<ApiResponse> IsFirstAdminExist(string userName, string password)
        {
            ApiResponse apiResponse = new();
            var userInfo = await _unitOfWork.UserRepository.FindFirstOrDefault(x => x.UserName == userName);
            if (userInfo == null)
            {
                throw new CustomApplicationException("Invalid user name");
            }
            if (!ProjectHelper.VerifyPassword(userInfo.Password ?? string.Empty, password))
            {
                throw new CustomApplicationException("Invalid password");
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            return apiResponse;
        }
        public async Task<ApiResponse> GetCurrentUserInfo()
        {
            ApiResponse apiResponse = new();
            var sessionInfo = _iHttpContextAccesor.HttpContext?.Session.Keys.ToList();
            string? currentUserName = null;
            if (sessionInfo != null && sessionInfo.Count > 0)
            {
                currentUserName = _iHttpContextAccesor.HttpContext?.Session.GetString("UserName");
            }
            if (!string.IsNullOrWhiteSpace(currentUserName))
            {
                apiResponse.code = StatusCodes.Status200OK;
                apiResponse.messages = new List<string>() { "Success" };
                apiResponse.data = await _unitOfWork.UserRepository.FindFirstOrDefault(x => x.UserName == currentUserName);
            }
            return apiResponse;
        }
        public async Task<ApiResponse> IsUsernameTaken(string userName)
        {
            ApiResponse apiResponse = new();
            var user = await _unitOfWork.UserRepository.FindFirstOrDefault(x => x.UserName == userName);
            if (user != null)
            {
                throw new CustomApplicationException("This user name is taken");
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            return apiResponse;
        }
        public async Task<ApiResponse> AddUser(UserViewModel userInfoViewModel)
        {
            ApiResponse apiResponse = new();
            ApUser user = new()
            {
                UserName = userInfoViewModel.UserName,
                Email = userInfoViewModel.Email,
                Password = ProjectHelper.GenerateHashPassword(userInfoViewModel.Password ?? "12345"),
                Mobile = userInfoViewModel.Mobile,
            };
            await _unitOfWork.UserRepository.Create(user);
            await _unitOfWork.CompleteAsync();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            return apiResponse;
        }
        public async Task<ApiResponse> GetUserInfos()
        {
            ApiResponse apiResponse = new();
            List<UserViewModel> userInfoViewModels = new();
            var data = await _unitOfWork.UserRepository.FindAll();
            if (data.Any())
            {
                userInfoViewModels = data.Select(a => new UserViewModel
                {
                    Email = a.Email,
                    Id = a.Id,
                    Mobile = a.Mobile,
                    Password = a.Password,
                    Status = a.Status
                }).ToList();
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = userInfoViewModels;
            return apiResponse;
        }
        public async Task<ApiResponse> GetUserInfoById(long id)
        {
            ApiResponse apiResponse = new();
            UserViewModel userViewModel = new UserViewModel();
            var data = await _unitOfWork.UserRepository.FindFirstOrDefault(x => x.Id == id);
            if (data == null)
            {
                throw new CustomApplicationException("The user is not found to edit");
            };
            var currentUserInfo = await GetCurrentUserInfo();
            userViewModel = new UserViewModel
            {
                Email = data.Email,
                Id = data.Id,
                Mobile = data.Mobile,
                Password = data.Password,
                Status = data.Status
            };
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = userViewModel;
            return apiResponse;
        }
        public async Task<ApiResponse> EditUser(UserViewModel userInfoViewModel)
        {
            ApiResponse apiResponse = new();
            var currentUserInfo = await GetCurrentUserInfo();
            ApUser? user_info = await _unitOfWork.UserRepository.FindFirstOrDefault(x => x.Id == userInfoViewModel.Id);
            if (user_info == null)
            {
                throw new CustomApplicationException("The user is not found to edit");
            }
            user_info.Email = userInfoViewModel.Email;
            user_info.Mobile = userInfoViewModel.Mobile;
            await _unitOfWork.UserRepository.Update(user_info);
            await _unitOfWork.CompleteAsync();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            return apiResponse;
        }
        public async Task<ApiResponse> getRoleByUserName(string userName)
        {
            ApiResponse apiResponse = new();
            var userInfo = await _dotNetContext.ApUsers.FirstOrDefaultAsync(x => x.UserName == userName);
            if (userInfo == null)
            {
                throw new CustomApplicationException("The user is not found");
            }
            var userRoleInfo = await _dotNetContext.ApUserRoles.FirstOrDefaultAsync(x => x.UserId == userInfo.Id);
            if (userRoleInfo == null)
            {
                throw new CustomApplicationException("This user is not assigned for any role");
            }
            var roleInfo = await _dotNetContext.ApRoles.FirstOrDefaultAsync(x => x.Id == userRoleInfo.RoleId);
            if (roleInfo == null)
            {
                throw new CustomApplicationException("The user assigned role is not found");
            }
            if (roleInfo.Name == null)
            {
                throw new CustomApplicationException("Role name is not found");
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = roleInfo.Name;
            return apiResponse;
        }
        public async Task<ApiResponse> GetUserRoleWisePermissionList(string roleName)
        {
            ApiResponse apiResponse = new();
            List<int> roleIds = new List<int> { };
            var userRoleInfo = await _dotNetContext.ApRoles.FirstOrDefaultAsync(x => x.Name == roleName);
            if (userRoleInfo != null)
            {
                roleIds.Add(userRoleInfo.Id);
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
                            ControllerName = menu.Url != null && menu.Url.Contains("/") ? menu.Url.Substring(0, menu.Url.IndexOf('/')) : "",
                            ActionName = menu.Url != null && menu.Url.Contains("/") ? menu.Url.Substring(menu.Url.IndexOf('/') + 1) : "",
                            IsAction = menu.IsAction,
                            Order = menu.MenuOrder
                        }).Distinct();
            var roleWisePermissions = data.ToList();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = roleWisePermissions;
            return apiResponse;
        }
        public ApiResponse GetUserNestedMenuList(List<PermissionInfoViewModel> PermissionInfoList)
        {
            ApiResponse apiResponse = new();
            List<NestedMenuInfoViewModel> nestedMenuInfoList = new List<NestedMenuInfoViewModel>();
            foreach (var rootParent in PermissionInfoList)
            {
                if (rootParent.ParentMenuId == null)
                {
                    nestedMenuInfoList.Add(new NestedMenuInfoViewModel
                    {
                        Id = (int)rootParent.MenuId,
                        Name = rootParent.MenuName,
                        Url = rootParent.MenuUrl,
                        ImageUrl = "fa fa-times",
                        ControllerName = rootParent.ControllerName,
                        ActionName = rootParent.ActionName,
                        Order = rootParent.Order
                    });
                }
                if (nestedMenuInfoList.Any())
                {
                    foreach (var parent in nestedMenuInfoList)
                    {
                        var child = PermissionInfoList.Where(a => a.ParentMenuId == parent.Id && (a.IsAction == 0 || a.IsAction == null)).OrderBy(a => a.Order).ToList();
                        if (child.Count > 0)
                        {
                            List<ChildMenuViewModel> childMenuViewModels = new List<ChildMenuViewModel>();
                            foreach (var item in child)
                            {
                                childMenuViewModels.Add(new ChildMenuViewModel
                                {
                                    Id = (int)item.MenuId,
                                    Name = item.MenuName,
                                    Url = item.MenuUrl,
                                    ImageUrl = "fa fa-times",
                                    ControllerName = item.ControllerName,
                                    ActionName = item.ActionName,
                                    Order = item.Order
                                });
                            }
                            parent.ChildMenuViewModels = childMenuViewModels;
                        }
                    }
                }
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = nestedMenuInfoList;
            return apiResponse;
        }
    }
}
