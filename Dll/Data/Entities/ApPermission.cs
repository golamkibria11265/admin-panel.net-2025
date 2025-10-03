using System;
using System.Collections.Generic;

namespace Dll.Data.Entities;

public partial class ApPermission
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? MenuId { get; set; }

    public int? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
