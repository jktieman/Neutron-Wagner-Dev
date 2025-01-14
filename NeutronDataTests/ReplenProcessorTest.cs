using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReplenService;
using System;
using System.Collections.Generic;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using AlliedLogger;
using OrderStatus = NeutronCore.Enums.OrderStatus;

namespace NeutronDataTests
{
    [TestClass]
    public class ReplenProcessorTest
    {
        private Mock<GenericRepository<Order>> _mockRepoOrder;
        private Mock<GenericRepository<OrderDetail>> _mockRepoOrderDetail;
        private Mock<GenericRepository<ReplenOrder>> _mockRepoReplenOrder;
        private Mock<GenericRepository<ReplenOrderDetail>> _mockRepoReplenOrderDetail;
        private Mock<GenericRepository<ItemDefinition>> _mockRepoItemDefinitions;
        private Mock<ReplenRepository> _mockReplenRepository;
        private Mock<IDynamicLogger> _mockLogger;
        private ReplenProcessor _replenProcessor;

        //[TestInitialize]
        //public void TestInitialize()
        //{
        //    _mockRepoOrder = new Mock<GenericRepository<Order>>();
        //    _mockRepoOrderDetail = new Mock<GenericRepository<OrderDetail>>();
        //    _mockRepoReplenOrder = new Mock<GenericRepository<ReplenOrder>>();
        //    _mockRepoReplenOrderDetail = new Mock<GenericRepository<ReplenOrderDetail>>();
        //    _mockRepoItemDefinitions = new Mock<GenericRepository<ItemDefinition>>();
        //    _mockReplenRepository = new Mock<ReplenRepository>();
        //    _mockLogger = new Mock<IDynamicLogger>();
        //    _replenProcessor = new ReplenProcessor(
        //        _mockRepoOrder.Object,
        //        _mockRepoOrderDetail.Object,
        //        _mockRepoReplenOrder.Object,
        //        _mockRepoReplenOrderDetail.Object,
        //        _mockRepoItemDefinitions.Object,
        //        _mockReplenRepository.Object,
        //        _mockLogger.Object);
        //}

        //[TestMethod]
        //public void TestProcessReplenishment_QuantityNeededLessThanZero_ReturnsNull()
        //{
        //    var replenishment = new Replenishment { QuantityNeeded = -1 };
        //    var result = _replenProcessor.ProcessReplenishment(replenishment);
        //    Assert.IsNull(result);
        //}

        //[TestMethod]
        //public void TestProcessReplenishment_QuantityNeededZero_ExistingReplenishmentPickRemoved()
        //{
        //    //var replenishment = new Replenishment { QuantityNeeded = 0, Item = "item1" };
        //    //var existingReplenishmentPick = new Order
        //    //    { Id = 1, Ord1 = "item1", Ord2 = "REPLEN", OrderStatusId = (int)OrderStatus.Available };
        //    //_mockRepoOrder.Setup(r => r.FindBy(It.IsAny<Func<Order, bool>>()))
        //    //    .Returns(new List<Order> { existingReplenishmentPick });
        //    //_replenProcessor.ProcessReplenishment(replenishment);
        //    //_mockRepoOrder.Verify(r => r.Delete(existingReplenishmentPick.Id), Times.Once);
        //}

        // Add more test methods here for each method in the ReplenProcessor class
    }
}