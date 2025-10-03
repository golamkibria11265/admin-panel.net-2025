using System;
using System.Collections.Generic;

namespace Dll.Data.Entities;

public partial class ApMenu
{
    public int Id { get; set; }

    public string? Url { get; set; }

    public int? Parent { get; set; }

    public string? Name { get; set; }

    public int? MenuOrder { get; set; }

    public int? IsAction { get; set; }

    public int? IsActive { get; set; }

    public string? Image { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
