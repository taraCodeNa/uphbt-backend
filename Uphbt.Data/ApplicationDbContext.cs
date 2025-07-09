using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uphbt.Identity;

namespace Uphbt.Data;

public class ApplicationDbContext: IdentityDbContext<User, IdentityRole<long>, long>
{
        // ReSharper disable once ConvertToPrimaryConstructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
                base.OnModelCreating(builder);
                
                // Identity tables 
                builder.HasDefaultSchema("Identity"); // puts all identity tables in an "Identity" schema

                builder.Entity<User>(entity =>
                { 
                        entity.ToTable("Users", "Identity");
                });

                builder.Entity<IdentityRole<long>>(entity =>
                {
                        entity.ToTable("Roles", "Identity");
                });

                builder.Entity<IdentityUserClaim<long>>(entity =>
                {
                        entity.ToTable("UserClaims", "Identity");
                });

                builder.Entity<IdentityUserLogin<long>>(entity =>
                {
                        entity.ToTable("UserLogins", "Identity");
                });

                builder.Entity<IdentityUserRole<long>>(entity =>
                {
                        entity.ToTable("UserRoles", "Identity");
                });

                builder.Entity<IdentityRoleClaim<long>>(entity =>
                {
                        entity.ToTable("RoleClaims", "Identity");
                });

                builder.Entity<IdentityUserToken<long>>(entity =>
                {
                        entity.ToTable("UserTokens", "Identity");
                });
        }
}