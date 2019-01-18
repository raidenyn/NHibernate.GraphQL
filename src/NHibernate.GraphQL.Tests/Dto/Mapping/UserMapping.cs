using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.GraphQL.Tests.Dto.Mapping
{
    public class UserMapping: ClassMapping<User>
    {
        public UserMapping()
        {
            Id(x => x.Id, map =>
            {
                map.Generator(Generators.Assigned);
            });
            Property(x => x.Login, m => m.NotNullable(true));
            Property(x => x.FirstName, m => m.NotNullable(false));
            Property(x => x.LastName, m => m.NotNullable(false));
            Property(x => x.Email, m => m.NotNullable(true));
            Property(x => x.EmailIsConfirmed, m => m.NotNullable(true));
            Property(x => x.CreatedAt, m => m.NotNullable(true));
            Property(x => x.RemovedAt, m => m.NotNullable(false));
            Property(x => x.PasswordHash, m =>
            {
                m.NotNullable(false);
                m.Lazy(true);
            });

            Set(x => x.Roles, map =>
            {
                map.Table("UserRoleJunction");
                map.Key(k => k.Column("UserId"));
                map.Lazy(CollectionLazy.Lazy);
            }, map => map.ManyToMany(p => p.Column("RoleId")));

            ManyToOne(x => x.Address, map =>
            {
                map.Column("AddressId");
                map.Cascade(Cascade.All);
            });
        }
    }
}
