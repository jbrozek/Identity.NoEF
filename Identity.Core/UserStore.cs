using Dapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Core
{
    public class UserStore<TUser> : IUserStore<TUser, Guid>,
        IUserPasswordStore<TUser, Guid>,
        IUserEmailStore<TUser, Guid>,
        IUserSecurityStampStore<TUser, Guid>,
        IUserRoleStore<TUser, Guid>,
        IUserLockoutStore<TUser, Guid>,
        IUserPhoneNumberStore<TUser, Guid>,
        IUserTwoFactorStore<TUser, Guid>,
        IUserLoginStore<TUser, Guid>,
        IQueryableUserStore<TUser, Guid>
        where TUser : IdentityUser
    {
        private string _connection;

        public UserStore(string connection)
        {
            _connection = connection;
        }

        /* IUserStore
        ---------------------------*/

        public Task CreateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (user.Audit == null)
                throw new ArgumentNullException("user.Audit");

            var sql = @"

BEGIN TRY
    BEGIN TRANSACTION

-- user
INSERT INTO [dbo].[IdentityUser]
            ([UserId]
            ,[Email]
            ,[EmailConfirmed]
            ,[PasswordHash]
            ,[SecurityStamp]
            ,[PhoneNumber]
            ,[PhoneNumberConfirmed]
            ,[TwoFactorEnabled]
            ,[LockoutEndDateUtc]
            ,[LockoutEnabled]
            ,[AccessFailedCount]
            ,[UserName]
            ,[CreateBy]
            ,[ModifyBy])
     VALUES
            (@USERID
            ,@EMAIL
            ,@EMAILCONFIRMED
            ,@PASSWORDHASH
            ,@SECURITYSTAMP
            ,@PHONENUMBER
            ,@PHONENUMBERCONFIRMED
            ,@TWOFACTORENABLED
            ,@LOCKOUTENDDATEUTC
            ,@LOCKOUTENABLED
            ,@ACCESSFAILEDCOUNT
            ,@USERNAME
            ,@CREATEBY
            ,@MODIFYBY)

-- profile
INSERT INTO [dbo].[IdentityProfile]
           ([UserId]
           ,[CreateBy]
           ,[ModifyBy])
     VALUES
           (@USERID
           ,@CREATEBY
           ,@MODIFYBY)

    COMMIT TRANSACTION

END TRY

BEGIN CATCH
    IF @@ERROR<>0 AND @@TRANCOUNT > 0
        ROLLBACK TRANSACTION

    DECLARE
        @ErrorMessage nvarchar(4000) = ERROR_MESSAGE(),
        @ErrorNumber int = ERROR_NUMBER(),
        @ErrorSeverity int = ERROR_SEVERITY(),
        @ErrorState int = ERROR_STATE(),
        @ErrorLine int = ERROR_LINE(),
        @ErrorProcedure nvarchar(200) = ISNULL(ERROR_PROCEDURE(), '-');
    SELECT @ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 'Message: ' + @ErrorMessage;
    RAISERROR (@ErrorMessage, @ErrorSeverity, 1, @ErrorNumber, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine)

    --THROW --if on SQL2012 or above
END CATCH";

            //ensure UserId
            if (user.UserId == default(Guid))
                user.UserId = Guid.NewGuid();

            //conver to sql min date
            var sqlMinDate = new DateTimeOffset(1753, 1, 1, 0, 0, 0, TimeSpan.FromHours(0));
            if (user.LockoutEndDateUtc < sqlMinDate)
                user.LockoutEndDateUtc = sqlMinDate;

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, new
                {
                    user.UserId,
                    user.Email,
                    user.EmailConfirmed,
                    user.PasswordHash,
                    user.SecurityStamp,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEndDateUtc,
                    user.LockoutEnabled,
                    user.AccessFailedCount,
                    user.UserName,
                    CreateBy = user.Audit.ModifyBy,
                    ModifyBy = user.Audit.ModifyBy
                }));
        }

        public Task DeleteAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var sql = @"
