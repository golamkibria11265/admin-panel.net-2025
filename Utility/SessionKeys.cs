namespace Utility
{
    /// <summary>Names of the values stored in the ASP.NET Core session.</summary>
    public static class SessionKeys
    {
        /// <summary>ApUser.Id of the signed-in user. Set LAST during login; its presence means "authenticated".</summary>
        public const string UserId = "Id";
        public const string UserName = "UserName";
        public const string RoleName = "RoleName";
        public const string RoleWisePermission = "RoleWisePermission";
        public const string RoleWiseMenuPermission = "RoleWiseMenuPermission";
    }
}
