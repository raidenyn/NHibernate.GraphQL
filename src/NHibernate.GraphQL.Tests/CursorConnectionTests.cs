using System;
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

            public Cursor After { get; set; }
        }

        class OrderStructure : IComparable<OrderStructure>
        {
            public string O1 { get; set; }

            public int O2 { get; set; }

            /// <summary>
            /// We have to make it comparable to sort the objects in Linq queries
            /// </summary>
            public int CompareTo(OrderStructure other)
            {
                return other.O1.CompareTo(other.O1) + other.O2.CompareTo(other.O2);
            }
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
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format("1")
                });

            Assert.AreEqual(2, connection.Edges.Count);
        }

        [Test]
        public void ShouldPageQueryFieldsByComplexOrder()
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
                item => new OrderStructure { O1 = item.SourceFirst, O2 = item.SourceSecond },
                (order, after) => order.O1.CompareTo(after.O1) > 0 && order.O2 > after.O2,
                item => new TargetItem
                {
                    TargetFirst = item.SourceFirst,
                    TargetSecond = item.SourceSecond,
                    TargetThird = item.SourceThird,
                },
                new Request
                {
                    First = 2,
                    After = ConnectionQuerySettings.Default.CursorFormatter.Format(new {
                        O1 = "1",
                        O2 = 1
                    })
                }); 

            Assert.AreEqual(2, connection.Edges.Count);
        }
    }
}
