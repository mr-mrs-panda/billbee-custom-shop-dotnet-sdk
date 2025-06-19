using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Panda.Billbee.CustomShopSdk.AspNetCore.Helpers;
using Panda.Billbee.CustomShopSdk.Constants;
using Panda.Billbee.CustomShopSdk.Interfaces;
using Panda.Billbee.CustomShopSdk.Models;

namespace Panda.Billbee.CustomShopSdk.AspNetCore.Tests;

public class BillbeeControllerHelperTests
{
    private readonly Mock<IBillbeeCustomShopService> _mockService;
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<IQueryCollection> _mockQuery;
    private readonly Mock<IFormCollection> _mockForm;

    public BillbeeControllerHelperTests()
    {
        _mockService = new Mock<IBillbeeCustomShopService>();
        _mockRequest = new Mock<HttpRequest>();
        _mockQuery = new Mock<IQueryCollection>();
        _mockForm = new Mock<IFormCollection>();
    }

    [Fact]
    public async Task HandleGetRequestAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        const string action = "GetOrders";
        const string key = "test-key";
        var expectedResult = ServiceResult.Success("test-data");
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(expectedResult);
        
        SetupMockRequest();

        // Act
        var result = await BillbeeControllerHelper.HandleGetRequestAsync(_mockService.Object, _mockRequest.Object, action, key);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal("test-data", okResult.Value);
        
