using NHibernate.GraphQL.Tests.Dto;

namespace NHibernate.GraphQL.Tests.TestData
{
    internal class AddressesSet
    {
        public AddressesSet(UsersSet userSet)
        {
            Addresses = new[]
            {
                new Address
                {
                    Id = 201,
                    Zip = "123456",
                    Street = "TestStreet",
                    House = "1",
                    Users = {
                        userSet.Users[0],
                    },
                },
                new Address
                {
                    Id = 202,
                    Zip = "123456",
                    Street = "TestStreet",
                    House = "2",
                    Users = {
                        userSet.Users[1],
                    },
                },
                new Address
                {
                    Id = 203,
                    Zip = "123456",
                    Street = "TestStreet",
                    House = "3",
                    Users = {
                        userSet.Users[2],
                    },
                },
                new Address
                {
                    Id = 204,
                    Zip = "123456",
                    Street = "TestStreet",
                    House = "4",
                    Users = {
                        userSet.Users[3],
                    },
                },
                new Address
                {
                    Id = 205,
                    Zip = "123456",
                    Street = "TestStreet",
                    House = "5",
                    Users = {
                        userSet.Users[4],
                    },
                },
                new Address
                {
                    Id = 206,
                    Zip = "123456",
                    Street = "TestStreet",
                    House = "6",
                    Users = {
                        userSet.Users[5],
                    },
                },
            };
        }

        public readonly Address[] Addresses;

        public void CreateData(ISession session)
        {
            foreach (var address in Addresses)
            {
                session.Save(address);
            }

            session.Flush();
        }
    }
}
