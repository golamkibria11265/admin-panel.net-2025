using Dll.Models.ViewModels;
using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Bll.Services.CommonService
{
    public interface ICommonService
    {
        Task<ApiResponse> GetRoleList();
    }
    public class CommonService : ICommonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        public CommonService(IUnitOfWork unitOfWork, Serilog.ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse> GetRoleList()
        {
            ApiResponse apiResponse = new();
            List<DropdownViewModel> list = new();
            var data = await _unitOfWork.RoleRepository.FindAll();
            foreach (var item in data)
            {
                DropdownViewModel dropdownViewModel = new()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                };
                list.Add(dropdownViewModel);
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>();
            apiResponse.data = list;
            return apiResponse;
        }
    }
}
