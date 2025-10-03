using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Utility;

namespace Bll.Services.CommonService
{
    public interface IDataSeedingService
    {
        void SeedData();
    }
    public class DataSeedingService : IDataSeedingService
    {
        private readonly string _connectionString;
        private readonly Serilog.ILogger _logger;
        public DataSeedingService
        (
            IConfiguration configuration,
            Serilog.ILogger logger
        )
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }
        public async void SeedData()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string insertAdminQuery = @"
                        INSERT INTO DotNet.dbo.ApUser 
                        (UserId, UserName, FirstName, LastName, Email, Mobile, Password, IsPasswordReset, Type, Status, MetaData, OtpRequest, IsActiveDirectoryEnabled, IsActive, CreatedBy, CreatedDate)
                        VALUES (@UserId, @UserName, @FirstName, @LastName, @Email, @Mobile, @Password, @IsPasswordReset, @Type, @Status, @MetaData, @OtpRequest, @IsActiveDirectoryEnabled, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertAdminQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@UserId", 0);
                        insertCommand.Parameters.AddWithValue("@UserName", "admin");
                        insertCommand.Parameters.AddWithValue("@FirstName", "admin");
                        insertCommand.Parameters.AddWithValue("@LastName", "user");
                        insertCommand.Parameters.AddWithValue("@Email", "admin@example.com");
                        insertCommand.Parameters.AddWithValue("@Mobile", "1234567890");
                        insertCommand.Parameters.AddWithValue("@Password", ProjectHelper.GenerateHashPassword("admin@1234"));
                        insertCommand.Parameters.AddWithValue("@IsPasswordReset", 0);
                        insertCommand.Parameters.AddWithValue("@Type", 1);
                        insertCommand.Parameters.AddWithValue("@Status", 1);
                        insertCommand.Parameters.AddWithValue("@MetaData", "{}");
                        insertCommand.Parameters.AddWithValue("@OtpRequest", 0);
                        insertCommand.Parameters.AddWithValue("@IsActiveDirectoryEnabled", 0);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertAdminRoleQuery = @"
                        INSERT INTO DotNet.dbo.ApRole (Name, Description, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @Description, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertAdminRoleQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Name", "admin");
                        insertCommand.Parameters.AddWithValue("@Description", "Administrator role with full permissions");
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertUserRoleQuery = @"
                        INSERT INTO DotNet.dbo.ApUserRole (UserId, RoleId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@UserId, @RoleId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertUserRoleQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@UserId", 1);
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }


                    string insertAdminMenuQuery_09 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_09, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", DBNull.Value);
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "User Management");
                        command.Parameters.AddWithValue("@MenuOrder", 0);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "dashboard.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_10 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_10, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "home/index");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Dashboard");
                        command.Parameters.AddWithValue("@MenuOrder", 1);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "dashboard.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_11 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_11, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "menu/index");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Menu");
                        command.Parameters.AddWithValue("@MenuOrder", 2);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "menu.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_12 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_12, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "rolePermission/index");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Role Permission");
                        command.Parameters.AddWithValue("@MenuOrder", 3);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "menu.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_13 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_13, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "menu/add");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Add");
                        command.Parameters.AddWithValue("@MenuOrder", 4);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "menu.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_14 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_14, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "menu/edit");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Edit");
                        command.Parameters.AddWithValue("@MenuOrder", 5);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "menu.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_15 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_15, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "menu/delete");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Delete");
                        command.Parameters.AddWithValue("@MenuOrder", 6);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "menu.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertAdminMenuQuery_16 = @"
                        INSERT INTO DotNet.dbo.ApMenu (Url, Parent, Name, MenuOrder, IsAction, IsActive, Image, CreatedBy, CreatedDate)
                        VALUES (@Url, @Parent, @Name, @MenuOrder, @IsAction, @IsActive, @Image, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertAdminMenuQuery_16, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Url", "permission/index");
                        command.Parameters.AddWithValue("@Parent", DBNull.Value);
                        command.Parameters.AddWithValue("@Name", "Permission");
                        command.Parameters.AddWithValue("@MenuOrder", 7);
                        command.Parameters.AddWithValue("@IsAction", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@Image", "menu.png");
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }


                    string insertPermissionQuery_09 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_09, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "User Management");
                        command.Parameters.AddWithValue("@MenuId", 0);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_10 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_10, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "Dashboard");
                        command.Parameters.AddWithValue("@MenuId", 1);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_11 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_11, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "Menu");
                        command.Parameters.AddWithValue("@MenuId", 2);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_12 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_12, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "Role Permission");
                        command.Parameters.AddWithValue("@MenuId", 3);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_13 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_13, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "Add");
                        command.Parameters.AddWithValue("@MenuId", 4);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_14 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_14, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "Edit");
                        command.Parameters.AddWithValue("@MenuId", 5);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_15 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_15, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "Delete");
                        command.Parameters.AddWithValue("@MenuId", 6);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertPermissionQuery_16 = @"
                        INSERT INTO DotNet.dbo.ApPermission (Name, MenuId, IsActive, CreatedBy, CreatedDate)
                        VALUES (@Name, @MenuId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var command = new SqlCommand(insertPermissionQuery_16, connection))
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Name", "permission");
                        command.Parameters.AddWithValue("@MenuId", 7);
                        command.Parameters.AddWithValue("@IsActive", 1);
                        command.Parameters.AddWithValue("@CreatedBy", 1);
                        await command.ExecuteNonQueryAsync();
                    }


                    string insertRolePermissionQuery_09 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_09, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 0);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_10 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_10, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 1);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_11 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_11, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 2);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_12 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_12, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 3);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_13 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_13, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 4);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_14 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_14, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 5);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_15 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_15, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 6);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    string insertRolePermissionQuery_16 = @"
                        INSERT INTO DotNet.dbo.ApRolePermission 
                        (RoleId, PermissionId, IsActive, CreatedBy, CreatedDate) 
                        VALUES (@RoleId, @PermissionId, @IsActive, @CreatedBy, GETUTCDATE())";
                    using (var insertCommand = new SqlCommand(insertRolePermissionQuery_16, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@RoleId", 1);
                        insertCommand.Parameters.AddWithValue("@PermissionId", 7);
                        insertCommand.Parameters.AddWithValue("@IsActive", 1);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", 1);
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Information("Data Seeding Exception: " + JsonConvert.SerializeObject(ex));
            }
        }
    }
}
