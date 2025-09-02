using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Infrastructure.EntityConfigurations
{
    internal class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseGuidVersion7ValueGenerator();

            builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
            builder.Property(b => b.Description).HasMaxLength(200);
            builder.Property(b => b.CreatedAt);
            builder.Property(b => b.IsActive);
            builder.Property(b => b.IsDeleted);

            // 唯一索引
            builder.HasIndex(b => b.Name).IsUnique();

            builder.HasMany(r => r.Permissions).WithOne().HasForeignKey(rp => rp.RoleId);
            builder.Navigation(e => e.Permissions).AutoInclude();

            // 软删除过滤器
            builder.HasQueryFilter(b => !b.IsDeleted);
        }
    }

    internal class RolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermission");

            builder.HasKey(t => new { t.RoleId, t.PermissionCode });

            builder.Property(b => b.RoleId);
            builder.Property(b => b.PermissionCode).HasMaxLength(100).IsRequired();
            builder.Property(b => b.PermissionName).HasMaxLength(100);
            builder.Property(b => b.PermissionDescription).HasMaxLength(200);

            // 外键关系
            builder.HasOne<Role>()
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
