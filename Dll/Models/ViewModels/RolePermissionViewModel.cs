using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class RolePermissionViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public int? MenuId { get; set; }
        public int? ParentId { get; set; }
        public Boolean IsSelected { get; set; }
        public List<RolePermissionViewModel>? ChildRolePermissionViewModels { get; set; }
    }
}