DELETE FROM IdentityUser WHERE UserId=@USERID";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, user.UserId));
        }

        public Task<TUser> FindByIdAsync(Guid userId)
        {
            var sql = @"
SELECT 
    IU.*, -- identity user
    IP.* -- identiy profile
FROM IdentityUser IU
    INNER JOIN IdentityProfile IP ON IU.UserId = IP.UserId 
WHERE IU.UserId=@USERID";

            using (var connection = new SqlConnection(_connection))
            {
                var result = connection.Query<TUser, IdentityProfile, TUser>(sql, (user, profile) =>
                {
                    user.Profile = profile;

                    return user;
                }, new { userId }, splitOn: "UserId").SingleOrDefault();

                return Task.FromResult(result);
            }
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            var sql = @"
SELECT 
    IU.*, -- identity user
    IP.* -- identiy profile
FROM IdentityUser IU
    INNER JOIN IdentityProfile IP ON IU.UserId = IP.UserId 
WHERE IU.UserName=@USERNAME";

            using (var connection = new SqlConnection(_connection))
            {
                var result = connection.Query<TUser, IdentityProfile, TUser>(sql, (user, profile) =>
                {
                    user.Profile = profile;

                    return user;
                }, new { userName }, splitOn: "UserId").SingleOrDefault();

                return Task.FromResult(result);
            }
        }

        public Task UpdateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var sql = @"
BEGIN TRY
    BEGIN TRANSACTION

    UPDATE [dbo].[IdentityUser]
       SET [Email] = @EMAIL
          ,[EmailConfirmed] = @EMAILCONFIRMED
          ,[PasswordHash] = @PASSWORDHASH
          ,[SecurityStamp] = @SECURITYSTAMP
          ,[PhoneNumber] = @PHONENUMBER
          ,[PhoneNumberConfirmed] = @PHONENUMBERCONFIRMED
          ,[TwoFactorEnabled] = @TWOFACTORENABLED
          ,[LockoutEndDateUtc] = @LOCKOUTENDDATEUTC
          ,[LockoutEnabled] = @LOCKOUTENABLED
          ,[AccessFailedCount] = @ACCESSFAILEDCOUNT
          ,[UserName] = @USERNAME
          ,[ModifyDate] = GETUTCDATE() 
    WHERE 
        UserId = @USERID


    UPDATE [dbo].[IdentityProfile]
       SET [FirstName] = @FIRSTNAME
          ,[MiddleName] = @MIDDLENAME
          ,[LastName] = @LASTNAME
          ,[ModifyDate] = GETUTCDATE()
    WHERE 
        UserId = @USERID

    IF (@MODIFYBY IS NOT NULL)
    BEGIN
        UPDATE [dbo].[IdentityUser] SET [ModifyBy] = @MODIFYBY WHERE UserId = @USERID
        UPDATE [dbo].[IdentityProfile] SET [ModifyBy] = @MODIFYBY WHERE UserId = @USERID
    END

   COMMIT TRANSACTION

END TRY

BEGIN CATCH
    IF @@ERROR<>0 AND @@TRANCOUNT > 0
        ROLLBACK TRANSACTION

    DECLARE
        @ErrorMessage nvarchar(4000) = ERROR_MESSAGE(),
        @ErrorNumber int = ERROR_NUMBER(),
        @ErrorSeverity int = ERROR_SEVERITY(),
        @ErrorState int = ERROR_STATE(),
        @ErrorLine int = ERROR_LINE(),
        @ErrorProcedure nvarchar(200) = ISNULL(ERROR_PROCEDURE(), '-');
    SELECT @ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 'Message: ' + @ErrorMessage;
    RAISERROR (@ErrorMessage, @ErrorSeverity, 1, @ErrorNumber, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine)

    --THROW --if on SQL2012 or above
END CATCH";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, new
                {
                    //user
                    user.UserId,
                    user.Email,
                    user.EmailConfirmed,
                    user.PasswordHash,
                    user.SecurityStamp,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEndDateUtc,
                    user.LockoutEnabled,
                    user.AccessFailedCount,
                    user.UserName,

                    //profile
                    user.Profile.FirstName,
                    user.Profile.MiddleName,
                    user.Profile.LastName,

                    //log
                    ModifyBy = (user.Audit == null) ? (Guid?)null : user.Audit.ModifyBy
                }));
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection = null;
            }
        }

        /* IUserPasswordStore
        ---------------------------*/

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.PasswordHash = passwordHash;

            return Task.FromResult(0);
        }

        /* IUserEmailStore
        ---------------------------*/

        public Task<TUser> FindByEmailAsync(string email)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            var sql = @"
