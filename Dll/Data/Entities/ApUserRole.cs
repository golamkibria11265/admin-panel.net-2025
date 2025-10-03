using System;
using System.Collections.Generic;

namespace Dll.Data.Entities;

public partial class ApUserRole
{
    public int Id { get; set; }

    public long UserId { get; set; }

    public int RoleId { get; set; }

    public int? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
