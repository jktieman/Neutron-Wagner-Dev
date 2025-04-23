using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NeutronData.Models;
using NeutronData.Repositories;

namespace NeutronDataTests.Repositories
{
    [TestClass]
    [TestSubject(typeof(NeutronData.Repositories.GenericRepository<History>))]
    public class GenericRepositoryTest
    {

        [TestMethod]
        public void InsertAndFindByKey_ShouldReturnInsertedEntity()
        {
            // Arrange
            var mockRepository = new Mock<GenericRepository<History>>();
            var testEntity = new History
            {
                Id = 1,
                ActionCode = 1,
                TransmitDateTime = DateTime.Now
            };
            // Act
            mockRepository.Object.Insert(testEntity);
            var result = mockRepository.Object.FindByKeyAsync(1);
            // Assert
            Assert.IsNotNull(result, "The result should not be null.");
            Assert.AreEqual(testEntity, result, "The result should match the inserted entity.");
        }

    }
}