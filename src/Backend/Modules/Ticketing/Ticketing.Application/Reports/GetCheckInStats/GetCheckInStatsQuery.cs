using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Reports.GetCheckInStats;

public sealed record GetCheckInStatsQuery(
    Guid EventId,
    Guid EventSessionId) : IQuery<CheckInStatsResponse>;

public sealed record CheckInStatsResponse(
    CheckInSummary Summary,
    IReadOnlyCollection<TicketTypeStat> TicketStats);

public sealed record CheckInSummary(
    int TotalTicketTypes,      
    int TotalTicketQuantity,     
    int TotalSold,         
    int TotalCheckedIn,     
    double CheckInRate);   

public sealed record TicketTypeStat(
    Guid TicketTypeId,         
    string TicketTypeName,    
    int Quantity,              
    int Sold,                
    int CheckedIn);          
