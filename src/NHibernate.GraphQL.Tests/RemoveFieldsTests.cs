using System.Linq;
using NUnit.Framework;
using NHibernate.GraphQL.Tests.TestData;
using NHibernate.GraphQL.Tests.Dto;

namespace NHibernate.GraphQL.Tests
{
    public class RemoveFieldsTests: DatabaseFixture
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
            var session = CreateSession();

            new UsersSet().CreateData(session);

            return session.Query<User>();
        }

        [Test]
        public void ShouldRemoveUnspecifiedFieldsFromQuery()
        {
            var session = CreateSession();

            new UsersSet().CreateData(session);

            IQueryable<ExposedUser> query = session.Query<User>()
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
    }
}
