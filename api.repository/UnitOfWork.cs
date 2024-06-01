using api.entities;
using api.migrator;
using api.repository.Utilities;
using Microsoft.EntityFrameworkCore;

namespace api.repository
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly APIDBContext _dbContext;

        public UnitOfWork(APIDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> SaveChangesWithUserRolesAsync(string pageName, OperationType operationType, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbContext.User
                    .Include(x => x.UserRoles)
                    .FirstOrDefaultAsync(x => x.Id == UserAudit.Id);

                if (user == null)
                {
                    return "User not found";
                }

                if (user.UserRoles == null)
                {
                    return $"The user {user?.Name} does not have any roles";
                }

                var roles = user.UserRoles.FirstOrDefault(x => x.Name == pageName);
                if (roles == null)
                {
                    return $"The user does not have permission to access this page '{pageName}' or it could not be found.";
                }

                switch (operationType)
                {
                    case OperationType.None:
                        break;
                    case OperationType.Add:
                        if (!roles.CanAdd)
                        {
                            return $"User '{user.Name}' does not have permission to add";
                        }
                        break;
                    case OperationType.Update:
                        if (!roles.CanUpdate)
                        {
                            return $"User '{user.Name}' does not have permission to Update";
                        }
                        break;
                    case OperationType.Remove:
                        if (!roles.CanRemove)
                        {
                            return $"User '{user.Name}' does not have permission to Remove";
                        }
                        break;
                }

                DateTime nowDate = DateTime.UtcNow;

                foreach (var entry in _dbContext.ChangeTracker.Entries<AuditEntity>())
                {
                    var auditEntity = entry.Entity;

                    if (entry.State == EntityState.Added)
                    {
                        auditEntity.CreatedAt = nowDate;
                        auditEntity.CreatedBy = user?.Name;

                        auditEntity.LastModifiedAt = nowDate;
                        auditEntity.LastModifiedBy = string.Empty;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditEntity.CreatedAt = user.CreatedAt;
                        auditEntity.CreatedBy = user.CreatedBy;

                        auditEntity.LastModifiedAt = nowDate;
                        auditEntity.LastModifiedBy = user?.Name;
                    }
                }

                int saveChanges = await _dbContext.SaveChangesAsync(cancellationToken);
                return saveChanges > 0 ? "Success" : "Failed";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbContext.User
                    .FindAsync(UserAudit.Id);

                DateTime nowDate = DateTime.UtcNow;

                foreach (var entry in _dbContext.ChangeTracker.Entries<AuditEntity>())
                {
                    var auditEntity = entry.Entity;

                    if (entry.State == EntityState.Added)
                    {
                        auditEntity.CreatedAt = nowDate;
                        auditEntity.CreatedBy = user?.Name;

                        auditEntity.LastModifiedAt = nowDate;
                        auditEntity.LastModifiedBy = string.Empty;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditEntity.CreatedAt = user.CreatedAt;
                        auditEntity.CreatedBy = user.CreatedBy;

                        auditEntity.LastModifiedAt = nowDate;
                        auditEntity.LastModifiedBy = user?.Name;
                    }
                }

                int saveChanges = await _dbContext.SaveChangesAsync(cancellationToken);
                return saveChanges > 0 ? "Success" : "Failed";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
