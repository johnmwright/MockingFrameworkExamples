using System;
using CodeBeingTested;
using CodeBeingTested.Interfaces;
using NUnit.Framework;
using Rhino.Mocks;

namespace RhinoMocksExamples
{
    [TestFixture]
    public class RhinoMocksTests
    {
        [Test]
        public void Test_EmployeeInOfficeGetsNotified()
        {
            //
            // Create mocks:
            //
            var loggerMock = MockRepository.GenerateStub<ILogger>();

            var bobMock = MockRepository.GenerateMock<IEmployee>();
            bobMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))                
                .Return(true);
            bobMock.Expect(x => x.GetNotificationPreference())
                .Return(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = MockRepository.GenerateMock<IEmployeeService>();
            employeeServiceMock.Expect(x => x.GetEmployeesInNewYorkOffice())
                .Return(new[] { bobMock });

            var notificationServiceMock = MockRepository.GenerateMock<INotificationService>();
            notificationServiceMock.Expect(x => x.SendEmail(Arg<IEmployee>.Is.Equal(bobMock), Arg<string>.Is.Anything));

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
            notificationServiceMock.AssertWasCalled(x => x.SendEmail(Arg<IEmployee>.Is.Equal(bobMock), Arg<string>.Is.Anything));
            notificationServiceMock.AssertWasNotCalled(x => x.SendSlackMessage(Arg<IEmployee>.Is.Equal(bobMock), Arg<string>.Is.Anything));

        }


        [Test]
        public void Test_EmployeeInOfficeGetsNotified_UsingStrictMock()
        {
            //
            // Create mocks:
            //
            var loggerMock = MockRepository.GenerateStub<ILogger>();

            var bobMock = MockRepository.GenerateStub<IEmployee>();
            bobMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Return(true);
            bobMock.Stub(x => x.GetNotificationPreference())
                .Return(LunchNotifier.NotificationType.Email);

            var employeeServiceMock = MockRepository.GenerateStub<IEmployeeService>();
            employeeServiceMock.Stub(x => x.GetEmployeesInNewYorkOffice())
                .Return(new[] { bobMock });

            var notificationServiceMock = MockRepository.GenerateStrictMock<INotificationService>();
            notificationServiceMock.Expect(x => x.SendEmail(Arg<IEmployee>.Is.Equal(bobMock), Arg<string>.Is.Anything));

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

            notificationServiceMock.VerifyAllExpectations();

        }


        [Test]
        public void Test_ExceptionDoesNotStopNotifications_UsingDoDelegate()
        {
            //
            // Create mocks:
            //
            var loggerMock = MockRepository.GenerateMock<ILogger>();
            loggerMock.Expect(x => x.Error(Arg<Exception>.Is.Anything));

            var bobMock = MockRepository.GenerateMock<IEmployee>();
            bobMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Return(true);
            bobMock.Expect(x => x.GetNotificationPreference())
                .Return(LunchNotifier.NotificationType.Email);

            var marthaMock = MockRepository.GenerateMock<IEmployee>();
            marthaMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Return(true);
            marthaMock.Expect(x => x.GetNotificationPreference())
                .Return(LunchNotifier.NotificationType.Email);

            DateTime? incomingDate;
            marthaMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Do((Func<DateTime, bool>)(input =>
                {
                    incomingDate = input;
                    return input.DayOfWeek != DayOfWeek.Sunday;
                }));

            var employeeServiceMock = MockRepository.GenerateMock<IEmployeeService>();
            employeeServiceMock.Expect(x => x.GetEmployeesInNewYorkOffice())
                .Return(new[] { bobMock, marthaMock });


            var notificationServiceMock = MockRepository.GenerateStrictMock<INotificationService>();
            notificationServiceMock.Expect(x => x.SendEmail(null, null))
                .IgnoreArguments()
                .Throw(new Exception());


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

            notificationServiceMock.AssertWasCalled(x => x.SendEmail(Arg<IEmployee>.Is.Anything, Arg<string>.Is.Anything), options => options.Repeat.Times(2));

            loggerMock.AssertWasCalled(x => x.Error(Arg<Exception>.Is.Anything), options => options.Repeat.Times(2));
        }

