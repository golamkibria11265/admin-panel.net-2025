using System;
using System.Collections.Generic;

namespace Dll.Data.Entities;

public partial class ApUser
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Mobile { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public bool? IsPasswordReset { get; set; }

    public byte? Type { get; set; }

    public byte? Status { get; set; }

    public string? MetaData { get; set; }

    public bool? OtpRequest { get; set; }

    public bool IsActiveDirectoryEnabled { get; set; }

    public int? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
