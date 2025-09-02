using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Infrastructure.EntityConfigurations
{
    internal  class OrganizationUnitEntityTypeConfiguration : IEntityTypeConfiguration<OrganizationUnit>
    {
        public void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            builder.ToTable("OrganizationUnits");

            builder.HasKey(o => o.Id);
            builder.Property(t => t.Id).UseSnowFlakeValueGenerator();

            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Description)
                .HasMaxLength(500);

            builder.Property(o => o.SortOrder)
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.IsActive)
                .IsRequired();

            builder.Property(o => o.IsDeleted)
                .IsRequired();

            builder.Property(o => o.DeletedAt);

            builder.Property(o => o.UpdateTime);

            // 索引
            builder.HasIndex(o => o.ParentId);
            builder.HasIndex(o => o.SortOrder);
            builder.HasIndex(o => o.IsActive);
            builder.HasIndex(o => o.IsDeleted);


            builder.HasQueryFilter(au => !au.IsDeleted);
        }
    }
}
