using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Users.Application.Features.Users.Queries
{
    public class GetDeviceInfoQueryHandler 
        : IQueryHandler<GetCurrentDeviceInfoQuery, DeviceInfo>
    {
        private readonly IDeviceDetectionService _deviceDetectionService;
        private readonly IMapper _mapper;

        public GetDeviceInfoQueryHandler(IDeviceDetectionService deviceDetectionService, IMapper mapper)
        {
            _deviceDetectionService = deviceDetectionService;
            _mapper = mapper;
        }

        public Task<Result<DeviceInfo>> Handle(GetCurrentDeviceInfoQuery request, CancellationToken cancellationToken)
        {
            var deviceInfo = _deviceDetectionService.GetDeviceInfo(
                request.UserAgent,
                request.IpAddress,
                request.DeviceId);

            if (deviceInfo is null)
            {
                return Task.FromResult(Result.Failure<DeviceInfo>(
                    Error.NotFound("Device.NotFound", "Device information not available.")
                ));
            }

            var dto = _mapper.Map<DeviceInfo>(deviceInfo);
            return Task.FromResult(Result.Success(dto));
        }
    }
}
