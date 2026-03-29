using Payments.Application.Features.Vnpay.DTOs;
using Shared.Application.Abstractions.Messaging;

public record GetOrganizerRevenueSummaryQuery(Guid OrganizerId)
    : IQuery<OrganizerRevenueSummaryDto>;