        [Test]
        public void Test_ExceptionDoesNotStopNotifications_UsingInputConstraint()
        {
            //
            // Create mocks:
            //
            var loggerMock = MockRepository.GenerateMock<ILogger>();
            loggerMock.Expect(x => x.Error(Arg<Exception>.Is.Anything));

            var bobMock = MockRepository.GenerateMock<IEmployee>();
            bobMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Return(true);
            bobMock.Expect(x => x.GetNotificationPreference())
                .Return(LunchNotifier.NotificationType.Email);

            var marthaMock = MockRepository.GenerateMock<IEmployee>();
            marthaMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Return(true);
            marthaMock.Expect(x => x.GetNotificationPreference())
                .Return(LunchNotifier.NotificationType.Email);

            DateTime? incomingDate;
            marthaMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Do((Func<DateTime, bool>)(input =>
                {
                    incomingDate = input;
                    return input.DayOfWeek != DayOfWeek.Sunday;
                }));

            var employeeServiceMock = MockRepository.GenerateMock<IEmployeeService>();
            employeeServiceMock.Expect(x => x.GetEmployeesInNewYorkOffice())
                .Return(new[] { bobMock, marthaMock });
            
            var notificationServiceMock = MockRepository.GenerateStrictMock<INotificationService>();
            notificationServiceMock
                .Expect(x => x.SendEmail(Arg<IEmployee>.Is.Equal(bobMock), Arg<string>.Is.Anything))
                .Throw(new Exception());

            notificationServiceMock                
                .Expect(x => x.SendEmail(Arg<IEmployee>.Is.Equal(marthaMock), Arg<string>.Is.Anything));


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

            notificationServiceMock.AssertWasCalled(x => x.SendEmail(Arg<IEmployee>.Is.Anything, Arg<string>.Is.Anything), options => options.Repeat.Twice());

            loggerMock.AssertWasCalled(x => x.Error(Arg<Exception>.Is.Anything), options => options.Repeat.Once());
        }



        [Test]
        [TestCase("2017-01-01 13:00:00", LunchNotifier.LateLunchTemplate)]
        [TestCase("2017-01-01 12:59:59", LunchNotifier.RegularLunchTemplate)]
        public void Test_CorrectTemplateIsUsed_LateLunch(string currentTime, string expectedTemplate)
        {
            //
            // Create mocks:
            //
            var loggerMock = MockRepository.GenerateStub<ILogger>();

            var bobMock = MockRepository.GenerateMock<IEmployee>();
            bobMock.Expect(x => x.IsWorkingOnDate(Arg<DateTime>.Is.Anything))
                .Return(true);

            var employeeServiceMock = MockRepository.GenerateMock<IEmployeeService>();
            employeeServiceMock.Expect(x => x.GetEmployeesInNewYorkOffice())
                .Return(new[] { bobMock });

            var notificationServiceMock = MockRepository.GenerateMock<INotificationService>();


            //
            // Create instance of class I'm testing:
            //
            var classUnderTest = MockRepository.GeneratePartialMock<LunchNotifier>(notificationServiceMock, employeeServiceMock, loggerMock);
            classUnderTest.Expect(x => x.GetDateTime())
                          .Return(DateTime.Parse(currentTime));

            //
            // Run some logic to test:
            //
            classUnderTest.SendLunchtimeNotifications_DateTimeSeam();

            //
            // Check the results:
            //
            notificationServiceMock.AssertWasCalled(x => x.SendEmail(Arg<IEmployee>.Is.Anything, Arg<string>.Is.Equal(expectedTemplate)));
        }


    }

}
