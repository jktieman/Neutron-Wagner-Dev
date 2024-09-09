using System;
using System.Linq.Expressions;
using AlliedLogger;
using System.Threading.Tasks;
using Moq;
using Neutron.Forms;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using NUnit.Framework;
using System.Data.Entity;

namespace NeutronTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task IsInInventory_ReturnsTrue_WhenLocationExistsInInventory()
        {
            // Arrange
            var mockLogger = new Mock<IDynamicLogger>();
            var mockContext = new Mock<NeutronDb>();
            var mockDbSet = new Mock<DbSet<Inventory>>();
            mockContext.Setup(c => c.Set<Inventory>()).Returns(mockDbSet.Object);
            var repository = new LocationsRepository();
            var locationId = 1;
            var inventory = new Inventory { LocationId = locationId };
            mockDbSet.Setup(d => d.FindAsync(locationId)).ReturnsAsync(inventory);
            // Act
            var result = await repository.IsInInventory(locationId);
            // Assert
            Assert.IsTrue(result);
        }
    }
}