using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.GraphQL.Tests.Dto.Mapping
{
    public class RoleMapping: ClassMapping<Role>
    {
        public RoleMapping()
        {
            Id(x => x.Id, map =>
            {
                map.Generator(Generators.Assigned);
            });
            Property(x => x.Code, m => m.NotNullable(true));
            Property(x => x.Name, m => m.NotNullable(false));

            Set(x => x.Users, map =>
            {
                map.Table("UserRoleJunction");
                map.Key(k => k.Column("RoleId"));
                map.Cascade(Cascade.All);
            }, map => map.ManyToMany(p => p.Column("UserId")));
        }
    }
}
