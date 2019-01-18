using System;
using System.Collections.Generic;
using System.Text;

namespace NHibernate.GraphQL.Tests.Dto
{
    public class Address
    {
        public virtual int Id { get; set; }

        public virtual string Street { get; set; }

        public virtual string House { get; set; }

        public virtual string Zip { get; set; }

        private ISet<User> _user;
        public virtual ISet<User> Users
        {
            get { return _user ?? (_user = new HashSet<User>()); }
            protected set { _user = value; }
        }
    }
}
