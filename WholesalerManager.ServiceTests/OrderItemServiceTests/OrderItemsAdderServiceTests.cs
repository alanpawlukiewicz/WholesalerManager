using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using WholesalerManager.Core.DTO.OrderItemDTO;
using WholesalerManager.Core.DTO.ProductDTO;
using WholesalerManager.Core.Exceptions;
using WholesalerManager.Core.ServiceContracts.OrderItemServiceContracts;
using WholesalerManager.Core.ServiceContracts.ProductServiceContracts;
using WholesalerManager.Core.Services.OrderItemServices;

namespace WholesalerManager.ServiceTests.OrderItemServiceTests
{

    public class OrderItemsAdderServiceTests
    {
        private readonly Mock<IOrderItemsRepository> _orderItemsRepositoryMock;
        private readonly Mock<ILogger<OrderItemsAdderService>> _loggerMock;
        private readonly Mock<IProductsGetterService> _productsGetterServiceMock;
        private readonly IFixture _fixture;
        private readonly IOrderItemsAdderService _sut;

        public OrderItemsAdderServiceTests()
        {
            _orderItemsRepositoryMock = new Mock<IOrderItemsRepository>();
            _loggerMock = new Mock<ILogger<OrderItemsAdderService>>();
            _productsGetterServiceMock = new Mock<IProductsGetterService>();

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                              .ToList()
                              .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _sut = new OrderItemsAdderService(
                _orderItemsRepositoryMock.Object,
                _loggerMock.Object,
                _productsGetterServiceMock.Object
            );
        }

        #region Helpers

        // Creates a valid OrderItemAddRequest with all required fields filled
        private OrderItemAddRequest CreateValidItemAddRequest(Action<OrderItemAddRequest>? configure = null)
        {
            var request = _fixture.Build<OrderItemAddRequest>()
                .With(r => r.OrderID, Guid.NewGuid())
                .With(r => r.ProductID, Guid.NewGuid())
                .With(r => r.Quantity, 5)
                .With(r => r.PriceAtSale, "19.99")
                .Create();

            configure?.Invoke(request);
            return request;
        }

        // Creates a ProductResponse with sufficient stock for the given quantity
        private ProductResponse CreateProductWithStock(Guid productId, int stockQuantity)
        {
            return _fixture.Build<ProductResponse>()
                .With(p => p.ProductID, productId)
                .With(p => p.StockQuantity, stockQuantity)
                .Create();
        }

