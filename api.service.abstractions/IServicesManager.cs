using api.service.abstractions.IServices;

namespace api.service.abstractions
{
    public interface IServicesManager
    {
        IUserService UserService { get; }
    }
}
