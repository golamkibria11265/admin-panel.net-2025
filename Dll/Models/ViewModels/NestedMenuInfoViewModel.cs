using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class NestedMenuInfoViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionName { get; set; }
        public string? ControllerName { get; set; }
        public int? Order { get; set; }
        public List<NestedMenuInfoViewModel>? ChildMenuViewModels { get; set; }
    }
}
