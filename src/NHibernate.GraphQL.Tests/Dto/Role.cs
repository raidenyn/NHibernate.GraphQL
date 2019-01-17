using System;
using System.Collections.Generic;

namespace NHibernate.GraphQL.Tests.Dto
{
    public class Role
    {
        public virtual long Id { get; set; }

        public virtual string Code { get; set; }

        public virtual string Name { get; set; }

        private ISet<User> _user;
        public virtual ISet<User> Users
        {
            get { return _user ?? (_user = new HashSet<User>()); }
            protected set { _user = value; }
        }
    }
}