        // Creates an OrderItem entity returned by the repository after adding
        private OrderItem CreateAddedOrderItem(OrderItemAddRequest request)
        {
            return _fixture.Build<OrderItem>()
                .With(i => i.OrderID, request.OrderID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();
        }

        // Sets up a full successful AddOrderItem flow
        private void SetupSuccessfulAdd(OrderItemAddRequest request, int stockQuantity = 100)
        {
            var product = CreateProductWithStock(request.ProductID!.Value, stockQuantity);
            var addedItem = CreateAddedOrderItem(request);

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request.ProductID))
                .ReturnsAsync(product);

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);
        }

        #endregion

        #region AddOrderItem

        [Fact]
        public async Task AddOrderItem_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert – null request should throw before any service or repository call
            await _sut.Invoking(s => s.AddOrderItem(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _productsGetterServiceMock.Verify(s => s.GetProductById(It.IsAny<Guid?>()), Times.Never);
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddOrderItem_MissingProductID_ThrowsArgumentException()
        {
            // Arrange – ProductID is required by validation
            var request = CreateValidItemAddRequest(r => r.ProductID = null);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsGetterServiceMock.Verify(s => s.GetProductById(It.IsAny<Guid?>()), Times.Never);
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.345")]
        [InlineData("-5.00")]
        public async Task AddOrderItem_InvalidPriceAtSale_ThrowsArgumentException(string price)
        {
            // Arrange – PriceAtSale must match the money regex pattern
            var request = CreateValidItemAddRequest(r => r.PriceAtSale = price);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().ThrowAsync<ArgumentException>();

            _productsGetterServiceMock.Verify(s => s.GetProductById(It.IsAny<Guid?>()), Times.Never);
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AddOrderItem_NullOrEmptyPriceAtSale_DoesNotThrowAndSetsPriceToZero(string? price)
        {
            // Arrange – null and empty string pass validation (no [Required] attribute)
            // and ToDecimalSafe() converts them to 0
            var request = CreateValidItemAddRequest(r => r.PriceAtSale = price);
            var addedItem = _fixture.Build<OrderItem>()
                .With(i => i.OrderID, request.OrderID)
                .With(i => i.ProductID, request.ProductID)
                .With(i => i.Quantity, request.Quantity)
                .With(i => i.PriceAtSale, 0m)
                .With(i => i.Product, _fixture.Build<Product>()
                    .With(p => p.Category, _fixture.Create<Category>())
                    .Create())
                .Create();

            var product = CreateProductWithStock(request.ProductID!.Value, 100);

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request.ProductID))
                .ReturnsAsync(product);

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.Invoking(s => s.AddOrderItem(request))
                                    .Should().NotThrowAsync();

            // Assert – price defaults to 0 when null or empty is provided
            result.Subject.PriceAtSale.Should().Be(0m);
        }

        [Fact]
        public async Task AddOrderItem_ProductNotFound_ThrowsArgumentNullException()
        {
            // Arrange – product referenced by the request does not exist
            var request = CreateValidItemAddRequest();

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request.ProductID))
                .ReturnsAsync((ProductResponse?)null);

            // Act & Assert
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().ThrowAsync<ArgumentNullException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddOrderItem_InsufficientStock_ThrowsInsufficientProductStockException()
        {
            // Arrange – ordered quantity exceeds available stock
            var request = CreateValidItemAddRequest(r => r.Quantity = 10);
            var product = CreateProductWithStock(request.ProductID!.Value, stockQuantity: 5);

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request.ProductID))
                .ReturnsAsync(product);

            // Act & Assert – custom exception thrown when stock is insufficient
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().ThrowAsync<InsufficientProductStockException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddOrderItem_ExactStockMatch_DoesNotThrow()
        {
            // Arrange – ordered quantity exactly equals available stock
            var request = CreateValidItemAddRequest(r => r.Quantity = 10);

            SetupSuccessfulAdd(request, stockQuantity: 10);

            // Act & Assert – exact match should be allowed
            await _sut.Invoking(s => s.AddOrderItem(request))
                      .Should().NotThrowAsync();
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_ReturnsOrderItemResponseWithNewGuid()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            SetupSuccessfulAdd(request);

            // Act
            var result = await _sut.AddOrderItem(request);

            // Assert – service should assign a new non-empty OrderItemID
            result.Should().NotBeNull();
            result.OrderItemID.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_MapsFieldsCorrectlyToOrderItemResponse()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedOrderItem(request);
            var product = CreateProductWithStock(request.ProductID!.Value, 100);

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request.ProductID))
                .ReturnsAsync(product);

            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .ReturnsAsync(addedItem);

            // Act
            var result = await _sut.AddOrderItem(request);

            // Assert – all fields from the added entity are reflected in the response
            result.OrderID.Should().Be(addedItem.OrderID);
            result.ProductID.Should().Be(addedItem.ProductID);
            result.Quantity.Should().Be(addedItem.Quantity);
            result.PriceAtSale.Should().Be(addedItem.PriceAtSale);
            result.ProductName.Should().Be(addedItem.Product?.ProductName);
            result.SKU.Should().Be(addedItem.Product?.SKU);
            result.ProductUnitPrice.Should().Be(addedItem.Product?.UnitPrice);
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_CallsRepositoryExactlyOnce()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            SetupSuccessfulAdd(request);

            // Act
            await _sut.AddOrderItem(request);

            // Assert
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Once);
        }

        [Fact]
        public async Task AddOrderItem_ValidRequest_PassesItemWithNewGuidToRepository()
        {
            // Arrange
            var request = CreateValidItemAddRequest();
            var addedItem = CreateAddedOrderItem(request);
            var product = CreateProductWithStock(request.ProductID!.Value, 100);
            OrderItem? capturedItem = null;

            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request.ProductID))
                .ReturnsAsync(product);

            // Capture the item passed to the repository to verify its contents
            _orderItemsRepositoryMock
                .Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .Callback<OrderItem>(i => capturedItem = i)
                .ReturnsAsync(addedItem);

            // Act
            await _sut.AddOrderItem(request);

            // Assert – the item passed to the repository should have a new non-empty ID
            capturedItem.Should().NotBeNull();
            capturedItem!.OrderItemID.Should().NotBeEmpty();
            capturedItem.OrderID.Should().Be(request.OrderID);
            capturedItem.ProductID.Should().Be(request.ProductID);
            capturedItem.Quantity.Should().Be(request.Quantity);
        }

        #endregion

        #region AddMultipleOrderItems

        [Fact]
        public async Task AddMultipleOrderItems_NullList_ThrowsArgumentNullException()
        {
            // Act & Assert – null list should throw before any repository call
            await _sut.Invoking(s => s.AddMultipleOrderItems(null))
                      .Should().ThrowAsync<ArgumentNullException>();

            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddMultipleOrderItems_EmptyList_ReturnsEmptyList()
        {
            // Arrange – empty list is valid, should return empty response list
            // Act
            var result = await _sut.AddMultipleOrderItems(new List<OrderItemAddRequest>());

            // Assert
            result.Should().BeEmpty();
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Never);
        }

        [Fact]
        public async Task AddMultipleOrderItems_ListWithInvalidItem_ThrowsArgumentException()
        {
            // Arrange – one item in the list fails validation
            var validItem = CreateValidItemAddRequest();
            var invalidItem = CreateValidItemAddRequest(r => r.ProductID = null);

            // Act & Assert – validation failure on any item should throw before processing
            await _sut.Invoking(s => s.AddMultipleOrderItems(
                    new List<OrderItemAddRequest> { validItem, invalidItem }))
                      .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddMultipleOrderItems_OneItemWithInsufficientStock_ThrowsInsufficientProductStockException()
        {
            // Arrange – second item exceeds available stock
            var request1 = CreateValidItemAddRequest(r => r.Quantity = 3);
            var request2 = CreateValidItemAddRequest(r => r.Quantity = 50);

            SetupSuccessfulAdd(request1, stockQuantity: 100);

            var productWithLowStock = CreateProductWithStock(request2.ProductID!.Value, stockQuantity: 5);
            _productsGetterServiceMock
                .Setup(s => s.GetProductById(request2.ProductID))
                .ReturnsAsync(productWithLowStock);

            // Act & Assert
            await _sut.Invoking(s => s.AddMultipleOrderItems(
                    new List<OrderItemAddRequest> { request1, request2 }))
                      .Should().ThrowAsync<InsufficientProductStockException>();
        }

        [Fact]
        public async Task AddMultipleOrderItems_ValidList_ReturnsAllAddedItemResponses()
        {
            // Arrange
            var requests = new List<OrderItemAddRequest>
        {
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
        };

            // Each request gets its own product and added entity
            foreach (var request in requests)
            {
                SetupSuccessfulAdd(request);
            }

            // Act
            var result = await _sut.AddMultipleOrderItems(requests);

            // Assert – all items processed and returned
            result.Should().HaveCount(3);
            result.Should().AllBeOfType<OrderItemResponse>();
        }

        [Fact]
        public async Task AddMultipleOrderItems_ValidList_CallsRepositoryOncePerItem()
        {
            // Arrange
            var requests = new List<OrderItemAddRequest>
        {
            CreateValidItemAddRequest(),
            CreateValidItemAddRequest(),
        };

            foreach (var request in requests)
            {
                SetupSuccessfulAdd(request);
            }

            // Act
            await _sut.AddMultipleOrderItems(requests);

            // Assert – repository called once per item in the list
            _orderItemsRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Exactly(2));
        }

        #endregion
    }
}
