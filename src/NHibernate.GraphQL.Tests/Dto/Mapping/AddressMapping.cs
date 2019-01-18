using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.GraphQL.Tests.Dto.Mapping
{
    public class AddressMapping: ClassMapping<Address>
    {
        public AddressMapping()
        {
            Id(x => x.Id, map =>
            {
                map.Generator(Generators.Assigned);
            });
            Property(x => x.House, m => m.NotNullable(true));
            Property(x => x.Street, m => m.NotNullable(false));
            Property(x => x.Zip, m => m.NotNullable(false));

            Set(x => x.Users, map =>
            {
                map.Table("UserAddressJunction");
                map.Key(k => k.Column("AddressId"));
                map.Cascade(Cascade.All);
            }, map => map.OneToMany());
        }
    }
}
