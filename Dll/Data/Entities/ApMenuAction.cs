using System;
using System.Collections.Generic;

namespace Dll.Data.Entities;

public partial class ApMenuAction
{
    public long Id { get; set; }

    public long MenuId { get; set; }

    public string? Url { get; set; }

    public string? ActionName { get; set; }

    public int? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
