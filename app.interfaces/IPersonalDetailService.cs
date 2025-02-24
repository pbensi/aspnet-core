using app.shared.Dto;
using app.shared.Dto.PersonalDetail;

namespace app.interfaces
{
    public interface IPersonalDetailService
    {
        Task<PaginatedOffsetResultDto<ViewPersonalDetailDto>> GetViewPersonalDetailAsync(OffsetQueryDto offsetQuery);
    }
}
