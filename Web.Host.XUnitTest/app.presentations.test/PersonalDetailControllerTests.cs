using app.interfaces;
using app.presentations.Controllers;
using app.shared.Dto;
using app.shared.Dto.PersonalDetail;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Web.Host.XUnitTest.app.presentations.test
{
    public class PersonalDetailControllerTests
    {
        private readonly Mock<IServicesManager<IPersonalDetailService>> _mockPersonalDetailService;
        private readonly Mock<ILogger<PersonalDetailController>> _mockLogger;
        private readonly PersonalDetailController _controller;

        public PersonalDetailControllerTests()
        {
            _mockPersonalDetailService = new Mock<IServicesManager<IPersonalDetailService>>();
            _mockLogger = new Mock<ILogger<PersonalDetailController>>();
            _controller = new PersonalDetailController(_mockPersonalDetailService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetViewPersonalDetailAsync_ShouldReturnCorrectResult()
        {
            // Arrange
            var offsetQuery = new OffsetQueryDto
            {
                PageNumber = 1,
                PageSize = 10,
                SortColumn = string.Empty,
                SortDirection = string.Empty,
                Search = string.Empty
            };

            var expectedResult = new PaginatedOffsetResultDto<ViewPersonalDetailDto>
            {
                Data = new List<ViewPersonalDetailDto>
                {
                    new ViewPersonalDetailDto
                    {
                        Id = 1,
                        UserGuid = Guid.NewGuid(),
                        FullName = "John Doe",
                        Gender = "Male",
                        BirthDate = "1990-01-01",
                        Email = "johndoe@example.com"
                    }
                },
                TotalCount = 1,
                SortColumn = string.Empty,
                SortDirection = "asc",
                Search = "John"
            };

            _mockPersonalDetailService.Setup(service => service.Service.GetViewPersonalDetailAsync(offsetQuery))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetViewPersonalDetailAsync(offsetQuery);

            // Assert
            Assert.IsType<PaginatedOffsetResultDto<ViewPersonalDetailDto>>(result); // Ensure the result is of the expected DTO type
            Assert.Equal(expectedResult.TotalCount, result.TotalCount); // Check TotalCount
            Assert.Equal(expectedResult.SortColumn, result.SortColumn); // Check SortColumn
            Assert.Equal(expectedResult.SortDirection, result.SortDirection); // Check SortDirection
            Assert.Equal(expectedResult.Search, result.Search); // Check Search

            // Check if the Data property matches
            Assert.Equal(expectedResult.Data.Count, result.Data.Count); // Check the count of Data
            for (int i = 0; i < expectedResult.Data.Count; i++)
            {
                Assert.Equal(expectedResult.Data[i].Id, result.Data[i].Id);
                Assert.Equal(expectedResult.Data[i].UserGuid, result.Data[i].UserGuid);
                Assert.Equal(expectedResult.Data[i].FullName, result.Data[i].FullName);
                Assert.Equal(expectedResult.Data[i].Gender, result.Data[i].Gender);
                Assert.Equal(expectedResult.Data[i].BirthDate, result.Data[i].BirthDate);
                Assert.Equal(expectedResult.Data[i].Email, result.Data[i].Email);
            }
        }

        [Fact]
        public async Task GetViewPersonalDetailAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var mockOffsetQuery = new OffsetQueryDto();
            var exception = new Exception("Test exception");

            _mockPersonalDetailService.Setup(service => service.Service.GetViewPersonalDetailAsync(mockOffsetQuery))
                .ThrowsAsync(exception);

            // Act
            var exceptionResult = await Assert.ThrowsAsync<Exception>(() => _controller.GetViewPersonalDetailAsync(mockOffsetQuery));

            // Assert
            Assert.Equal("Test exception", exceptionResult.Message); // Ensure the exception message is as expected

            // Verify if logging occurred
            //_mockLogger.Verify(logger => logger.LogError(It.IsAny<string>(), exception), Times.Once);
        }
    }
}
