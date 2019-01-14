using System;
using NHibernate.GraphQL.Tests.Dto;

namespace NHibernate.GraphQL.Tests.TestData
{
    internal class UsersSet
    {
        public void CreateData(ISession session)
        {
            session.Save(new User
            {
                Id = 1,
                Login = "User1",
                Email = "user1@test.org",
                EmailIsConfirmed = true,
                FirstName = "Test",
                LastName = "User1",
                PasswordHash = "1",
                CreatedAt = new DateTime(2019, 1, 1),
            });
            session.Save(new User
            {
                Id = 2,
                Login = "User2",
                Email = "user2@test.org",
                EmailIsConfirmed = true,
                FirstName = "Another Test",
                LastName = "User2",
                PasswordHash = "1234567890",
                CreatedAt = new DateTime(2019, 1, 2),
            });
            session.Save(new User
            {
                Id = 3,
                Login = "User3",
                Email = "user3@test.org",
                EmailIsConfirmed = true,
                FirstName = "Another Test",
                LastName = "User3",
                PasswordHash = "1234567890",
                CreatedAt = new DateTime(2019, 1, 3),
            });
            session.Save(new User
            {
                Id = 4,
                Login = "User4",
                Email = "user4@test.org",
                EmailIsConfirmed = true,
                FirstName = "Test4",
                LastName = "User4",
                PasswordHash = "1234567890",
                CreatedAt = new DateTime(2019, 1, 2),
            });
            session.Save(new User
            {
                Id = 5,
                Login = "User5",
                Email = "user5@test.org",
                EmailIsConfirmed = true,
                FirstName = "Test5",
                LastName = "User5",
                PasswordHash = "1234567890",
                CreatedAt = new DateTime(2019, 1, 3),
            });
            session.Save(new User
            {
                Id = 6,
                Login = "User6",
                Email = "user6@test.org",
                EmailIsConfirmed = true,
                FirstName = "Test6",
                LastName = "User6",
                PasswordHash = "1234567890",
                CreatedAt = new DateTime(2019, 1, 2),
            });

            session.Flush();
        }
    }
}
