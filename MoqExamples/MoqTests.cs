using System;
using CodeBeingTested;
using CodeBeingTested.Interfaces;
using Moq;
using NUnit.Framework;

namespace MoqExamples
{

    [TestFixture]
    class MoqTests
    {

        [Test]
        public void Test_EmployeeInOfficeGetsNotified()
        {
            //
            // Create mocks:
            //
            var loggerMock = new Moq.Mock<ILogger>();

            var bobMock = new Moq.Mock<IEmployee>();
            bobMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);
            bobMock.Setup(x => x.GetNotificationPreference())
                .Returns(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = new Moq.Mock<IEmployeeService>();
            employeeServiceMock.Setup(x => x.GetEmployeesInNewYorkOffice())
                .Returns(new[] { bobMock.Object });

            var notificationServiceMock = new Moq.Mock<INotificationService>();
            notificationServiceMock.Setup(x => x.SendEmail(bobMock.Object, It.IsAny<string>()));

            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock.Object, employeeServiceMock.Object, loggerMock.Object);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //
            notificationServiceMock.Verify(x => x.SendEmail(bobMock.Object, It.IsAny<string>()), Times.Once);
            notificationServiceMock.Verify(x => x.SendSlackMessage(bobMock.Object, It.IsAny<string>()), Times.Never);

        }


        [Test]
        public void Test_EmployeeInOfficeGetsNotified_UsingStrictMock()
        {
            //
            // Create mocks:
            //
            var loggerMock = new Moq.Mock<ILogger>();

            var bobMock = new Moq.Mock<IEmployee>();
            bobMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);
            bobMock.Setup(x => x.GetNotificationPreference())
                .Returns(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = new Moq.Mock<IEmployeeService>();
            employeeServiceMock.Setup(x => x.GetEmployeesInNewYorkOffice())
                .Returns(new[] { bobMock.Object });

            var notificationServiceMock = new Moq.Mock<INotificationService>(MockBehavior.Strict);
            notificationServiceMock.Setup(x => x.SendEmail(bobMock.Object, It.IsAny<string>()))
                .Verifiable();

            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock.Object, employeeServiceMock.Object, loggerMock.Object);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //

            notificationServiceMock.Verify();

        }

        
        [Test]
        public void Test_ExceptionDoesNotStopNotifications()
        {
            //
            // Create mocks:
            //
            var loggerMock = new Moq.Mock<ILogger>();
            loggerMock.Setup(x => x.Error(It.IsAny<Exception>()));

            var bobMock = new Moq.Mock<IEmployee>();
            bobMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);
            bobMock.Setup(x => x.GetNotificationPreference())
                .Returns(LunchNotifier.NotificationType.Email);

            var marthaMock = new Moq.Mock<IEmployee>();
            marthaMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);
            marthaMock.Setup(x => x.GetNotificationPreference())
                .Returns(LunchNotifier.NotificationType.Email);

            DateTime? incomingDate;
            marthaMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns((Func<DateTime, bool>)(input =>
                {
                    incomingDate = input;
                    return input.DayOfWeek != DayOfWeek.Sunday;
                }));

            var employeeServiceMock = new Moq.Mock<IEmployeeService>();
            employeeServiceMock.Setup(x => x.GetEmployeesInNewYorkOffice())
                .Returns(new[] { bobMock.Object, marthaMock.Object });

            
            var notificationServiceMock = new Moq.Mock<INotificationService>(MockBehavior.Strict);
            notificationServiceMock
                .Setup(x => x.SendEmail(It.IsAny<IEmployee>(), It.IsAny<string>()))
                .Throws<Exception>();

            
            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock.Object, employeeServiceMock.Object, loggerMock.Object);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //

            notificationServiceMock.Verify(x => x.SendEmail(It.IsAny<IEmployee>(), It.IsAny<string>()), Times.Exactly(2));

            loggerMock.Verify(x => x.Error(It.IsAny<Exception>()), Times.Exactly(2));
        }

        [Test]
        public void Test_ExceptionDoesNotStopNotifications_UsingWhenConstraint()
        {
            //
            // Create mocks:
            //
            var loggerMock = new Moq.Mock<ILogger>();
            loggerMock.Setup(x => x.Error(It.IsAny<Exception>()));

            var bobMock = new Moq.Mock<IEmployee>();
            bobMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);
            bobMock.Setup(x => x.GetNotificationPreference())
                .Returns(LunchNotifier.NotificationType.Email);

            var marthaMock = new Moq.Mock<IEmployee>();
            marthaMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);
            marthaMock.Setup(x => x.GetNotificationPreference())
                .Returns(LunchNotifier.NotificationType.Email);

            DateTime? incomingDate;
            marthaMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns((Func<DateTime, bool>)(input =>
                {
                    incomingDate = input;
                    return input.DayOfWeek != DayOfWeek.Sunday;
                }));

            var employeeServiceMock = new Moq.Mock<IEmployeeService>();
            employeeServiceMock.Setup(x => x.GetEmployeesInNewYorkOffice())
                .Returns(new[] { bobMock.Object, marthaMock.Object });

            bool isFirstCall = true;

            var notificationServiceMock = new Moq.Mock<INotificationService>(MockBehavior.Strict);
            notificationServiceMock
                .When(() => isFirstCall)
                .Setup(x => x.SendEmail(It.IsAny<IEmployee>(), It.IsAny<string>()))
                .Callback(() => isFirstCall = false)
                .Throws<Exception>(); ;

            notificationServiceMock
                .When(() => !isFirstCall)
                .Setup(x => x.SendEmail(It.IsAny<IEmployee>(), It.IsAny<string>()));


            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new LunchNotifier(notificationServiceMock.Object, employeeServiceMock.Object, loggerMock.Object);

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications();

            //
            // Check the results:
            //

            notificationServiceMock.Verify(x => x.SendEmail(It.IsAny<IEmployee>(), It.IsAny<string>()), Times.Exactly(2));

            loggerMock.Verify(x => x.Error(It.IsAny<Exception>()), Times.Once);
        }



        [Test]
        [TestCase("2017-01-01 13:00:00", LunchNotifier.LateLunchTemplate)]
        [TestCase("2017-01-01 12:59:59", LunchNotifier.RegularLunchTemplate)]
        public void Test_CorrectTemplateIsUsed_LateLunch(string currentTime, string expectedTemplate)
        {
            //
            // Create mocks:
            //
            var loggerMock = new Moq.Mock<ILogger>();

            var bobMock = new Moq.Mock<IEmployee>();
            bobMock.Setup(x => x.IsWorkingOnDate(It.IsAny<DateTime>()))
                .Returns(true);

            var employeeServiceMock = new Moq.Mock<IEmployeeService>();
            employeeServiceMock.Setup(x => x.GetEmployeesInNewYorkOffice())
                .Returns(new[] { bobMock.Object });

            var notificationServiceMock = new Moq.Mock<INotificationService>();


            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = new Moq.Mock<LunchNotifier>(notificationServiceMock.Object, employeeServiceMock.Object, loggerMock.Object)
            { CallBase = true };
            classUnderTest.Setup(x => x.GetDateTime())
                          .Returns(DateTime.Parse(currentTime));

            //
            // Run some logic to test:
            //
            classUnderTest.Object.SendLunchtimeNotifications_DateTimeSeam();

            //
            // Check the results:
            //
            notificationServiceMock.Verify(x => x.SendEmail(It.IsAny<IEmployee>(), expectedTemplate));
        }


    }

}
