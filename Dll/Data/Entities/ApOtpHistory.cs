using System;
using System.Collections.Generic;

namespace Dll.Data.Entities;

public partial class ApOtpHistory
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string? Ikey { get; set; }

    public string? Ivalue { get; set; }

    public int? Otp { get; set; }

    public int? OtpCount { get; set; }

    public DateTimeOffset? ExpireAt { get; set; }

    public string? Status { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Remarks { get; set; }
}
