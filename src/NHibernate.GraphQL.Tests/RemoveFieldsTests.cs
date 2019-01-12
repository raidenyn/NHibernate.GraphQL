using System.Linq;
using NUnit.Framework;

namespace NHibernate.GraphQL.Tests
{
    public class RemoveFieldsTests
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

        [Test]
        public void ShouldRemoveUnspecifiedFieldsFromQuery()
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
                }
            };

            IQueryable<TargetItem> query = data.AsQueryable()
                .Select(item => new TargetItem
                {
                    TargetFirst = item.SourceFirst,
                    TargetSecond = item.SourceSecond,
                    TargetThird = item.SourceThird,
                });

            query = query.OptimizeQuery(new []
            {
                nameof(TargetItem.TargetFirst)
            });

            var result = query.ToList();

            Assert.AreEqual(3, result.Count);
            foreach (var item in result)
            {
                Assert.AreNotEqual(default(string), item.TargetFirst);
                Assert.AreEqual(default(int), item.TargetSecond);
                Assert.AreEqual(default(bool), item.TargetThird);
            }
        }
    }
}
