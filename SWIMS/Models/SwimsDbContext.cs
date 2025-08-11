// -------------------------------------------------------------------
// File:    SwimsDbContext.cs
// Author:  N/A
// Created: N/A
// Purpose: EF Core DbContext for SWIMS, managing application entities and Identity tables.
// Dependencies:
//   - Microsoft.EntityFrameworkCore.DbContext
//   - Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<SwUser, SwRole, int>
//   - SWIMS.Models.SwUser, SwRole
// -------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace SWIMS.Models;

/// <summary>
/// Database context for the SWIMS application, inherits from
/// <c>IdentityDbContext&lt;SwUser, SwRole, int&gt;</c> to include the Identity schema.
/// </summary>
public partial class SwimsDbContext : IdentityDbContext<SwUser, SwRole, int>

{
    /// <summary>
    /// Initializes a new instance of <see cref="SwimsDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">
    /// The <see cref="DbContextOptions{SwimsDbContext}"/> used to configure the context.
    /// </param>
    public SwimsDbContext(DbContextOptions<SwimsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for <see cref="SwRole"/> entities.
    /// </summary>
    public virtual DbSet<SwRole> SwRoles { get; set; }

    /// <summary>
    /// DbSet for <see cref="SwUser"/> entities.
    /// </summary>
    public virtual DbSet<SwUser> SwUsers { get; set; }

    /// <summary>
    /// Configures the EF Core model.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> for constructing entity mappings.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<SwRole>(entity =>
        {
            entity.ToTable("SW_roles");

            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<SwUser>(entity =>
        {
            entity.ToTable("SW_users");

            
        });

        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("SW_user_roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("SW_user_claims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("SW_user_logins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("SW_role_claims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("SW_user_tokens");


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
