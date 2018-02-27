using System;

namespace CodeBeingTested.Interfaces
{
    public interface IEmployee
    {
        bool IsWorkingOnDate(DateTime date);
        LunchNotifier.NotificationType GetNotificationPreference();
    }
}