SELECT 
    IU.*, -- identity user
    IP.* -- identiy profile
FROM IdentityUser IU
    INNER JOIN IdentityProfile IP ON IU.UserId = IP.UserId 
WHERE IU.Email=@EMAIL";

            using (var connection = new SqlConnection(_connection))
            {
                var result = connection.Query<TUser, IdentityProfile, TUser>(sql, (user, profile) =>
                {
                    user.Profile = profile;

                    return user;
                }, new { email }, splitOn: "UserId").SingleOrDefault();

                return Task.FromResult(result);
            }
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.Email = email;

            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.EmailConfirmed = confirmed;

            return Task.FromResult(0);
        }

        /* IUserSecurityStampStore
        ---------------------------*/

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;

            return Task.FromResult(0);
        }

        /* IUserRoleStore
        ---------------------------*/

        public Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var sql = @"
BEGIN TRY
    BEGIN TRANSACTION

    -- get role id
    DECLARE @ROLEID uniqueidentifier = (SELECT RoleId FROM IdentityRole WHERE Name=@ROLENAME)

    -- insert, if not exist
    INSERT INTO IdentityUserRole (UserId,RoleId)
         SELECT 
            IU.UserId,
            @ROLEID
        FROM
            IdentityUser IU
            LEFT JOIN IdentityUserRole IUR ON IU.UserId = IUR.UserId AND IUR.RoleId = @ROLEID
        WHERE
            IU.UserId=@USERID
            AND IUR.UserId IS NULL
    
    COMMIT TRANSACTION

END TRY

BEGIN CATCH
    IF @@ERROR<>0 AND @@TRANCOUNT > 0
        ROLLBACK TRANSACTION

    DECLARE
        @ErrorMessage nvarchar(4000) = ERROR_MESSAGE(),
        @ErrorNumber int = ERROR_NUMBER(),
        @ErrorSeverity int = ERROR_SEVERITY(),
        @ErrorState int = ERROR_STATE(),
        @ErrorLine int = ERROR_LINE(),
        @ErrorProcedure nvarchar(200) = ISNULL(ERROR_PROCEDURE(), '-');
    SELECT @ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 'Message: ' + @ErrorMessage;
    RAISERROR (@ErrorMessage, @ErrorSeverity, 1, @ErrorNumber, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine)

    --THROW --if on SQL2012 or above
END CATCH";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, new
                {
                    user.UserId,
                    roleName
                }));
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var sql = @"
SELECT
    IR.Name
FROM
    IdentityRole IR
    INNER JOIN IdentityUserRole IUR ON IR.RoleId = IUR.RoleId AND IUR.UserId=@USERID";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult<IList<string>>(connection.Query<string>(sql, new
                {
                    user.UserId
                }).ToList());
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (String.IsNullOrEmpty(roleName))
                throw new ArgumentNullException("roleName");

            var result = await GetRolesAsync(user);

            if (result == null || result.Count == 0)
                return false;

            return result.Contains<string>(roleName);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (String.IsNullOrEmpty(roleName))
                throw new ArgumentNullException("roleName");

            var sql = @"
BEGIN TRY
    BEGIN TRANSACTION

    -- get role id
    DECLARE @ROLEID uniqueidentifier = (SELECT RoleId FROM IdentityRole WHERE Name=@ROLENAME)

    -- delete
    DELETE FROM IdentityUserRole WHERE UserId=@USERID AND RoleId=@ROLEID
    
    COMMIT TRANSACTION

END TRY

