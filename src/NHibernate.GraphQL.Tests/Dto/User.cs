using System;

namespace NHibernate.GraphQL.Tests.Dto
{
    public class User
    {
        public virtual long Id { get; set; }

        public virtual string Login { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailIsConfirmed { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        public virtual DateTime? RemovedAt { get; set; }
    }
}
