using app.entities;
using app.migrator;
using app.migrator.Contexts;
using Microsoft.EntityFrameworkCore;

namespace app.repositories
{
    public interface IUnitOfWork
    {
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
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

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var transaction = await _databaseContext.Database.BeginTransactionAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                string userName = await GetNameAsync(cancellationToken);

                await Task.WhenAll(
                    AccountSecurityLogAsync(cancellationToken),
                    AuditEntityAsync(userName)
                );

                int rowsAffected = await _databaseContext.SaveChangesAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (rowsAffected > 0)
                {
                    await _databaseContext.Database.CommitTransactionAsync(cancellationToken);
                    return true;
                }

                return false;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _databaseContext.Database.RollbackTransactionAsync(cancellationToken);
                throw new ApplicationException("An error occurred while saving changes to the database.", ex);
            }
            catch (Exception ex)
            {
                await _databaseContext.Database.RollbackTransactionAsync(cancellationToken);
                throw new ApplicationException($"An unexpected error occurred: {ex.Message}", ex);
            }
        }

        private async Task<string> GetNameAsync(CancellationToken cancellationToken = default)
        {
            Guid userGuid = _requestContext.UserGuid;

            if (userGuid == Guid.Empty) return "DefaultUser";

            var user = await _databaseContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserGuid == userGuid && p.IsActive, cancellationToken);

            return user?.UserName ?? "DefaultUser";
        }

        private async Task AccountSecurityLogAsync(CancellationToken cancellationToken = default)
        {
            Guid userGuid = _requestContext.UserGuid;
            if (userGuid == Guid.Empty) return;

            var userCredential = await _databaseContext.AccountSecurities
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserGuid == userGuid, cancellationToken);

            if (userCredential == null) return;

            var mostRecentLog = await _databaseContext.AccountSecurityLogs
                .AsNoTracking()
                .Where(p => p.UserGuid == userGuid)
                .OrderByDescending(p => p.Id)
                .Select(p => new { p.OldEncryptedKey, p.OldEncryptedIV })
                .FirstOrDefaultAsync(cancellationToken);

            if (mostRecentLog == null ||
                userCredential.EncryptedKey != mostRecentLog.OldEncryptedKey ||
                userCredential.EncryptedIV != mostRecentLog.OldEncryptedIV)
            {
                var newLogEntry = new AccountSecurityLog
                {
                    UserGuid = userGuid,
                    OldEncryptedKey = userCredential.EncryptedKey,
                    OldEncryptedIV = userCredential.EncryptedIV,
                    Ipv4 = NetworkProvider.GetIpV4(),
                    Ipv6 = NetworkProvider.GetIpV6(),
                    OS = NetworkProvider.GetOperatingSystem()
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
                            auditEntity.LastModifiedBy = string.Empty;
                            break;

                        case EntityState.Modified:
                            entry.Entity.CreatedAt = entry.Entity.CreatedAt;
                            entry.Entity.CreatedBy = entry.Entity.CreatedBy;
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
