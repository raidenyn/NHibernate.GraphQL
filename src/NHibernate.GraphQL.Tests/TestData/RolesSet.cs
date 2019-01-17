using System;
using NHibernate.GraphQL.Tests.Dto;

namespace NHibernate.GraphQL.Tests.TestData
{
    internal class RolesSet
    {
        public RolesSet(UsersSet userSet)
        {
            Roles = new[]
        {
            new Role
                {
                    Id = 101,
                    Code = "Role1",
                    Name = "TestRole1",
                    Users = {
                        userSet.Users[0],
                        userSet.Users[1],
                        userSet.Users[2],
                    },
                },
                new Role
                {
                    Id = 102,
                    Code = "Role2",
                    Name = "TestRole2",
                    Users = {
                        userSet.Users[3],
                        userSet.Users[4],
                        userSet.Users[5],
                    },
                },
                new Role
                {
                    Id = 103,
                    Code = "Role3",
                    Name = "TestRole3",
                    Users = {
                        userSet.Users[1],
                        userSet.Users[3],
                        userSet.Users[5],
                    },
                }
            };
        }

        public readonly Role[] Roles;

        public void CreateData(ISession session)
        {
            foreach (var role in Roles)
            {
                session.Save(role);
            }

            session.Flush();
        }
    }
}
