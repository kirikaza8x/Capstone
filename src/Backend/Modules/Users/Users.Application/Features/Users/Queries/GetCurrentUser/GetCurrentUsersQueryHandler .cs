using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;
using Shared.Domain.Abstractions;

namespace Users.Application.Features.Users.Queries
{
    public class GetCurrentUserQueryHandler
        : IQueryHandler<GetCurrentUserQuery, CurrentUserDto>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDeviceDetectionService _deviceDetectionService;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(
            ICurrentUserService currentUserService,
            IDeviceDetectionService deviceDetectionService,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _deviceDetectionService = deviceDetectionService;
            _mapper = mapper;
        }

        public Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var currentUser = _currentUserService.GetCurrentUser();

            if (currentUser is null || currentUser.UserId == Guid.Empty)
            {
                return Task.FromResult(Result.Failure<CurrentUserDto>(
                    Error.NotFound("User.NotFound", "Current user not found.")
                ));
            }

            // enrich with device info
            var deviceInfo = _deviceDetectionService.GetDeviceInfo(
                currentUser.UserAgent,
                currentUser.IpAddress,
                currentUser.DeviceId);

            var dto = _mapper.Map<CurrentUserDto>(currentUser);

            dto.DeviceId = deviceInfo.DeviceId;
            dto.DeviceName = deviceInfo.DeviceName;
            dto.Browser = deviceInfo.Browser;
            dto.OperatingSystem = deviceInfo.OperatingSystem;
            dto.DeviceType = deviceInfo.DeviceType;
            dto.BrowserVersion = deviceInfo.BrowserVersion;
            dto.OSVersion = deviceInfo.OSVersion;
            dto.UserAgent = deviceInfo.UserAgent;
            dto.IpAddress = deviceInfo.IpAddress;

            return Task.FromResult(Result.Success(dto));
        }
    }
}
