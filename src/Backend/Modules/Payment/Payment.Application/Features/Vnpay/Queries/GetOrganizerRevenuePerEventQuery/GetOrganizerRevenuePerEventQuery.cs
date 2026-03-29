using Payments.Application.Features.Vnpay.DTOs;
using Shared.Application.Abstractions.Messaging;

public record GetOrganizerRevenuePerEventQuery(Guid OrganizerId, bool ByNet = false)
    : IQuery<OrganizerRevenuePerEventDto>;
