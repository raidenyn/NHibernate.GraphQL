using System;
using System.Linq;
using NHibernate.GraphQL.Tests.Dto;
using NHibernate.GraphQL.Tests.TestData;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    public class CursorConnectionTests: DatabaseFixture
    {
        class ExposedUser
        {
            public string Login { get; set; }

            public string Name { get; set; }

            public string FirstName { get; set; }

            public string Email { get; set; }
        }

        class Request : ICursorRequest
        {
            public int? First { get; set; }

            public Cursor After { get; set; }
        }

        private IQueryable<User> GetUserQuery()
        {
            new UsersSet().CreateData(Session);

            return Session.Query<User>();
        }

        [Test]
        public void ShouldPageQueryWithSimpleOrder()
        {
            var connection = GetUserQuery().ToConnection(
                user => user.Id,
                (userId, after) => userId > after,
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    First = 2,
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(1)
                });

            Assert.AreEqual(2, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsTrue(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsTrue(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }

        [Test]
        public void ShouldPageQueryWithExplicitAcsendingOrder()
        {
            var connection = GetUserQuery().ToConnection(
                user => SortBy.Ascending(user.Id),
                (userId, after) => userId > after,
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    First = 2,
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(1)
                });

            Assert.AreEqual(2, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsTrue(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsTrue(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }

        [Test]
        public void ShouldPageQueryWithExplicitDesendingOrder()
        {
            var connection = GetUserQuery().ToConnection(
                user => SortBy.Descending(user.Id),
                (userId, after) => userId < after,
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    First = 2,
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(5)
                });

            Assert.AreEqual(2, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsTrue(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsTrue(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }

        [Test]
        public void ShouldPageQueryFieldsByComplexOrder()
        {
            var connection = GetUserQuery().ToConnection(
                user => new { user.Id, user.CreatedAt },
                (order, after) => order.CreatedAt > after.CreatedAt || (order.CreatedAt == after.CreatedAt && order.Id > after.Id),
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    First = 2,
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(new {
                        Id = 1,
                        CreatedAt = new DateTime(2019, 1, 2)
                    })
                });

            Assert.AreEqual(2, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsTrue(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsTrue(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }

        [Test]
        public void ShouldPageQueryFieldsByComplexOrderWithExplicitDesending()
        {
            var connection = GetUserQuery().ToConnection(
                user => new {
                    CreatedAt = SortBy.Ascending(user.CreatedAt),
                    Id = SortBy.Descending(user.Id)
                },
                (order, after) => (order.CreatedAt > after.CreatedAt) || (order.CreatedAt == after.CreatedAt && order.Id < after.Id),
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    First = 2,
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(new
                    {
                        CreatedAt = new DateTime(2019, 1, 2),
                        Id = 4,
                    })
                });

            Assert.AreEqual(2, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsTrue(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsTrue(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }

        [Test]
        public void ShouldPageQueryOnlyByCount()
        {
            var connection = GetUserQuery().ToConnection(
                user => user.Id,
                (order, after) => order > after,
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    First = 2
                });

            Assert.AreEqual(2, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsTrue(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsFalse(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }

        [Test]
        public void ShouldPageQueryOnlyByCursor()
        {
            var connection = GetUserQuery().ToConnection(
                user => user.Id,
                (order, after) => order > after,
                user => new ExposedUser
                {
                    Login = user.Login,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Name = user.FirstName + user.LastName
                },
                new Request
                {
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(3)
                });

            Assert.AreEqual(3, connection.Edges.Count, "Count of edges is wrong");
            Assert.AreEqual(6, connection.TotalCount, "TotalCount is wrong");
            Assert.IsFalse(connection.PageInfo.HasNextPage, "HasNextPage is wrong");
            Assert.IsTrue(connection.PageInfo.HasPreviousPage, "HasPreviousPage is wrong");
        }
    }
}
