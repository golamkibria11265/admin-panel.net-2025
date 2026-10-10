using System;
using System.Collections.Generic;
using Dll.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dll.Data;

public partial class DotNetContext : DbContext
{
    public DotNetContext()
    {
    }

    public DotNetContext(DbContextOptions<DotNetContext> options)
        : base(options)
    {
    }

    #region Ap Entities
    public virtual DbSet<ApMenu> ApMenus { get; set; }

    public virtual DbSet<ApMenuAction> ApMenuActions { get; set; }

    public virtual DbSet<ApOtpHistory> ApOtpHistories { get; set; }

    public virtual DbSet<ApPermission> ApPermissions { get; set; }

    public virtual DbSet<ApRole> ApRoles { get; set; }

    public virtual DbSet<ApRolePermission> ApRolePermissions { get; set; }

    public virtual DbSet<ApUser> ApUsers { get; set; }

    public virtual DbSet<ApUserRole> ApUserRoles { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApMenu>(entity =>
        {
            entity.ToTable("ApMenu");
        });

        modelBuilder.Entity<ApMenuAction>(entity =>
        {
            entity.ToTable("ApMenuAction");
        });

        modelBuilder.Entity<ApOtpHistory>(entity =>
        {
            entity.ToTable("ApOtpHistory");

            entity.Property(e => e.Ikey).HasColumnName("IKey");
            entity.Property(e => e.Ivalue).HasColumnName("IValue");
        });

        modelBuilder.Entity<ApPermission>(entity =>
        {
            entity.ToTable("ApPermission");
        });

        modelBuilder.Entity<ApRole>(entity =>
        {
            entity.ToTable("ApRole");
        });

        modelBuilder.Entity<ApRolePermission>(entity =>
        {
            entity.ToTable("ApRolePermission");
        });

        modelBuilder.Entity<ApUser>(entity =>
        {
            entity.ToTable("ApUser");
            // User names are unique (migration 20261011000000_UniqueUserName).
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.HasIndex(e => e.UserName).IsUnique().HasFilter("[UserName] IS NOT NULL");
        });

        modelBuilder.Entity<ApUserRole>(entity =>
        {
            entity.ToTable("ApUserRole");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
