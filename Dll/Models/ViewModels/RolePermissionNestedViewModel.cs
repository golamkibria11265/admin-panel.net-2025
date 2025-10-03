using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class RolePermissionNestedViewModel
    {
        public int RoleId { get; set; }
        public List<RolePermissionViewModel>? RolePermissionNestedViewModelList { get; set; }
    }
}
