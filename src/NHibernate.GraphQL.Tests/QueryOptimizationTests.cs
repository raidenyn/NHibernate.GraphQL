using System.Linq;
using NUnit.Framework;
using NHibernate.GraphQL.Tests.TestData;
using NHibernate.GraphQL.Tests.Dto;
using System;

namespace NHibernate.GraphQL.Tests
{
    public class QueryOptimizationTests : DatabaseFixture
    {
        class ExposedUser
        {
            public string Login { get; set; }

            public string Name { get; set; }

            public string FirstName { get; set; }

            public string Email { get; set; }
        }

        private IQueryable<User> GetUserQuery()
        {
            new UsersSet().CreateData(Session);

            return Session.Query<User>();
        }

        [Test]
        public void ShouldRemoveUnspecifiedFieldsFromQuery()
        {
            IQueryable<ExposedUser> query = GetUserQuery()
                .Select(user => new ExposedUser
                {
                    Login = user.Login,
                    Name = user.FirstName + user.LastName,
                    Email = user.Email,
                    FirstName = user.FirstName
                });

            query = query.OptimizeQuery(new []
            {
                nameof(ExposedUser.Login),
                nameof(ExposedUser.Name)
            });

            var result = query.ToList();

            Assert.Greater(result.Count, 0);
            foreach (var user in result)
            {
                Assert.AreNotEqual(default(string), user.Login);
                Assert.AreNotEqual(default(string), user.Name);
                Assert.AreEqual(default(string), user.Email);
                Assert.AreEqual(default(string), user.FirstName);
            }
        }

        [Test]
        public void ShouldRiseErrorOnEmptyList()
        {
            IQueryable<ExposedUser> query = GetUserQuery()
                .Select(user => new ExposedUser
                {
                    Login = user.Login,
                    Name = user.FirstName + user.LastName,
                    Email = user.Email,
                    FirstName = user.FirstName
                });

            Assert.Throws<ArgumentException>(() => query.OptimizeQuery(new string[0]));
        }

        [Test]
        public void ShouldRiseErrorForNotExistingFields()
        {
            IQueryable<ExposedUser> query = GetUserQuery()
                .Select(user => new ExposedUser
                {
                    Login = user.Login,
                    Name = user.FirstName + user.LastName,
                    FirstName = user.FirstName
                });

            Assert.Throws<ArgumentException>(() => query.OptimizeQuery(new string[] { nameof(ExposedUser.Email) }));
        }

        [Test]
        public void ShouldRiseErrorForNotExistingMembers()
        {
            IQueryable<ExposedUser> query = GetUserQuery()
                .Select(user => new ExposedUser
                {
                    Login = user.Login,
                    Name = user.FirstName + user.LastName,
                    FirstName = user.FirstName
                });

            Assert.Throws<ArgumentException>(() => query.OptimizeQuery(new [] { typeof(ExposedUser).GetMember(nameof(ExposedUser.Email))[0] }));
        }
    }
}
