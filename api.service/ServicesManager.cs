using api.repository;
using api.repository.Authorization;
using api.service.abstractions;
using api.service.abstractions.IServices;
using api.service.Services;
using AutoMapper;

namespace api.service
{
    public sealed class ServiceManager : IServicesManager
    {
        private readonly Lazy<IUserService> _lazyUserService;

        public ServiceManager(IRepositoryManager repositoryManager, IMapper mapper, IPermissionProvider permissionProvider)
        {
            _lazyUserService = new Lazy<IUserService>(() => new UserService(repositoryManager, mapper, permissionProvider));
        }

        public IUserService UserService => _lazyUserService.Value;
    }
}
