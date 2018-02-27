using System;
using CodeBeingTested;
using CodeBeingTested.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace NSubstituteExamples
{
    [TestFixture]
    public class NSubstituteTests
    {
        [Test]
        public void Test_EmployeeInOfficeGetsNotified()
        {
            //
            // Create mocks:
            //
            var loggerMock = Substitute.For<ILogger>();

            var bobMock = Substitute.For<IEmployee>();
            bobMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(true);
            bobMock.GetNotificationPreference()
                .Returns(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = Substitute.For<IEmployeeService>();
            employeeServiceMock.GetEmployeesInNewYorkOffice()
                .Returns(new[] { bobMock });

            var notificationServiceMock = Substitute.For<INotificationService>();
            notificationServiceMock.SendEmail(bobMock, Arg.Any<string>());

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
            notificationServiceMock.Received().SendEmail(bobMock, Arg.Any<string>());
            notificationServiceMock.DidNotReceive().SendSlackMessage(bobMock, Arg.Any<string>());

        }


        [Test]
        public void Test_ExceptionDoesNotStopNotifications()
        {
            //
            // Create mocks:
            //
            var loggerMock = Substitute.For<ILogger>();
            loggerMock.Error(Arg.Any<Exception>());

            var bobMock = Substitute.For<IEmployee>();
            bobMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(true);
            bobMock.GetNotificationPreference()
                .Returns(LunchNotifier.NotificationType.Email);

            var marthaMock = Substitute.For<IEmployee>();
            marthaMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(true);
            marthaMock.GetNotificationPreference()
                .Returns(LunchNotifier.NotificationType.Email);

            DateTime? incomingDate;
            marthaMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(input =>
                {
                    incomingDate = input.ArgAt<DateTime>(0);
                    return incomingDate.Value.DayOfWeek != DayOfWeek.Sunday;
                });

            var employeeServiceMock = Substitute.For<IEmployeeService>();
            employeeServiceMock.GetEmployeesInNewYorkOffice()
                .Returns(new[] { bobMock, marthaMock });


            var notificationServiceMock = Substitute.For<INotificationService>();
            notificationServiceMock
                    .When(x => x.SendEmail(Arg.Any<IEmployee>(), Arg.Any<string>()))
                    .Do(input =>  throw new Exception());


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

            notificationServiceMock.Received(2).SendEmail(Arg.Any<IEmployee>(), Arg.Any<string>());

            loggerMock.Received(2).Error(Arg.Any<Exception>());
        }

        [Test]
        public void Test_ExceptionDoesNotStopNotifications_UsingWhenConstraint()
        {
            //
            // Create mocks:
            //
            var loggerMock = Substitute.For<ILogger>();
            loggerMock.Error(Arg.Any<Exception>());

            var bobMock = Substitute.For<IEmployee>();
            bobMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(true);
            bobMock.GetNotificationPreference()
                .Returns(LunchNotifier.NotificationType.Email);

            var marthaMock = Substitute.For<IEmployee>();
            marthaMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(true);
            marthaMock.GetNotificationPreference()
                .Returns(LunchNotifier.NotificationType.Email);

            DateTime? incomingDate;
            marthaMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(input =>
                {
                    incomingDate = input.Arg<DateTime>();
                    return incomingDate.Value.DayOfWeek != DayOfWeek.Sunday;
                });

            var employeeServiceMock = Substitute.For<IEmployeeService>();
            employeeServiceMock.GetEmployeesInNewYorkOffice()
                .Returns(new[] { bobMock, marthaMock });

            bool isFirstCall = true;

            var notificationServiceMock = Substitute.For<INotificationService>();
            notificationServiceMock
                .When(x => x.SendEmail(Arg.Any<IEmployee>(), Arg.Any<string>()))
                .Do((input) =>
                {
                    if (isFirstCall)
                    {
                        isFirstCall = false;
                        throw new Exception();
                    }

                });

         

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

            notificationServiceMock.Received(2).SendEmail(Arg.Any<IEmployee>(), Arg.Any<string>());

            loggerMock.Received(1).Error(Arg.Any<Exception>());
        }



        [Test]
        [TestCase("2017-01-01 13:00:00", LunchNotifier.LateLunchTemplate)]
        [TestCase("2017-01-01 12:59:59", LunchNotifier.RegularLunchTemplate)]
        public void Test_CorrectTemplateIsUsed_LateLunch(string currentTime, string expectedTemplate)
        {
            //
            // Create mocks:
            //
            var loggerMock = Substitute.For<ILogger>();

            var bobMock = Substitute.For<IEmployee>();
            bobMock.IsWorkingOnDate(Arg.Any<DateTime>())
                .Returns(true);

            var employeeServiceMock = Substitute.For<IEmployeeService>();
            employeeServiceMock.GetEmployeesInNewYorkOffice()
                .Returns(new[] { bobMock });

            var notificationServiceMock = Substitute.For<INotificationService>();


            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = Substitute.ForPartsOf<LunchNotifier>(notificationServiceMock, employeeServiceMock, loggerMock);
            classUnderTest.GetDateTime()
                          .Returns(DateTime.Parse(currentTime));

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications_DateTimeSeam();

            //
            // Check the results:
            //
            notificationServiceMock.Received().SendEmail(Arg.Any<IEmployee>(), expectedTemplate);
        }


    }
}
