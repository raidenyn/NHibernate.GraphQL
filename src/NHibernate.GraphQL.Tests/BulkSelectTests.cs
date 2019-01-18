using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.GraphQL.Tests.Dto;
using NHibernate.GraphQL.Tests.TestData;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    class BulkSelectTests: DatabaseFixture
    {
        [Test]
        public void ShouldSelectObjectByIdSplitedByBatches()
        {
            var data = Execute(userIds: new int[] { 1, 2, 3, 4, 5 });

            Assert.AreEqual(5, data.Dictionary.Count, "Count of adresses is wrong");
            Assert.AreEqual(3, data.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public async Task ShouldSelectObjectByIdSplitedByBatchesAsync()
        {
            var data = await ExecuteAsync(userIds: new int[] { 1, 2, 3, 4, 5 });

            Assert.AreEqual(5, data.Dictionary.Count, "Count of adresses is wrong");
            Assert.AreEqual(3, data.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public void ShouldReturnEmptyListEmptyIdRequest()
        {
            var data = Execute(userIds: new int[] { });

            Assert.AreEqual(0, data.Dictionary.Count, "Count of adresses is wrong");
            Assert.AreEqual(0, data.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public async Task ShouldReturnEmptyListEmptyIdRequestAsync()
        {
            var data = await ExecuteAsync(userIds: new int[] { });

            Assert.AreEqual(0, data.Dictionary.Count, "Count of adresses is wrong");
            Assert.AreEqual(0, data.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public void ShouldNotReturnNotExistingItems()
        {
            var data = Execute(userIds: new int[] { 1, 3, 6, 9, 12 });

            Assert.AreEqual(3, data.Dictionary.Count, "Count of adresses is wrong");
            Assert.AreEqual(3, data.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public async Task ShouldNotReturnNotExistingItemsAsync()
        {
            var data = await ExecuteAsync(userIds: new int[] { 1, 3, 6, 9, 12 });

            Assert.AreEqual(3, data.Dictionary.Count, "Count of adresses is wrong");
            Assert.AreEqual(3, data.BatchCount, "BatchCount is wrong");
        }

        private IQueryable<Address> GetAddressQuery()
        {
            new AddressesSet(new UsersSet().CreateData(Session)).CreateData(Session);

            return Session.Query<Address>();
        }

        private LoadResult Execute(int[] userIds)
        {
            int batchCounter = 0;

            IDictionary<int, ExposedAddress> dic = GetAddressQuery().BulkSelect(
                filter: (addresses, ids) =>
                {
                    batchCounter++;
                    return from address in addresses
                           from user in address.Users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user,
                               address
                           };
                },
                select: (junction) => new ExposedAddress
                {
                    Zip = junction.address.Zip,
                    Street = junction.address.Street,
                    House = junction.address.House,
                    Text = junction.address.Street + " " + junction.address.House + ", " + junction.address.Zip,
                },
                getJoinedId: (junction) => junction.user.Id,
                ids: userIds,
                batchSize: 2);

            return new LoadResult
            {
                Dictionary = dic,
                BatchCount = batchCounter,
            };
        }

        private async Task<LoadResult> ExecuteAsync(int[] userIds)
        {
            int batchCounter = 0;

            IDictionary<int, ExposedAddress> dic = await GetAddressQuery().BulkSelectAsync(
                filter: (addresses, ids) =>
                {
                    batchCounter++;
                    return from address in addresses
                           from user in address.Users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user,
                               address
                           };
                },
                select: (junction) => new ExposedAddress
                {
                    Zip = junction.address.Zip,
                    Street = junction.address.Street,
                    House = junction.address.House,
                    Text = junction.address.Street + " " + junction.address.House + ", " + junction.address.Zip,
                },
                getJoinedId: (junction) => junction.user.Id,
                ids: userIds,
                batchSize: 2);

            return new LoadResult
            {
                Dictionary = dic,
                BatchCount = batchCounter,
            };
        }

        private class ExposedAddress
        {
            public string Zip { get; set; }

            public string Street { get; set; }

            public string House { get; set; }

            public string Text { get; set; }
        }

        private struct LoadResult
        {
            public IDictionary<int, ExposedAddress> Dictionary { get; set; }

            public int BatchCount { get; set; }
        }
    }
}