        _mockService.Verify(s => s.HandleRequestAsync(It.Is<BillbeeRequest>(r => 
            r.Method == BillbeeMethods.Get && 
            r.Action == action && 
            r.Key == key)), Times.Once);
    }

    [Fact]
    public async Task HandlePostRequestAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        const string action = "AckOrder";
        const string key = "test-key";
        var expectedResult = ServiceResult.Success("OK");
        
        _mockService.Setup(s => s.HandleRequestAsync(It.IsAny<BillbeeRequest>()))
                   .ReturnsAsync(expectedResult);
        
        SetupMockRequest();

        // Act
        var result = await BillbeeControllerHelper.HandlePostRequestAsync(_mockService.Object, _mockRequest.Object, action, key);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal("OK", okResult.Value);
        
        _mockService.Verify(s => s.HandleRequestAsync(It.Is<BillbeeRequest>(r => 
            r.Method == BillbeeMethods.Post && 
            r.Action == action && 
            r.Key == key)), Times.Once);
    }

    [Fact]
    public void CreateBillbeeRequest_ShouldMapQueryParameters()
    {
        // Arrange
        const string method = "GET";
        const string action = "GetOrders";
        const string key = "test-key";
        
        var queryDict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["StartDate"] = "2024-01-01",
            ["Page"] = "1",
            ["PageSize"] = "100"
        };
        
        _mockQuery.Setup(q => q.GetEnumerator()).Returns(queryDict.GetEnumerator());
        _mockRequest.Setup(r => r.Query).Returns(_mockQuery.Object);
        _mockRequest.Setup(r => r.HasFormContentType).Returns(false);
        _mockRequest.Setup(r => r.Headers.Authorization).Returns(new Microsoft.Extensions.Primitives.StringValues("Bearer token"));

        // Act
        var result = BillbeeControllerHelper.CreateBillbeeRequest(method, action, key, _mockRequest.Object);

        // Assert
        Assert.Equal(method, result.Method);
        Assert.Equal(action, result.Action);
        Assert.Equal(key, result.Key);
        Assert.Equal("Bearer token", result.AuthorizationHeader);
        Assert.Equal("2024-01-01", result.QueryParameters["StartDate"]);
        Assert.Equal("1", result.QueryParameters["Page"]);
        Assert.Equal("100", result.QueryParameters["PageSize"]);
    }

    [Fact]
    public void CreateBillbeeRequest_ShouldMapFormParameters_WhenPostRequest()
    {
        // Arrange
        const string method = "POST";
        const string action = "AckOrder";
        
        var formDict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["OrderId"] = "12345",
            ["Comment"] = "Order processed"
        };
        
        _mockForm.Setup(f => f.GetEnumerator()).Returns(formDict.GetEnumerator());
        _mockQuery.Setup(q => q.GetEnumerator()).Returns(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>().GetEnumerator());
        _mockRequest.Setup(r => r.Query).Returns(_mockQuery.Object);
        _mockRequest.Setup(r => r.Form).Returns(_mockForm.Object);
        _mockRequest.Setup(r => r.HasFormContentType).Returns(true);
        _mockRequest.Setup(r => r.Headers.Authorization).Returns(new Microsoft.Extensions.Primitives.StringValues());

        // Act
        var result = BillbeeControllerHelper.CreateBillbeeRequest(method, action, null, _mockRequest.Object);

        // Assert
        Assert.Equal(method, result.Method);
        Assert.Equal(action, result.Action);
        Assert.Equal("12345", result.FormParameters["OrderId"]);
        Assert.Equal("Order processed", result.FormParameters["Comment"]);
    }

    [Theory]
    [InlineData(ServiceErrorType.Unauthorized, typeof(UnauthorizedObjectResult), 401)]
    [InlineData(ServiceErrorType.NotFound, typeof(NotFoundObjectResult), 404)]
    [InlineData(ServiceErrorType.BadRequest, typeof(BadRequestObjectResult), 400)]
    [InlineData(ServiceErrorType.Forbidden, typeof(ObjectResult), 403)]
    [InlineData(ServiceErrorType.InternalServerError, typeof(ObjectResult), 500)]
    public void ConvertToActionResult_ShouldReturnCorrectErrorResult(ServiceErrorType errorType, Type expectedType, int expectedStatusCode)
    {
        // Arrange
        ServiceResult serviceResult = errorType switch
        {
            ServiceErrorType.Unauthorized => ServiceResult.Unauthorized("Test error message"),
            ServiceErrorType.NotFound => ServiceResult.NotFound("Test error message"),
            ServiceErrorType.BadRequest => ServiceResult.BadRequest("Test error message"),
            ServiceErrorType.Forbidden => ServiceResult.Forbidden("Test error message"),
            ServiceErrorType.InternalServerError => ServiceResult.InternalServerError("Test error message"),
            _ => throw new ArgumentOutOfRangeException(nameof(errorType))
        };

        // Act
        var result = BillbeeControllerHelper.ConvertToActionResult(serviceResult);

        // Assert
        Assert.IsType(expectedType, result);
        
        if (result is ObjectResult objectResult)
        {
            Assert.Equal(expectedStatusCode, objectResult.StatusCode);
            Assert.Equal("Test error message", objectResult.Value);
        }
    }

    [Fact]
    public void ConvertToActionResult_ShouldReturnOkResult_WhenServiceResultIsSuccess()
    {
        // Arrange
        var testData = new { Message = "Success" };
        var serviceResult = ServiceResult.Success(testData);

        // Act
        var result = BillbeeControllerHelper.ConvertToActionResult(serviceResult);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal(testData, okResult.Value);
    }

    [Fact]
    public void CreateUnauthorizedResult_ShouldReturnUnauthorizedWithMessage()
    {
        // Arrange
        const string message = "Access denied";

        // Act
        var result = BillbeeControllerHelper.CreateUnauthorizedResult(message);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        var unauthorizedResult = (UnauthorizedObjectResult)result;
        Assert.Equal(401, unauthorizedResult.StatusCode);
        Assert.Equal(message, unauthorizedResult.Value);
    }

    private void SetupMockRequest()
    {
        _mockQuery.Setup(q => q.GetEnumerator()).Returns(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>().GetEnumerator());
        _mockRequest.Setup(r => r.Query).Returns(_mockQuery.Object);
        _mockRequest.Setup(r => r.HasFormContentType).Returns(false);
        _mockRequest.Setup(r => r.Headers.Authorization).Returns(new Microsoft.Extensions.Primitives.StringValues());
    }
}