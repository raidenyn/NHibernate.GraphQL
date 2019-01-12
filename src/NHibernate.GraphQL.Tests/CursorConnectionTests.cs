using System.Linq;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    public class CursorConnectionTests
    {
        class SourceItem
        {
            public string SourceFirst { get; set; }

            public int SourceSecond { get; set; }

            public bool SourceThird { get; set; }
        }

        class TargetItem
        {
            public string TargetFirst { get; set; }

            public int TargetSecond { get; set; }

            public bool TargetThird { get; set; }
        }

        class Request : ICursorRequest
        {
            public int? First { get; set; }

            public string After { get; set; }
        }

        [Test]
        public void ShouldPageQueryFields()
        {
            var data = new[]
            {
                new SourceItem
                {
                    SourceFirst = "1",
                    SourceSecond = 1,
                    SourceThird = true
                },
                new SourceItem
                {
                    SourceFirst = "2",
                    SourceSecond = 2,
                    SourceThird = true
                },
                new SourceItem
                {
                    SourceFirst = "3",
                    SourceSecond = 3,
                    SourceThird = true
                },
                new SourceItem
                {
                    SourceFirst = "4",
                    SourceSecond = 4,
                    SourceThird = true
                }
            };

            IQueryable<SourceItem> query = data.AsQueryable();

            var connection = query.ToConnection(
                item => item.SourceFirst,
                (order, after) => order.CompareTo(after) > 0,
                item => new TargetItem
                {
                    TargetFirst = item.SourceFirst,
                    TargetSecond = item.SourceSecond,
                    TargetThird = item.SourceThird,
                },
                new Request
                {
                    First = 2,
                    After = "'1'"
                });

            Assert.AreEqual(2, connection.Edges.Count);
        }
    }
}
