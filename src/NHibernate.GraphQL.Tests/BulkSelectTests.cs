using System.Linq;
using System.Threading.Tasks;
using NHibernate.GraphQL.Tests.Dto;
using NHibernate.GraphQL.Tests.TestData;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    class BulkSelectTests: DatabaseFixture
    {
        private IQueryable<User> GetUserQuery()
        {
            new UsersSet().CreateData(Session);

            return Session.Query<User>();
        }

        [Test]
        public void ShouldSelectObjectByIdSplitedByBatches()
        {
            int batchCounter = 0;

            var items = GetUserQuery().BulkSelect(
                select: (users, ids) =>
                    {
                        batchCounter++;
                        return from user in users
                               where ids.Contains(user.Id)
                               select new
                               {
                                   user.Login,
                                   user.Email,
                                   user.FirstName,
                                   Name = user.FirstName + user.LastName
                               };
                    },
                ids: new long[] { 1, 2, 3, 4, 5 },
                batchSize: 2).ToList();

            Assert.AreEqual(5, items.Count, "Count of users is wrong");
            Assert.AreEqual(3, batchCounter, "BatchCounter is wrong");
        }

        [Test]
        public async Task ShouldSelectObjectByIdSplitedByBatchesAsync()
        {
            int batchCounter = 0;

            var items = (await GetUserQuery().BulkSelectAsync(
                select: (users, ids) =>
                {
                    batchCounter++;
                    return from user in users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user.Login,
                               user.Email,
                               user.FirstName,
                               Name = user.FirstName + user.LastName
                           };
                },
                ids: new long[] { 1, 2, 3, 4, 5 },
                batchSize: 2)).ToList();

            Assert.AreEqual(5, items.Count, "Count of users is wrong");
            Assert.AreEqual(3, batchCounter, "BatchCounter is wrong");
        }

        [Test]
        public void ShouldSelectObjectIntoDictionaryByBatches()
        {
            int batchCounter = 0;

            var dictionary = GetUserQuery().BulkSelect(
                select: (users, ids) =>
                {
                    batchCounter++;
                    return from user in users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user.Id,
                               user.Login,
                               user.Email,
                               user.FirstName,
                               Name = user.FirstName + user.LastName
                           };
                },
                getId: user => user.Id,
                ids: new long[] { 1, 2, 3, 4, 5 },
                batchSize: 2);

            Assert.AreEqual(5, dictionary.Count, "Count of users is wrong");
            Assert.AreEqual(3, batchCounter, "BatchCounter is wrong");
        }

        [Test]
        public async Task ShouldSelectObjectIntoDictionaryByBatchesAsync()
        {
            int batchCounter = 0;

            var dictionary = await GetUserQuery().BulkSelectAsync(
                select: (users, ids) =>
                {
                    batchCounter++;
                    return from user in users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user.Id,
                               user.Login,
                               user.Email,
                               user.FirstName,
                               Name = user.FirstName + user.LastName
                           };
                },
                getId: user => user.Id,
                ids: new long[] { 1, 2, 3, 4, 5 },
                batchSize: 2);

            Assert.AreEqual(5, dictionary.Count, "Count of users is wrong");
            Assert.AreEqual(3, batchCounter, "BatchCounter is wrong");
        }

        [Test]
        public void ShouldReturnEmptyListEmptyIdRequest()
        {
            int batchCounter = 0;

            var items = GetUserQuery().BulkSelect(
                select: (users, ids) =>
                {
                    batchCounter++;
                    return from user in users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user.Login,
                               user.Email,
                               user.FirstName,
                               Name = user.FirstName + user.LastName
                           };
                },
                ids: new long[] { },
                batchSize: 2).ToList();

            Assert.AreEqual(0, items.Count, "Count of users is wrong");
            Assert.AreEqual(0, batchCounter, "BatchCounter is wrong");
        }

        [Test]
        public void ShouldNotReturnNotExistingItems()
        {
            int batchCounter = 0;

            var items = GetUserQuery().BulkSelect(
                select: (users, ids) =>
                {
                    batchCounter++;
                    return from user in users
                           where ids.Contains(user.Id)
                           select new
                           {
                               user.Login,
                               user.Email,
                               user.FirstName,
                               Name = user.FirstName + user.LastName
                           };
                },
                ids: new long[] { 1, 3, 6, 9, 12 },
                batchSize: 2).ToList();

            Assert.AreEqual(3, items.Count, "Count of users is wrong");
            Assert.AreEqual(3, batchCounter, "BatchCounter is wrong");
        }
    }
}
