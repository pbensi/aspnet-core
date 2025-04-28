using app.entities;
using app.interfaces;
using app.migrator;
using app.repositories;
using app.repositories.Extensions;
using app.services.Authorizations;
using app.shared.Dto;
using app.shared.Dto.Account;
using app.shared.Dto.PersonalDetail;
using app.shared.Securities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static app.shared.EnumGroup;

namespace app.services.Services
{
    internal sealed class PersonalDetailService : IPersonalDetailService
    {
        private readonly IRepository<PersonalDetail> _personalDetail;
        private readonly IRepository<AccountSecurityLog> _accountSecurityLog;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PersonalDetailService(
            IRepositoryManager repositoryManager,
            IMapper mapper)
        {
            _personalDetail = repositoryManager.Entity<PersonalDetail>();
            _accountSecurityLog = repositoryManager.Entity<AccountSecurityLog>();
            _unitOfWork = repositoryManager.UnitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedOffsetResultDto<ViewPersonalDetailDto>> GetViewPersonalDetailAsync(OffsetQueryDto offsetQuery)
        {
            try
            {
                var query = _personalDetail.AsQueryable();

                query = query.Where(p => p.IsDelete == false);

                if (!string.IsNullOrEmpty(offsetQuery.Search))
                {
                    query = query.Where(p => p.FirstName.Contains(offsetQuery.Search));
                }

                if (string.IsNullOrEmpty(offsetQuery.SortColumn) || string.IsNullOrEmpty(offsetQuery.SortDirection))
                {
                    offsetQuery.SortColumn = "FirstName";
                    offsetQuery.SortDirection = "asc";
                }

                int skip = (offsetQuery.PageNumber - 1) * offsetQuery.PageSize;
                var pagedItems = await query
                    .Sorting(offsetQuery.SortColumn, offsetQuery.SortDirection)
                    .ThenBySorting("Id", offsetQuery.SortDirection)
                    .Skip(skip)
                    .Take(offsetQuery.PageSize)
                    .ToListAsync();

                int totalCount = await query.CountAsync();

                var mappedItems = _mapper.Map<List<ViewPersonalDetailDto>>(pagedItems);

                return new PaginatedOffsetResultDto<ViewPersonalDetailDto>
                {
                    Data = mappedItems,
                    TotalCount = totalCount,
                    SortColumn = offsetQuery.SortColumn,
                    SortDirection = offsetQuery.SortDirection,
                    Search = offsetQuery.Search
                };
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(e.Message);
            }
        }

        public async Task<ResultDto> CreatePersonalDetailForAccountAsync(CreatePersonalDetailDto createPersonal)
        {
            try
            {
                var existingUser = await _personalDetail
                    .FirstOrDefaultAsync(p => p.Account.UserName == createPersonal.UserName);

                if (existingUser != null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "UserName Already Exists",
                        Type = MessageType.Warning
                    };
                }

                List<AccountRoleDto> accountRole =  await AccountRolePermission.RolesAsync(new List<AccountRoleDto>(), _mapper);

                var accountRoles = _mapper.Map<List<AccountRole>>(accountRole);
                var personal = _mapper.Map<PersonalDetail>(createPersonal);

                Guid userGuid = Guid.NewGuid();

                (string publicKey, string publicIV) = Symmetric.GenerateRandomKeyAndIV();
                (string privateKey, string privateIV) = Symmetric.GenerateRandomKeyAndIV();

                personal.UserGuid = userGuid;
                personal.Account.IsAdmin = false;
                personal.Account.IsActive = true;
                personal.Account.Password = Hashing.Hashed(createPersonal.Password);
                personal.Account.AccountRole = accountRoles;
                personal.Account.AccountSecurity = new AccountSecurity
                {
                    PublicIV = publicKey,
                    PublicKey = publicIV,
                    PrivateKey = privateKey,
                    PrivateIV = privateIV,
                    DeviceName = NetworkProvider.DeviceName(),
                    Ipv4Address = NetworkProvider.Ipv4Address(),
                    Ipv6Address = NetworkProvider.Ipv6Address(),
                    OperatingSystem = NetworkProvider.OperatingSystem()
                };

                var newLogEntry = new AccountSecurityLog
                {
                    UserGuid = userGuid,
                    OldPublicIV = publicKey,
                    OldPublicKey = publicIV,
                    OldPrivateKey = privateKey,
                    OldPrivateIV = privateIV,
                    DeviceName = NetworkProvider.DeviceName(),
                    Ipv4Address = NetworkProvider.Ipv4Address(),
                    Ipv6Address = NetworkProvider.Ipv6Address(),
                    OperatingSystem = NetworkProvider.OperatingSystem()
                };

                _personalDetail.Add(personal);
                _accountSecurityLog.Add(newLogEntry);

                bool isSavedSuccessfully = await _unitOfWork.SaveChangesAsync();

                return new ResultDto
                {
                    IsSuccess = isSavedSuccessfully,
                    Message = isSavedSuccessfully ? "Created successfully." : "Failed to create user.",
                    Type = isSavedSuccessfully ? MessageType.Success : MessageType.Error
                };
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while creating the user.", e);
            }
        }
    }
}
