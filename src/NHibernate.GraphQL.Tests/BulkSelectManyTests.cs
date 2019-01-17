using System.Linq;
using System.Threading.Tasks;
using NHibernate.GraphQL.Tests.Dto;
using NHibernate.GraphQL.Tests.TestData;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    class BulkSelectManyTests : DatabaseFixture
    {
        private IQueryable<User> GetUserQuery()
        {
            var usersSet = new UsersSet().CreateData(Session);
            new RolesSet(usersSet).CreateData(Session);

            return Session.Query<User>();
        }

        [Test]
        public void ShouldSelectObjectByIdSplitedByBatches()
        {
            var rolesId = new long[] { 101, 103 };

            var result = Execute(rolesId);

            Assert.AreEqual(2, result.Lookup.Count, "Count of roles is wrong");
            Assert.AreEqual(6, result.Lookup.Sum(users => users.Count()), "Count of users is wrong");
            Assert.AreEqual(2, result.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public async Task ShouldSelectObjectByIdSplitedByBatchesAsync()
        {
            var rolesId = new long[] { 101, 103 };

            var result = await ExecuteAsync(rolesId);

            Assert.AreEqual(2, result.Lookup.Count, "Count of roles is wrong");
            Assert.AreEqual(6, result.Lookup.Sum(users => users.Count()), "Count of users is wrong");
            Assert.AreEqual(2, result.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public void ShouldReturnEmptyListEmptyIdRequest()
        {
            var rolesId = new long[] { };

            var result = Execute(rolesId);

            Assert.AreEqual(0, result.Lookup.Count, "Count of users is wrong");
            Assert.AreEqual(0, result.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public async Task ShouldReturnEmptyListEmptyIdRequestAsync()
        {
            var rolesId = new long[] { };

            var result = await ExecuteAsync(rolesId);

            Assert.AreEqual(0, result.Lookup.Count, "Count of users is wrong");
            Assert.AreEqual(0, result.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public void ShouldNotReturnNotExistingItems()
        {
            long[] rolesId = new long[] { 101, 102, 103, 104 };

            var result = Execute(rolesId);

            Assert.AreEqual(3, result.Lookup.Count, "Count of roles is wrong");
            Assert.AreEqual(9, result.Lookup.Sum(users => users.Count()), "Count of users is wrong");
            Assert.AreEqual(4, result.BatchCount, "BatchCount is wrong");
        }

        [Test]
        public async Task ShouldNotReturnNotExistingItemsAsync()
        {
            long[] rolesId = new long[] { 101, 102, 103, 104 };

            var result = await ExecuteAsync(rolesId);

            Assert.AreEqual(3, result.Lookup.Count, "Count of roles is wrong");
            Assert.AreEqual(9, result.Lookup.Sum(users => users.Count()), "Count of users is wrong");
            Assert.AreEqual(4, result.BatchCount, "BatchCounter is wrong");
        }

        private ExecutionResult Execute(long[] rolesId)
        {
            int batchCounter = 0;

            ILookup<long, ExposedUser> lookup = GetUserQuery().BulkSelectMany(
                filter: (users, ids) => {
                    batchCounter++;
                    return from user in users
                           from role in user.Roles
                           where ids.Contains(role.Id)
                           select new { user, role };
                },
                select: junction => new ExposedUser
                {
                    Login = junction.user.Login,
                    Email = junction.user.Email,
                    FirstName = junction.user.FirstName,
                    Name = junction.user.FirstName + junction.user.LastName
                },
                getResultId: junction => junction.user.Id,
                getJuncedId: junction => junction.role.Id,
                ids: rolesId,
                batchSize: 1);

            return new ExecutionResult
            {
                Lookup = lookup,
                BatchCount = batchCounter,
            };
        }

        private async Task<ExecutionResult> ExecuteAsync(long[] rolesId)
        {
            int batchCounter = 0;

            ILookup<long, ExposedUser> lookup = await GetUserQuery().BulkSelectManyAsync(
                filter: (users, ids) => {
                    batchCounter++;
                    return from user in users
                           from role in user.Roles
                           where ids.Contains(role.Id)
                           select new { user, role };
                },
                select: junction => new ExposedUser
                {
                    Login = junction.user.Login,
                    Email = junction.user.Email,
                    FirstName = junction.user.FirstName,
                    Name = junction.user.FirstName + junction.user.LastName
                },
                getResultId: junction => junction.user.Id,
                getJuncedId: junction => junction.role.Id,
                ids: rolesId,
                batchSize: 1);

            return new ExecutionResult {
                Lookup = lookup,
                BatchCount = batchCounter,
            };
        }

        private struct ExecutionResult
        {
            public ILookup<long, ExposedUser> Lookup { get; set; }

            public int BatchCount { get; set; }
        }

        private class ExposedUser
        {
            public string Login { get; set; }

            public string Email { get; set; }

            public string FirstName { get; set; }

            public string Name { get; set; }
        }
    }
}
