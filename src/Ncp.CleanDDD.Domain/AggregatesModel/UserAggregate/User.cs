using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate
{
    public partial record UserId : IInt64StronglyTypedId;

    public class User : Entity<UserId>, IAggregateRoot
    {
        protected User()
        {
        }

        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;

        public string RealName { get; private set; } = string.Empty;

        public int Status { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; init; }
        public DateTime? LastLoginTime { get; private set; }
        public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTime.UtcNow);

        /// <summary>
        /// 删除标记
        /// </summary>
        public Deleted IsDeleted { get; private set; } = new Deleted(false);

        /// <summary>
        /// 删除时间
        /// </summary>
        public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTime.UtcNow);

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; private set; } = string.Empty;

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; private set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime BirthDate { get; private set; } = default!;


        // 用户角色关系
        public virtual ICollection<UserRole> Roles { get; } = [];


        // 用户组织架构关系 - 1对1关系
        public virtual UserOrganizationUnit OrganizationUnit { get; private set; } = default!;

        public ICollection<UserRefreshToken> RefreshTokens { get; } = [];

     

        public User(string name, string phone, string password, IEnumerable<UserRole> roles, string realName, int status, string email, string gender, DateTime birthDate)
        {
            CreatedAt = DateTime.Now;
            Name = name;
            Phone = phone;
            PasswordHash = password;
            RealName = realName;
            Status = status;
            Email = email;
            Gender = gender;
            Age = CalculateAge(birthDate);
            BirthDate = birthDate;
            foreach (var userRole in roles)
            {
                Roles.Add(userRole);
            }

        }



        public void SoftDelete()
        {
            IsDeleted = true;
        }

        public void PasswordReset(string password)
        {
            PasswordHash = password;
        }

        public void SetUserRefreshToken(string refreshToken)
        {
            var refreshTokenInfo = new UserRefreshToken(refreshToken);
            RefreshTokens.Add(refreshTokenInfo);
        }

        public void UpdateLastLoginTime(DateTime loginTime)
        {
            LastLoginTime = loginTime;
            UpdateTime = new UpdateTime(DateTime.UtcNow);
        }


        // 组织架构管理方法 - 1对1关系
        public void AssignOrganizationUnit(UserOrganizationUnit organizationUnit)
        {
            if (organizationUnit == null)
            {
                throw new KnownException("组织架构不能为空");
            }

            OrganizationUnit = organizationUnit;
        }



        public static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // 如果生日还没过，则减一岁
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public void UpdateUserInfo(string name, string phone, string realName, int status, string email, string gender, DateTime birthDate)
        {
            Name = name;
            Phone = phone;
            RealName = realName;
            Status = status;
            Email = email;
            Gender = gender;
            Age = CalculateAge(birthDate);
            BirthDate = birthDate;
        }

        public void UpdateRoleInfo(RoleId roleId, string roleName)
        {
            var savedRole = Roles.FirstOrDefault(r => r.RoleId == roleId);
            savedRole?.UpdateRoleInfo(roleName);
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (!string.IsNullOrEmpty(newPasswordHash))
            {
                PasswordHash = newPasswordHash;
                UpdateTime = new UpdateTime(DateTime.UtcNow);
            }


        }


        // 角色管理方法
        public void UpdateRoles(IEnumerable<UserRole> rolesToBeAssigned)
        {
            var currentRoleMap = Roles.ToDictionary(r => r.RoleId);
            var targetRoleMap = rolesToBeAssigned.ToDictionary(r => r.RoleId);

            var roleIdsToRemove = currentRoleMap.Keys.Except(targetRoleMap.Keys);
            foreach (var roleId in roleIdsToRemove)
            {
                Roles.Remove(currentRoleMap[roleId]);
            }

            var roleIdsToAdd = targetRoleMap.Keys.Except(currentRoleMap.Keys);
            foreach (var roleId in roleIdsToAdd)
            {
                var targetRole = targetRoleMap[roleId];
                Roles.Add(targetRole);
            }
        }


       

    }
}