BEGIN CATCH
    IF @@ERROR<>0 AND @@TRANCOUNT > 0
        ROLLBACK TRANSACTION

    DECLARE
        @ErrorMessage nvarchar(4000) = ERROR_MESSAGE(),
        @ErrorNumber int = ERROR_NUMBER(),
        @ErrorSeverity int = ERROR_SEVERITY(),
        @ErrorState int = ERROR_STATE(),
        @ErrorLine int = ERROR_LINE(),
        @ErrorProcedure nvarchar(200) = ISNULL(ERROR_PROCEDURE(), '-');
    SELECT @ErrorMessage = N'Error %d, Level %d, State %d, Procedure %s, Line %d, ' + 'Message: ' + @ErrorMessage;
    RAISERROR (@ErrorMessage, @ErrorSeverity, 1, @ErrorNumber, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine)

    --THROW --if on SQL2012 or above
END CATCH";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, new
                {
                    user.UserId,
                    roleName
                }));
        }

        /* IUserLockoutStore
        ---------------------------*/

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.LockoutEndDateUtc);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.AccessFailedCount++;

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.AccessFailedCount = 0;

            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.LockoutEnabled = enabled;

            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var sqlMinDate = new DateTimeOffset(1753, 1, 1, 0, 0, 0, TimeSpan.FromHours(0));

            if (lockoutEnd < sqlMinDate)
            {
                lockoutEnd = sqlMinDate;
            }

            user.LockoutEndDateUtc = lockoutEnd;

            return Task.FromResult(0);
        }

        /* IUserPhoneNumberStore
        ---------------------------*/

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.PhoneNumber = phoneNumber;

            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.PhoneNumberConfirmed = confirmed;

            return Task.FromResult(0);
        }

        /* IUserTwoFactorStore
        ---------------------------*/

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            user.TwoFactorEnabled = enabled;

            return Task.FromResult(0);
        }

        /* IUserLoginStore
        ---------------------------*/

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (login == null)
                throw new ArgumentNullException("login");

            var sql = @"
INSERT INTO [dbo].[IdentityLogin]
           ([LoginProvider]
           ,[ProviderKey]
           ,[UserId])
     VALUES
           (@LOGINPROVIDER
           ,@PROVIDERKEY
           ,@USERID)";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, new
                {
                    login.LoginProvider,
                    login.ProviderKey,
                    user.UserId
                }));
        }

        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            var sql = @"
SELECT
    UserId 
FROM 
    IdentityLogin
WHERE 
    LoginProvider=@LOGINPROVIDER
    AND ProviderKey=@PROVIDERKEY";

            var userId = default(Guid);

            using (var connection = new SqlConnection(_connection))
            {
                //get user id (could combine this into a single sql statement)
                userId = connection.Query<Guid>(sql, new
                {
                    login.LoginProvider,
                    login.ProviderKey,
                }).SingleOrDefault();
            }

            //return user
            if (userId != default(Guid))
                return FindByIdAsync(userId);

            //null user
            return Task.FromResult<TUser>(null);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var sql = @"
SELECT
    * 
FROM 
    IdentityLogin
WHERE 
    UserId=@USERID";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult<IList<UserLoginInfo>>(connection.Query<UserLoginInfo, dynamic, UserLoginInfo>(sql, (userLoginInfo, result) =>
                {
                    return new UserLoginInfo(result.LoginProvider, result.ProviderKey);
                }, new
                {
                    user.UserId
                }, splitOn: "UserId").ToList());
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            var sql = @"
DELETE FROM IdentityLogin
WHERE 
    UserId=@USERID
    AND LoginProvider=@LOGINPROVIDER
    AND ProviderKey=@PROVIDERKEY";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Query<UserLoginInfo>(sql, new
                {
                    user.UserId,
                    login.LoginProvider,
                    login.ProviderKey
                }).ToList() as IList<UserLoginInfo>);
        }

        /* IQueryableUserStore
        ---------------------------*/

        public IQueryable<TUser> Users
        {
            get
            {
                var sql = @"
SELECT 
    IU.*, -- identity user
    IP.* -- identiy profile
FROM IdentityUser IU
    INNER JOIN IdentityProfile IP ON IU.UserId = IP.UserId";

                using (var connection = new SqlConnection(_connection))
                {
                    var result = connection.Query<TUser, IdentityProfile, TUser>(sql, (user, profile) =>
                    {
                        user.Profile = profile;

                        return user;
                    }, splitOn: "UserId");

                    return result.AsQueryable();
                }
            }
        }
    }
}
