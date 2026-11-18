
# Unit Testing Quick Guide - .NET 10

## Setup

### Required NuGet Packages
```
NUnit (3.14.0)
NUnit3TestAdapter (4.5.0)
Microsoft.NET.Test.Sdk (17.8.0)
FluentAssertions (8.8.0)
NSubstitute (5.3.0)
WireMock.Net (2.15.0)
Microsoft.AspNetCore.Mvc.Testing (10.0.0)
coverlet.collector (6.0.0)
```

### Test Project Template
```
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <RootNamespace>API.UnitTests</RootNamespace>
  <ImplicitUsings>disable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

---

## Test Structure

### Naming Convention
- **Class**: `<ClassName>Tests` or `<ClassName>TestFixture`
- **Method**: `<MethodName>_<Scenario>_<ExpectedOutcome>`

### Basic Test Template
```
[TestFixture]
public class BookingServiceTests
{
	private BookingService? _sut; // System Under Test

	[SetUp]
	public void Setup()
	{
		_sut = new BookingService();
	}

	[Test]
	public void CreateBooking_WithValidData_ReturnsBookingId()
	{
		// Arrange
		var booking = new Booking { CustomerId = 123 };

		// Act
		var result = _sut.CreateBooking(booking);

		// Assert
		result.Should().BeGreaterThan(0);
	}
}
```

---

## Core Patterns

### 1. Happy Path
```
[Test]
public void GetBooking_WithValidId_ReturnsBooking()
{
	var result = _sut.GetBooking(1);
	result.Should().NotBeNull();
}
```

### 2. Exception Handling
```
[Test]
public void GetBooking_WithInvalidId_ThrowsNotFoundException()
{
	var act = () => _sut.GetBooking(-1);
	act.Should().Throw<NotFoundException>();
}
```

### 3. Parameterized Tests
```
[TestCase(1, 1, 2)]
[TestCase(2, 3, 5)]
[TestCase(-1, 1, 0)]
public void Calculator_Add_ReturnsCorrectSum(int a, int b, int expected)
{
	var result = _sut.Add(a, b);
	result.Should().Be(expected);
}
```

### 4. Collections
```
[Test]
public void GetAllBookings_ReturnsAllUserBookings()
{
	var result = _sut.GetUserBookings(100);
	result.Should().HaveCount(2);
	result.Should().AllSatisfy(b => b.CustomerId.Should().Be(100));
}
```

---

## Common Assertions (FluentAssertions)

```
// String
result.Should().Be("expected");
result.Should().Contain("substring");
result.Should().StartWith("prefix");

// Numeric
result.Should().BeGreaterThan(10);
result.Should().BeLessThanOrEqualTo(100);

// Object
result.Should().NotBeNull();
result.Should().BeOfType<Booking>();

// Collection
result.Should().HaveCount(5);
result.Should().BeEmpty();
result.Should().Contain(item);

// Exception
action.Should().Throw<ArgumentNullException>();

// Boolean
result.Should().BeTrue();
```

---

## Mocking with NSubstitute

```
[SetUp]
public void Setup()
{
	_mockRepository = Substitute.For<IRepository>();
	_sut = new Service(_mockRepository);
}

[Test]
public void Service_CallsRepository()
{
	_mockRepository.GetData().Returns("test data");

	var result = _sut.DoSomething();

	_mockRepository.Received(1).GetData();
}

// Argument matching
_mockRepository.GetById(Arg.Any<int>()).Returns(booking);
_mockRepository.GetById(123).Returns(booking);
_mockRepository.GetById(Arg.Is<int>(x => x > 0)).Returns(booking);

// Exceptions
_mockRepository.GetById(Arg.Any<int>()).Throws<NotFoundException>();
```

---

## Integration Testing

### HTTP Client Testing
```
[TestFixture]
public class BookingControllerIntegrationTests
{
	private WebApplicationFactory<Program>? _factory;
	private HttpClient? _client;

	[SetUp]
	public void Setup()
	{
		_factory = new WebApplicationFactory<Program>();
		_client = _factory.CreateClient();
	}

	[Test]
	public async Task GetBooking_ReturnsOkResult()
	{
		var response = await _client.GetAsync("/api/bookings/1");
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
```

### Mock External APIs with WireMock
```
var mockServer = WireMockServer.Start();
mockServer
	.Given(Request.Create().WithPath("/api/bookings/1").UsingGet())
	.RespondWith(Response.Create()
		.WithStatusCode(200)
		.WithBodyAsJson(new { id = 1 }));

var client = new ExternalApiClient(mockServer.Urls[0]);
var result = await client.GetBooking(1);

mockServer.Stop();
```

---

## Running Tests

### Visual Studio
- **Test Explorer**: View → Other Windows → Test Explorer
- **Run All**: Test → Run All Tests
- **Run Specific**: Right-click test → Run

### PowerShell
```
# Run all tests
dotnet test

# Run specific project
dotnet test .\API.test\API.test.csproj

# Verbose output
dotnet test --verbosity detailed

# With code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter TestFixture.TestMethod
```

---

## Best Practices

✅ **Do**
- One logical assertion per test (or use FluentAssertions chains)
- Use meaningful test names
- Keep tests independent - use `[SetUp]` for fresh state
- Test behavior, not implementation
- Follow Arrange-Act-Assert pattern
- Mock external dependencies

❌ **Don't**
- Test multiple scenarios in one test
- Use actual external services (always mock)
- Skip negative test cases
- Hard-code magic numbers
- Ignore test failures

---

## Project Organization

```
API.test/
├── Controllers/
│   └── BookingControllerTests.cs
├── Services/
│   └── BookingServiceTests.cs
├── Mocks/
│   └── MockData.cs
├── TestData/
│   └── *.json
├── GlobalUsings.cs
└── API.test.csproj
```

---

## Quick Reference: Test Method Template

```
[TestFixture]
public class YourServiceTests
{
	private YourService? _sut;
	private IADependency? _mockDependency;

	[SetUp]
	public void Setup()
	{
		_mockDependency = Substitute.For<IADependency>();
		_sut = new YourService(_mockDependency);
	}

	[Test]
	[Category("Happy Path")]
	public void Method_Scenario_Expected()
	{
		// Arrange

		// Act
		var result = _sut.Method();

		// Assert
		result.Should().NotBeNull();
	}

	[Test]
	[Category("Exception Handling")]
	public void Method_InvalidInput_ThrowsException()
	{
		var act = () => _sut.Method(null!);
		act.Should().Throw<ArgumentNullException>();
	}
}
```

---

**Target Framework**: .NET 10.0  
**Test Framework**: NUnit 3.14+  
**Project**: API.test  
**Last Updated**: 2026-11-03
  