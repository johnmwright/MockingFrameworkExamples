using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CodeBeingTested;
using CodeBeingTested.Fakes;
using CodeBeingTested.Interfaces;
using Fakes.Contrib;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.QualityTools.Testing.Fakes.Stubs;
using NUnit.Framework;
using CodeBeingTested.Interfaces.Fakes;
using Microsoft.QualityTools.Testing.Fakes.Shims;

namespace MSFakesExamples
{

    [TestFixture]
    class MsFakesTests
    {

        [Test]
        public void Test_EmployeeInOfficeGetsNotified()
        {
          
            //
            // Create mocks:
            //

            var loggerMock = new StubILogger();

            var bobMock = new StubIEmployee
            {
                IsWorkingOnDateDateTime = x => true,
                GetNotificationPreference = () => LunchNotifier.NotificationType.Email
            };

            var employeeServiceMock = new StubIEmployeeService
                {
                    GetEmployeesInNewYorkOffice = () => new[] {bobMock}
                };

            var notificationServiceMock = new StubINotificationService()
                .AsObservable();
            
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
            notificationServiceMock.AssertWasCalled(x => x.SendEmail(bobMock, With.Any<string>()));
            notificationServiceMock.AssertWasNotCalled(x => x.SendSlackMessage(With.Any<IEmployee>(), With.Any<string>()));
            
        }


        [Test]
        public void Test_EmployeeInOfficeGetsNotified_UsingStrictMock()
        {
            //
            // Create mocks:
            //
            var loggerMock = new StubILogger();

            var bobMock = new StubIEmployee
            {
                IsWorkingOnDateDateTime = x => true,
                GetNotificationPreference = () => LunchNotifier.NotificationType.Email
            };

            var employeeServiceMock = new StubIEmployeeService
            {
                GetEmployeesInNewYorkOffice = () => new[] { bobMock }
            };

            var notificationServiceMock = new StubINotificationService() { InstanceBehavior = StubBehaviors.NotImplemented}
                .AsObservable();

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

            notificationServiceMock.AssertWasCalled(x => x.SendEmail(bobMock, With.Any<string>()));


        }


        [Test]
        public void Test_ExceptionDoesNotStopNotifications()
        {
            //
            // Create mocks:
            //
            var loggerMock = new StubILogger().AsObservable();

            var bobMock = new StubIEmployee
            {
                IsWorkingOnDateDateTime = x => true,
                GetNotificationPreference = () => LunchNotifier.NotificationType.Email
            };

            DateTime? incomingDate;
            var marthaMock = new StubIEmployee
            {                
                GetNotificationPreference = () => LunchNotifier.NotificationType.Email,
                IsWorkingOnDateDateTime = (input) => {
                    incomingDate = input;
                    return input.DayOfWeek != DayOfWeek.Sunday;
                }
            };
            
            var employeeServiceMock = new StubIEmployeeService
            {
                GetEmployeesInNewYorkOffice = () => new IEmployee[] { bobMock, marthaMock }
            };


            var notificationServiceMock = new StubINotificationService()
                {
                    SendEmailIEmployeeString = (emply, tmpl) => throw new Exception()
                }
                .AsObservable();


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
            notificationServiceMock.AssertWasCalled(x => x.SendEmail(bobMock, With.Any<string>()));
            notificationServiceMock.AssertWasCalled(x => x.SendEmail(marthaMock, With.Any<string>()));

            loggerMock.AssertWasCalled(x => x.Error(With.Any<Exception>()));
            //FIXME: Assert times called = 2
        }
        


        [Test]
        [TestCase("2017-01-01 13:00:00", LunchNotifier.LateLunchTemplate)]
        [TestCase("2017-01-01 12:59:59", LunchNotifier.RegularLunchTemplate)]
        public void Test_CorrectTemplateIsUsed_LateLunch(string currentTime, string expectedTemplate)
        {
            //
            // Create mocks:
            //
            var loggerMock = new StubILogger();

            var bobMock = new StubIEmployee
            {
                IsWorkingOnDateDateTime = x => true
            };

            var employeeServiceMock = new StubIEmployeeService
            {
                GetEmployeesInNewYorkOffice = () => new[] { bobMock }
            };

           

            using (ShimsContext.Create())
            {
                var notificationServiceMock = new StubINotificationService().AsObservable();

                System.Fakes.ShimDateTime.NowGet = () => DateTime.Parse(currentTime);
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
                notificationServiceMock.AssertWasCalled(x => x.SendEmail(With.Any<IEmployee>(), expectedTemplate));

            }
        }


    }
    
}


