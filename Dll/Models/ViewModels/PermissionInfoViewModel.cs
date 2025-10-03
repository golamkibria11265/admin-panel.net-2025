using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class PermissionInfoViewModel
    {
        public long PermissionId { get; set; }
        public long MenuId { get; set; }
        public int? ParentMenuId { get; set; }
        public string? MenuName { get; set; }
        public string? MenuUrl { get; set; }
        public string? Icon { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public int? IsAction { get; set; }
        public int? Order { get; set; }
    }
}
