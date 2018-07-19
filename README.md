Mocking Framework Examples
=============
As a companion to my blog series on [.NET Mocking Frameworks and how they work](https://wrightfully.com/intro-to-net-mocking-frameworks), this repository attempts to implement the same unit tests with mocks using each of the major mocking frameworks available for .NET.

This project is intended to be a reference for anyone looking to compare .NET mocking frameworks or see syntax examples for a given framework.  It is not intended to be an exhaustive reference, not is it gauranteed to be bug-free (but please do let me know if you find any issues).

# Code Under Test

In order to write tests, we need some code in need of testing.  Thus, I've created the `LunchNotifier`, a slightly-more-than-hello-world class against which unit tests (with mocks) can be written.

## The Pretext

One of the benefits provided to [Stack Overflow](https://stackoverflow.com/company/work-here) emplpoyees working in the New York office is free lunch from in-house chefs.  Every day, when the cooks have completed preparing the meal, a notification goes out to employees in the office to let them know food is available.

## CodeBeingTested Project

In the `CodeBeingTested` project, the `LunchNotifier` class is our main target.

This class takes has five external dependancies that need to be mocked in order to perform our tests:
- `INotificationService` (injected via constructor)
  - _implements the actual notification actions, interfacing with external notification platforms like email or Slack_
-  `ILogger` (injected via constructor)
   - _provides ability to log messages for debugging/troubleshooting_
- `IEmployeeService` (injected via constructor)
  - _used to get a list of employees (as `IEmployee` objects) based in the New York Office_
- `IEmployee` (returned from `IEmployeeService`)
  - _encapsulates employee-specific logic and data_
- `System.DateTime`
  - _the logic will send a different notification template based on the time of day_


The main logic is in `LunchNotifier.SendLunchtimeNotifications()`. This method includes an inline call to `System.DateTime.Now`, which is not mockable by contrained frameworks like Moq or RhinoMocks:

 	public void SendLunchtimeNotifications()
    {
		var now = DateTime.Now;
    	var templateToUse = now.Hour > 12 ? LateLunchTemplate : RegularLunchTemplate;

An alternate implementation is provided for use by contrained frames in `LunchNotifier.SendLunchtimeNotifications_DateTimeSeam()`, where the call to `DateTime.Now` has been extracted into it's own method, which can be replaced in a mock:


    public virtual DateTime GetDateTime() => DateTime.Now;

 	public void SendLunchtimeNotifications_DateTimeSeam()
    {
        var now = GetDateTime();
        var templateToUse = now.Hour > 12 ? LateLunchTemplate : RegularLunchTemplate;


# Sample Projects and Tests

Each of the remaining projects attempt to implement a similar set of unit tests against the `LunchNotifier` using different Mocking Frameworks.

All tests are written using [NUnit v3.10](http://nunit.org/) by way of NuGet package.

In some cases, there are multiple ways to approach a test using the mocking framework, so I have provided multiple variants of the test to showcase the different approaches.

## Test: Test_EmployeeInOfficeGetsNotified

This test creates a mock employee "Bob" who prefers Email notifications and uses mocks to ensure the `LunchNotifier` logic will send him an email when lunch is ready.

## Test: Test_ExceptionDoesNotStopNotifications

This test creates two mock employees, "Bob" and "Martha" and ensures that if sending a notification to one of them results in the error getting logged and does not prevent the remaining employees from getting notified.

## Test: Test_CorrectTemplateIsUsed_LateLunch

This test uses NUnit's `TestCase` syntax to run two test cases, one for before 1pm and one for after 1pm, to ensure the appropriate notification template is used based on the time of day.

For unconstrained frameworks, `DateTime.Now` can be directly controlled to return the test's input time, however, constrained frameworks cannot, so they must extract the call to `DateTime.Now` into a sperate method and modify that new method's behavior instead. Thus the need for the `SendLunchtimeNotifications_DateTimeSeam` variant.


# Frameworks Used

## Moq
Type: Constrained

Project: https://github.com/Moq/moq4/wiki/Quickstart

NuGet: https://www.nuget.org/packages/moq/

## RhinoMocks
Type: Constrained

Project: https://github.com/hibernating-rhinos/rhino-mocks

NuGet: https://www.nuget.org/packages/RhinoMocks

**WARNING:** RhinoMocks is effectively a dead project and will likely not provide support for future changes in .NET, such as .NET Core. I do not suggest you begin using this framework if you're not already using it.

**To run these tests, you must have VS2017 Enterprise (or equiv) installed.**

## NSubstitute
Type: Constrained

Project: http://nsubstitute.github.io/

NuGet: https://www.nuget.org/packages/NSubstitute


## Microsoft Fakes
Type: Unconstrained

[Microsoft Fakes](https://docs.microsoft.com/en-us/visualstudio/test/isolating-code-under-test-with-microsoft-fakes) comes as part of Visual Studio 2017 Enterprise or Visual Studio 2015 Ultimate and is not available as a sperate component.

Fakes does not natively support the `AssertWasCalled` style validation, so I utilize an additional NuGet package (which I have contributed to) to provide that feature: [Fakes.Contrib](https://github.com/fvilers/Fakes.Contrib) / [NuGet](https://www.nuget.org/packages/Fakes.Contrib/)



## Typemock Isolator
Type: Unconstrained

Product Info: [https://www.typemock.com/isolator](https://www.typemock.com/isolator)

This product requires [a paid license](https://www.typemock.com/pricing) (there's a free 15-day trial available) to do unconstrained mocking.  Typemock was nice enought to provide
me with a free community license so that I can work on projects like this one.