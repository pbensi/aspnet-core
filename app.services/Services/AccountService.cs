using app.entities;
using app.interfaces;
using app.migrator;
using app.migrator.Contexts;
using app.repositories;
using app.services.Authorizations;
using app.shared.Dto.Account;
using app.shared.Securities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static app.shared.EnumGroup;

namespace app.services.Services
{
    internal sealed class AccountService : IAccountService
    {
        private readonly IRepository<Account> _account;
        private readonly IRepository<AccountRole> _accountRole;
        private readonly IRepository<OpenResourcePath> _openResourcePath;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RequestContext _requestContext;
        private readonly IMapper _mapper;

        public AccountService(IRepositoryManager repositoryManager,
            RequestContext requestContext,
            IMapper mapper)
        {
            _account = repositoryManager.Entity<Account>();
            _accountRole = repositoryManager.Entity<AccountRole>();
            _openResourcePath = repositoryManager.Entity<OpenResourcePath>();
            _unitOfWork = repositoryManager.UnitOfWork;
            _requestContext = requestContext;
            _mapper = mapper;
        }

        public async Task<(AccountDto? account, string message)> SignInAccountAsync(SignInDto signIn)
        {
            var account = await _account
                .Include(
                p => p.AccountSecurity,
                p => p.PersonalDetail)
                .FirstOrDefaultAsync(p => p.UserName == signIn.UserName);

            if (account == null)
            {
                return (null, "Invalid user name or password. Please try again.");
            }

            bool isPasswordValid = Hashing.Verify(account.Password, signIn.Password);
            if (!isPasswordValid)
            {
                return (null, "Invalid user name or password. Please try again.");
            }

            if (account.PersonalDetail != null && account.PersonalDetail.IsDelete)
            {
                return (null, $"Account has been removed on {account.PersonalDetail.LastModifiedAt}. Please contact your administrator for assistance.");
            }

            if (!account.IsActive)
            {
                return (null, $"The user has been inactive since {account.LastModifiedAt}. Please contact your administrator for assistance.");
            }

            account.AccountSecurity.DeviceName = NetworkProvider.DeviceName();
            account.AccountSecurity.Ipv4Address = NetworkProvider.Ipv4Address();
            account.AccountSecurity.Ipv6Address = NetworkProvider.Ipv6Address();
            account.AccountSecurity.OperatingSystem = NetworkProvider.OperatingSystem();

            _requestContext.UserGuid = account.UserGuid;

            _account.Update(account);

            var resultDto = _mapper.Map<AccountDto>(account);

            bool isSaveChanges = await _unitOfWork.SaveChangesAsync();

            return (resultDto, isSaveChanges.ToString());
        }

        public async Task<AccountDto> GetAccountAsync(Guid userGuid)
        {
            var account = await _account.FirstOrDefaultAsync(p => p.UserGuid == userGuid);

            return _mapper.Map<AccountDto>(account);
        }

        public async Task<PermissionCheckDto> CheckAccountPermissionAsync(string pageName, string requestMethod, string requestPath, string allowedRole)
        {
            bool hasPermission = false;
            Guid userGuid = _requestContext.UserGuid;

            if (userGuid != Guid.Empty)
            {
                var account = await _account
                .Include(
                    p => p.PersonalDetail,
                    p => p.AccountRole)
                .FirstOrDefaultAsync(p => p.UserGuid == userGuid);

                if (account == null)
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = "Account not found. Please reach out to your administrator for assistance."
                    };
                }

                if (account.PersonalDetail == null)
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = $"Account has been removed on {account.LastModifiedAt}. Please reach out to your administrator for assistance."
                    };
                }

                if (!account.IsActive)
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = $"The user has been inactive since {account.LastModifiedAt}. Please reach out to your administrator for assistance."
                    };
                }

                if (!account.IsAdmin && pageName == PageName.Pages_Swagger)
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = "Access Denied: You do not have administrator privileges. Please reach out to your administrator for assistance."
                    };
                }

                if (!Enum.TryParse<RequestMethod>(requestMethod, true, out var method))
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = "Invalid request method."
                    };
                }

                hasPermission = method switch
                {
                    RequestMethod.Post => account.AccountRole.Any(p => p.PageName == pageName && p.IsPost),
                    RequestMethod.Put => account.AccountRole.Any(p => p.PageName == pageName && p.IsPut),
                    RequestMethod.Delete => account.AccountRole.Any(p => p.PageName == pageName && p.IsDelete),
                    RequestMethod.Options => account.AccountRole.Any(p => p.PageName == pageName && p.IsOptions),
                    RequestMethod.Get => account.AccountRole.Any(p => p.PageName == pageName && p.IsGet),
                    _ => false,
                };
            }

            if (!hasPermission)
            {
                var resourcePath = await _openResourcePath.FirstOrDefaultAsync(p => p.RequestPath == requestPath && p.AllowedRole == allowedRole);
                if (resourcePath == null)
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = $"Request path or allowed {allowedRole} not found {requestPath}."
                    };
                }

                var resourceMethod = await _openResourcePath.FirstOrDefaultAsync(p => p.RequestPath == requestPath && p.AllowedRole == allowedRole && p.RequestMethod == requestMethod);
                if (resourceMethod == null)
                {
                    return new PermissionCheckDto
                    {
                        HasPermission = hasPermission,
                        Message = $"User does not have permission to {requestMethod} on {requestPath}."
                    };
                }

                hasPermission = !hasPermission;
            }

            return new PermissionCheckDto
            {
                HasPermission = hasPermission,
                Message = "Permission granted"
            };
        }

        public async Task<List<AccountRoleDto>> CheckPermissionPageNamesAsync(List<string> pageName)
        {
            try
            {
                if (pageName == null || !pageName.Any())
                {
                    return new List<AccountRoleDto>();
                }

                var user = await _accountRole
                    .Where(p => p.UserGuid == _requestContext.UserGuid && pageName.Any(name => p.PageName == name) && p.IsGet)
                    .ToListAsync();

                return _mapper.Map<List<AccountRoleDto>>(user);
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while getting permission page name.", e);
            }
        }
    }
}
