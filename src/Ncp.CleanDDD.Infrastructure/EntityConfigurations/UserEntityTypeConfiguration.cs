using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Infrastructure.EntityConfigurations
{
    internal class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseSnowFlakeValueGenerator();

            builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
            builder.Property(b => b.Email).HasMaxLength(100).IsRequired();
            builder.Property(b => b.PasswordHash).HasMaxLength(255).IsRequired();
            builder.Property(b => b.Phone).HasMaxLength(20);
            builder.Property(b => b.RealName).HasMaxLength(50);
            builder.Property(b => b.Gender).HasMaxLength(10);
            builder.Property(b => b.Age);
            builder.Property(b => b.BirthDate);
            builder.Property(b => b.IsActive);
            builder.Property(b => b.CreatedAt);
            builder.Property(b => b.LastLoginTime);
            builder.Property(b => b.UpdateTime);

            //加上IsUnique就是唯一索引
            builder.HasIndex(b => b.Name);//.IsUnique();
            builder.HasIndex(b => b.Email);//.IsUnique();

            builder.HasMany(au => au.Roles)
            .WithOne()
            .HasForeignKey(aur => aur.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(au => au.Roles).AutoInclude();


            // 配置 User 与 UserOrganizationUnit 的一对一关系
            builder.HasOne(au => au.OrganizationUnit)
                .WithOne()
                .HasForeignKey<UserOrganizationUnit>(uou => uou.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(au => au.OrganizationUnit).AutoInclude();



        }
    }

    internal class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRole");

            builder.HasKey(t => new { t.UserId, t.RoleId });

            builder.Property(b => b.UserId);
            builder.Property(b => b.RoleId);
            builder.Property(b => b.RoleName).HasMaxLength(50).IsRequired();

            // 外键关系
            builder.HasOne<User>()
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    internal class UserOrganizationUnitEntityTypeConfiguration : IEntityTypeConfiguration<UserOrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<UserOrganizationUnit> builder)
        {
            builder.ToTable("UserOrganizationUnits");

            builder.HasKey(uo => uo.UserId);

            builder.Property(uo => uo.UserId);

            builder.Property(uo => uo.OrganizationUnitId);



            builder.Property(uo => uo.AssignedAt)
                .IsRequired();

            // 索引
            builder.HasIndex(uo => uo.UserId);
            builder.HasIndex(uo => uo.OrganizationUnitId);


        }
    }
}
