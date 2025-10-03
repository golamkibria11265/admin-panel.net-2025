using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dll.Models.ViewModels
{
    public class MenuViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public int? Parent { get; set; }

        public string? ParentName { get; set; }

        public int? MenuOrder { get; set; }

        public IFormFile? Image { get; set; }

        public string? ImageUrl { get; set; }

        public Boolean IsAction { get; set; }

        public int? IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
