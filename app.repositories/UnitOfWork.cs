using app.entities;
using app.migrator;
using app.migrator.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace app.repositories
{
    public interface IUnitOfWork
    {
        Task<bool> SaveChangesAsync();
    }

    internal sealed class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _databaseContext;
        private readonly RequestContext _requestContext;

        public UnitOfWork(DatabaseContext databaseContext, RequestContext requestContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<bool> SaveChangesAsync()
        {
            IDbContextTransaction transaction = await _databaseContext.Database.BeginTransactionAsync();

            try
            {
                string userName = await GetNameAsync();

                await Task.WhenAll(
                    AccountSecurityLogAsync(userName),
                    AuditEntityAsync(userName)
                );

                int rowsAffected = await _databaseContext.SaveChangesAsync();

                if (rowsAffected > 0)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (DbUpdateException ex)
            {
                await RollbackAndRethrowAsync(transaction, ex, "An error occurred while saving changes to the database.");
                return false;
            }
            catch (Exception ex)
            {
                await RollbackAndRethrowAsync(transaction, ex, $"An unexpected error occurred: {ex.Message}");
                return false;
            }
        }

        private async Task RollbackAndRethrowAsync(IDbContextTransaction transaction, Exception ex, string message)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                Console.WriteLine($"Rollback failed: {rollbackEx.Message}");
                Console.WriteLine($"Rollback Stack Trace: {rollbackEx.StackTrace}");
            }

            throw new ApplicationException(message, ex);
        }

        private async Task<string> GetNameAsync()
        {
            Guid userGuid = _requestContext.UserGuid;

            if (userGuid == Guid.Empty) return "DefaultUser";

            var user = await _databaseContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserGuid == userGuid && p.IsActive);

            return user?.UserName ?? "DefaultUser";
        }

        private async Task AccountSecurityLogAsync(string userName)
        {
            Guid userGuid = _requestContext.UserGuid;
            if (userGuid == Guid.Empty) return;

            var userCredential = await _databaseContext.AccountSecurities
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserGuid == userGuid);

            if (userCredential == null) return;

            var mostRecentLog = await _databaseContext.AccountSecurityLogs
                .AsNoTracking()
                .Where(p => p.UserGuid == userGuid)
                .OrderByDescending(p => p.Id)
                .Select(p => new { p.OldPublicKey, p.OldPublicIV, p.OldPrivateKey, p.OldPrivateIV})
                .FirstOrDefaultAsync();

            if (mostRecentLog == null ||
                userCredential.PublicKey != mostRecentLog.OldPublicKey ||
                userCredential.PublicIV != mostRecentLog.OldPublicIV ||
                userCredential.PrivateKey != mostRecentLog.OldPrivateKey ||
                userCredential.PrivateIV != mostRecentLog.OldPrivateIV
                )
            {
                DateTime now = DateTime.UtcNow;

                var newLogEntry = new AccountSecurityLog
                {
                    UserGuid = userGuid,
                    OldPublicKey = userCredential.PublicKey,
                    OldPublicIV = userCredential.PublicIV,
                    OldPrivateKey = userCredential.PrivateKey,
                    OldPrivateIV = userCredential.PrivateIV,
                    DeviceName = NetworkProvider.DeviceName(),
                    Ipv4Address = NetworkProvider.Ipv4Address(),
                    Ipv6Address = NetworkProvider.Ipv6Address(),
                    OperatingSystem = NetworkProvider.OperatingSystem(),
                    CreatedAt = now,
                    CreatedBy = userName,
                    LastModifiedAt = now,
                    LastModifiedBy = userName
                };

                _databaseContext.AccountSecurityLogs.Add(newLogEntry);
            }
        }

        private async Task AuditEntityAsync(string user)
        {
            DateTime now = DateTime.UtcNow;

            foreach (var entry in _databaseContext.ChangeTracker.Entries<AuditEntity>())
            {
                if (entry.Entity is AuditEntity auditEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntity.CreatedAt = now;
                            auditEntity.CreatedBy = user;
                            auditEntity.LastModifiedAt = now;
                            auditEntity.LastModifiedBy = user;
                            break;

                        case EntityState.Modified:
                            auditEntity.CreatedAt = entry.Entity.CreatedAt;
                            auditEntity.CreatedBy = entry.Entity.CreatedBy;
                            auditEntity.LastModifiedAt = now;
                            auditEntity.LastModifiedBy = user;
                            break;
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
