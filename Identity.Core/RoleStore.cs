using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Identity.Core
{
    public class RoleStore<TRole> : IRoleStore<TRole, Guid>, IQueryableRoleStore<TRole, Guid>
        where TRole : IdentityRole
    {
        private string _connection;

        public RoleStore(string connection)
        {
            _connection = connection;
        }

        public Task CreateAsync(TRole role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            string sql = @"
INSERT INTO [dbo].[IdentityRole]
           ([RoleId]
           ,[Name])
     VALUES
           (@ROLEID
           ,@NAME)";

            if (role.RoleId == default(Guid))
                role.RoleId = Guid.NewGuid();

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, role));
        }

        public Task DeleteAsync(TRole role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            string sql = @"
DELETE FROM IdentityRole 
WHERE RoleId=@ROLEID";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, role));
        }

        public Task<TRole> FindByIdAsync(Guid roleId)
        {
            var sql = @"
SELECT 
    *
FROM 
    IdentityRole   
WHERE 
    RoleId=@ROLEID";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult<TRole>(connection.Query<TRole>(sql, roleId).SingleOrDefault());
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            if (String.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("roleName");

            var sql = @"
SELECT 
    *
FROM 
    IdentityRole   
WHERE 
    Name=@NAME";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult<TRole>(connection.Query<TRole>(sql, new { Name = roleName }).SingleOrDefault());
        }

        public Task UpdateAsync(TRole role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            string sql = @"
UPDATE IdentityRole SET 
	[Name] = @NAME
WHERE 
    RoleId = @ROLEID";

            using (var connection = new SqlConnection(_connection))
                return Task.FromResult(connection.Execute(sql, role));
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection = null;
            }
        }

        /* IQueryableRoleStore
        ---------------------------*/

        public IQueryable<TRole> Roles
        {
            get
            {
                var sql = @"
SELECT 
    * 
FROM IdentityRole";

                using (var connection = new SqlConnection(_connection))
                {
                    var result = connection.Query<TRole>(sql);

                    return result.AsQueryable();
                }
            }
        }
    }
}
