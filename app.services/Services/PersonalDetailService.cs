using app.entities;
using app.interfaces;
using app.repositories;
using app.shared.Dto;
using app.shared.Dto.PersonalDetail;
using AutoMapper;
using app.repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace app.services.Services
{
    internal sealed class PersonalDetailService : IPersonalDetailService
    {
        private readonly IRepository<PersonalDetail> _personalDetail;
        private readonly IMapper _mapper;

        public PersonalDetailService(
            IRepositoryManager repositoryManager,
            IMapper mapper)
        {
            _personalDetail = repositoryManager.Entity<PersonalDetail>();
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
    }
}
