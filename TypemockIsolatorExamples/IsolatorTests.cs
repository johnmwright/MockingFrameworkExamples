using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBeingTested;
using CodeBeingTested.Interfaces;
using NUnit.Framework;
using TypeMock.ArrangeActAssert;

namespace TypemockIsolatorExamples
{
    [TestFixture]
    public class IsolatorTests
    {
        /********
         * NOTE: With Typemock Isolator, when changing a method behavior, the argument checking is off by default.
         * Regardless of the parameters at runtime, the behavior will change.
         * Use `.WasCalledWithExactArguments()` to validate specific args
         */

        [Test]
        [Isolated]
        public void Test_EmployeeInOfficeGetsNotified()
        {

            //
            // Create mocks:
            //

            var loggerMock = Isolate.Fake.Instance<ILogger>();

            var bobMock = Isolate.Fake.Instance<IEmployee>();
            Isolate.WhenCalled(() => bobMock.IsWorkingOnDate(DateTime.MinValue)).WillReturn(true);
            Isolate.WhenCalled(() => bobMock.GetNotificationPreference()).WillReturn(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = Isolate.Fake.Instance<IEmployeeService>();
            Isolate.WhenCalled(() => employeeServiceMock.GetEmployeesInNewYorkOffice()).WillReturn(new[] { bobMock });

            var notificationServiceMock = Isolate.Fake.Instance<INotificationService>();

            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock, employeeServiceMock, loggerMock);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //
            Isolate.Verify.WasCalledWithArguments(() => notificationServiceMock.SendEmail(null, null))
                          .Matching(args => ((IEmployee)args[0]).Equals(bobMock));
            Isolate.Verify.WasNotCalled(() => notificationServiceMock.SendSlackMessage(null, null));

        }


        [Test]
        [Isolated]
        public void Test_EmployeeInOfficeGetsNotified_UsingStrictMock()
        {
            //
            // Create mocks:
            //
            var loggerMock = Isolate.Fake.Instance<ILogger>();
            
            var bobMock = Isolate.Fake.Instance<IEmployee>();
            Isolate.WhenCalled(() => bobMock.IsWorkingOnDate(DateTime.MinValue)).WillReturn(true);
            Isolate.WhenCalled(() => bobMock.GetNotificationPreference()).WillReturn(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = Isolate.Fake.Instance<IEmployeeService>();
            Isolate.WhenCalled(() => employeeServiceMock.GetEmployeesInNewYorkOffice()).WillReturn(new[] { bobMock });

            var notificationServiceMock = Isolate.Fake.Instance<INotificationService>(Members.MustBeSpecified);
            Isolate.WhenCalled(() => notificationServiceMock.SendEmail(null, null)).IgnoreCall();

            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock, employeeServiceMock, loggerMock);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //

            Isolate.Verify.WasCalledWithArguments(() => notificationServiceMock.SendEmail(null, null))
                .Matching(args => ((IEmployee)args[0]).Equals(bobMock));

            // Since the notification mock calls are wrapped in a try/catch where the exeception is just
            // logged, we need to ensure the error logger was never called due to our strict mock failing.
            Isolate.Verify.WasNotCalled(() => loggerMock.Error(null));

        }


        [Test]
        [Isolated]
        public void Test_ExceptionDoesNotStopNotifications()
        {
            //
            // Create mocks:
            //
            var loggerMock = Isolate.Fake.Instance<ILogger>();

            var bobMock = Isolate.Fake.Instance<IEmployee>();
            Isolate.WhenCalled(() => bobMock.IsWorkingOnDate(DateTime.MinValue)).WillReturn(true);
            Isolate.WhenCalled(() => bobMock.GetNotificationPreference()).WillReturn(LunchNotifier.NotificationType.Email);

            var marthaMock = Isolate.Fake.Instance<IEmployee>();
            Isolate.WhenCalled(() => marthaMock.IsWorkingOnDate(DateTime.MinValue)).WillReturn(true);
            Isolate.WhenCalled(() => marthaMock.GetNotificationPreference()).WillReturn(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = Isolate.Fake.Instance<IEmployeeService>();
            Isolate.WhenCalled(() => employeeServiceMock.GetEmployeesInNewYorkOffice()).WillReturn(new[] { bobMock, marthaMock });

            var notificationServiceMock = Isolate.Fake.Instance<INotificationService>();
            Isolate.WhenCalled(() => notificationServiceMock.SendEmail(null, null)).WillThrow(new Exception());

            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock, employeeServiceMock, loggerMock);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //

            Isolate.Verify.WasCalledWithArguments(() => notificationServiceMock.SendEmail(null, null))
                .Matching(args => ((IEmployee)args[0]).Equals(bobMock));

            Isolate.Verify.WasCalledWithArguments(() => notificationServiceMock.SendEmail(null, null))
                .Matching(args => ((IEmployee)args[0]).Equals(marthaMock));

            Isolate.Verify.WasCalledWithAnyArguments(() => loggerMock.Error(null));
            int count = Isolate.Verify.GetTimesCalled(() => loggerMock.Error(null));
            Assert.That(count, Is.EqualTo(2));

        }



        [Test]
        [Isolated]
        [TestCase("2017-01-01 13:00:00", LunchNotifier.LateLunchTemplate)]
        [TestCase("2017-01-01 12:59:59", LunchNotifier.RegularLunchTemplate)]
        public void Test_CorrectTemplateIsUsed_LateLunch(string currentTime, string expectedTemplate)
        {
            //
            // Create mocks:
            //
            var loggerMock = Isolate.Fake.Instance<ILogger>();

            var bobMock = Isolate.Fake.Instance<IEmployee>();
            Isolate.WhenCalled(() => bobMock.IsWorkingOnDate(DateTime.MinValue)).WillReturn(true);
            Isolate.WhenCalled(() => bobMock.GetNotificationPreference()).WillReturn(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = Isolate.Fake.Instance<IEmployeeService>();
            Isolate.WhenCalled(() => employeeServiceMock.GetEmployeesInNewYorkOffice()).WillReturn(new[] { bobMock });

            var notificationServiceMock = Isolate.Fake.Instance<INotificationService>();

            Isolate.WhenCalled(() => System.DateTime.Now).WillReturn(DateTime.Parse(currentTime));

            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock, employeeServiceMock, loggerMock);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //            
            Isolate.Verify.WasCalledWithExactArguments(() => notificationServiceMock.SendEmail(bobMock, expectedTemplate));

        }


    }
}
