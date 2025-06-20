using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Panda.Billbee.CustomShopSdk.AspNetCore.Controllers;
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Models;

namespace Panda.Billbee.CustomShopSdk.AspNetCore.Tests;

public class BillbeeControllerBaseTests
{
    private readonly Mock<IBillbeeCustomShopService> _mockService;
    private readonly TestBillbeeController _controller;

    public BillbeeControllerBaseTests()
    {
        _mockService = new Mock<IBillbeeCustomShopService>();
        _controller = new TestBillbeeController(_mockService.Object);
        
        // Setup controller context
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?action=GetOrders");
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task HandleGetRequest_ShouldReturnOkResult_WhenServiceSucceeds()
    {
        // Arrange
        const string action = "GetOrders";
        const string key = "test-key";
        var expectedData = new { Orders = new[] { "Order1", "Order2" } };
        var billbeeRequest = new BillbeeRequest { Method = "GET", Action = action };
        var serviceResult = ServiceResult.Success(billbeeRequest, expectedData);
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.HandleGetRequest(action, key);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal(expectedData, okResult.Value);
    }

    [Fact]
    public async Task HandleGetRequest_ShouldReturnUnauthorized_WithWWWAuthenticateHeader()
    {
        // Arrange
        const string action = "GetOrders";
        var billbeeRequest = new BillbeeRequest { Method = "GET", Action = action };
        var serviceResult = ServiceResult.Unauthorized(billbeeRequest);
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.HandleGetRequest(action);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.True(_controller.Response.Headers.ContainsKey("WWW-Authenticate"));
        Assert.Equal("Basic realm=\"Billbee API\"", _controller.Response.Headers["WWW-Authenticate"].ToString());
    }

    [Fact]
    public async Task HandlePostRequest_ShouldReturnOkResult_WhenServiceSucceeds()
    {
        // Arrange
        const string action = "AckOrder";
        const string key = "test-key";
        const string expectedData = "OK";
        var billbeeRequest = new BillbeeRequest { Method = "POST", Action = action };
        var serviceResult = ServiceResult.Success(billbeeRequest, expectedData);
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.HandlePostRequest(action, key);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal(expectedData, okResult.Value);
    }

    [Fact]
    public async Task HandlePostRequest_ShouldReturnUnauthorized_WithWWWAuthenticateHeader()
    {
        // Arrange
        const string action = "AckOrder";
        var billbeeRequest = new BillbeeRequest { Method = "POST", Action = action };
        var serviceResult = ServiceResult.Unauthorized(billbeeRequest);
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.HandlePostRequest(action);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.True(_controller.Response.Headers.ContainsKey("WWW-Authenticate"));
        Assert.Equal("Basic realm=\"Billbee API\"", _controller.Response.Headers["WWW-Authenticate"].ToString());
    }

    [Theory]
    [InlineData("GetOrders", null)]
    [InlineData("GetOrder", "12345")]
    [InlineData("GetProducts", "test-key")]
    public async Task HandleGetRequest_ShouldPassCorrectParameters_ToService(string action, string? key)
    {
        // Arrange
        var billbeeRequest = new BillbeeRequest { Method = "GET", Action = action };
        var serviceResult = ServiceResult.Success(billbeeRequest, "test");
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(serviceResult);

        // Act
        await _controller.HandleGetRequest(action, key);

        // Assert
        _mockService.Verify(s => s.HandleRequestAsync(It.Is<BillbeeRequest>(r => 
            r.Action == action && r.Key == key)), Times.Once);
    }

    [Theory]
    [InlineData("AckOrder", null)]
    [InlineData("SetOrderState", "test-key")]
    [InlineData("SetStock", "12345")]
    public async Task HandlePostRequest_ShouldPassCorrectParameters_ToService(string action, string? key)
    {
        // Arrange
        var billbeeRequest = new BillbeeRequest { Method = "POST", Action = action };
        var serviceResult = ServiceResult.Success(billbeeRequest, "test");
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(serviceResult);

        // Act
        await _controller.HandlePostRequest(action, key);

        // Assert
        _mockService.Verify(s => s.HandleRequestAsync(It.Is<BillbeeRequest>(r => 
            r.Action == action && r.Key == key)), Times.Once);
    }

    private class TestBillbeeController : BillbeeControllerBase
    {
        private readonly IBillbeeCustomShopService _service;

        public TestBillbeeController(IBillbeeCustomShopService service) : base(null)
        {
            _service = service;
        }

        protected override IBillbeeCustomShopService BillbeeService => _service;
    }
}