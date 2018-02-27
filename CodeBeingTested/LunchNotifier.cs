using System;
using CodeBeingTested.Interfaces;

namespace CodeBeingTested
{
    public class LunchNotifier
    {
        public enum NotificationType
        {
            Email,
            Slack
        }

        private readonly INotificationService _notificationService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger _logger;

        public LunchNotifier(INotificationService notifySrv, IEmployeeService employeeSrv, ILogger logger)
        {
            _notificationService = notifySrv;
            _employeeService = employeeSrv;
            _logger = logger;
        }

        /// <summary>
        /// Determines which employees are currently working in the New York office and
        /// sends notifications to their preferred notification platform.  If lunch is
        /// late (after 1pm), a "Late Lunch" notification template is used instead of 
        /// the typical template.
        /// </summary>
        public void SendLunchtimeNotifications()
        {
            var now = DateTime.Now;
            var templateToUse = now.Hour > 12 ? LateLunchTemplate : RegularLunchTemplate;
            _logger.Write($"Using template: {templateToUse}");

            var nycEmployees = _employeeService.GetEmployeesInNewYorkOffice();

            foreach (var employee in nycEmployees)
            {
                if (!employee.IsWorkingOnDate(now.Date))
                {
                    // no need to notify employees that are out sick
                    // or on vacation today.
                    _logger.Debug("Skipping employe {employee}");
                    continue;
                }

                try
                {
                    var notificationType = employee.GetNotificationPreference();
                    switch (notificationType)
                    {
                        case NotificationType.Email:
                            _notificationService.SendEmail(employee, templateToUse);
                            break;
                        case NotificationType.Slack:
                            _notificationService.SendSlackMessage(employee, templateToUse);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }


        public virtual DateTime GetDateTime() => DateTime.Now;

        /// <summary>
        /// This method is identical to SendLunchtimeNotification, except that
        /// it extracts the use of System.DateTime.Now into a seperate, public and virtual
        /// method, so that constrained frameworks (Moq, RhinoMocks, NSubstitute) can mock
        /// the "current" time.
        /// </summary>
        public void SendLunchtimeNotifications_DateTimeSeam()
        {
            var now = GetDateTime();
            var templateToUse = now.Hour > 12 ? LateLunchTemplate : RegularLunchTemplate;
            _logger.Write($"Using template: {templateToUse}");

            var nycEmployees = _employeeService.GetEmployeesInNewYorkOffice();

            foreach (var employee in nycEmployees)
            {
                if (!employee.IsWorkingOnDate(now.Date))
                {
                    _logger.Debug("Skipping employe {employee}");
                    continue;
                }

                try
                {
                    var notificationType = employee.GetNotificationPreference();
                    switch (notificationType)
                    {
                        case NotificationType.Email:
                            _notificationService.SendEmail(employee, templateToUse);
                            break;
                        case NotificationType.Slack:
                            _notificationService.SendSlackMessage(employee, templateToUse);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }


        public const string RegularLunchTemplate = "It's Lunchtime, come eat!";
        public const string LateLunchTemplate = "It's Lunchtime -- Sorry it's late!";

    }
}