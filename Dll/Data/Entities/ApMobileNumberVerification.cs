using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dll.Data.Entities;

[Table("ApMobileNumberVerification")]
public class ApMobileNumberVerification
{
    public int Id { get; set; }

    public string? MobileNumber { get; set; }

    public bool? IsMobileNumberVerified { get; set; }

    public string? Otp { get; set; }

    public int? OtpCount { get; set; }

    public DateTimeOffset? OtpExpireTime { get; set; }
}
