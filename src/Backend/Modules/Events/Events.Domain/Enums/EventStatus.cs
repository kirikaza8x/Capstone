namespace Events.Domain.Enums;

public enum EventStatus
{
    Draft = 0,
    PendingReview = 1,
    Published = 2,
    //Suspended = 3,
    Unpublished = 3,
    PendingCancellation = 4,
    Cancelled = 5,
    Completed = 6
}