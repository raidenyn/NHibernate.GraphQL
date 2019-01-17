# Tools for GraphQL API implementation based on NHibernate

## Select data optimization
GraphQL allows optimize amount of data sending between server and client sides, but it would be also great to optimize traffic between application and database.

Method `OptimizeQuery` keeps only explicitly requested fields in LINQ query:

``` cs

var query = session.Query<User>()
                   .Select(user => new ExposedUser
                        {
                            Login = user.Login,
                            Name = user.FirstName + user.LastName,
                            Email = user.Email,
                            FirstName = user.FirstName
                        });

query = query.OptimizeQuery(new []
    {
        nameof(ExposedUser.Login),
        nameof(ExposedUser.Name)
    });

// Now the query retrieves only Login and Name, but Email and FirstName are skip.
var result = query.ToList(); 
```

> Be aware that the technic decreases traffic between database and application,
> but increase sql query variety. So it may impact to database query plan cash size.

## Creating Connections
GraphQL [suggests](https://graphql.org/learn/pagination/) to use Relay style pagination based on cursors technic.

`ToConnection` and `ToConnectionAsync` methods allow create Connection objects easily based on your LINQ query.

``` cs
var query = session.Query<User>()
                   .Where(user => Email.Contains("@gmail.com"));

Connection<ExposedUser> connection = query.ToConnection(
    user => user.Id,                        // Order by field
    (userId, after) => userId > after,      // Condition for applying cursor filtration
    user => new ExposedUser                 // Select statement
    {
        Login = user.Login,
        Email = user.Email,
        FirstName = user.FirstName,
        Name = user.FirstName + user.LastName
    },
    new Request
    {
        First = 2,       // Count of items in the current result (optional)
        After = "MQ=="   // Cursor value from previous response (optional)
    });

```

## Selecting objects from aggregated ids with Many To One relationship
DataLoader is nice way to optimize count of select queries to your database and it solves N+1 select problem. But sometimes amount of requested data might be too large to pass it to you database in one SQL request as most of DB has limitation for its size.

Methods `BulkSelect` and `BulkSelectAsync` allow to request data from database and put result to `IDictionary`:
``` cs

// income parameters from DataLoader
IReadOnlyCollection<long> userIds = new [] { 1, 2, 3, 4, 5 };

IDictionary<int, ExposedUserAddress> dictionary = 
  session.Query<UserAddress>().BulkSelect(
    // filtering query to retrieve a junction of address and user
    filter: (addresses, userIdsPart) =>
        from address in addresses 
        from user in address.Users
        where userIdsPart.Contains(user.Id)
        select new { address, user },

    // expression to extract result object from the junction
    select: item => new ExposedUserAddress
        {
            Zip = item.address.Zip,
            Street = item.address.Street,
            Home = item.address.Home,
            Text = item.address.Street + item.address.Home + item.address.Zip
        }

    // expression to extract id value from the junction
    getId: item => item.address.Id,

    // required ids
    ids: userIds,

    // maximum ids in one select query to database
    batchSize: 1000);

```

## Selecting objects from aggregated ids with Many To Many relationship
Sometimes we need to select aggregated objects with many-to-many relationship. Methods `BulkSelectMany` and `BulkSelectManyAsync` help you with it and create `ILookup` result:
``` cs

// income parameters from DataLoader
IReadOnlyCollection<long> userIds = new [] { 1, 2, 3, 4, 5 };

ILookup<int, ExposedUserRole> lookup = 
  session.Query<UserRole>().BulkSelectMany(
    // filtering query to retrieve a junction of address and user
    filter: (roles, userIdsPart) =>
        from role in roles 
        from user in role.Users
        where userIdsPart.Contains(user.Id)
        select new { role, user },

    // expression to extract result object from the junction
    select: junction => new ExposedUserRole
        {
            Code = junction.role.Code,
            Name = junction.role.Name,
        }

    // expression to extract id value from the junction
    getResultId: item => item.role.Id,

    // expression to extract id value of joined object from the junction
    getJoinedId: junction => junction.user.Id,

    // required ids
    ids: userIds,

    // maximum ids in one select query to database
    batchSize: 1000);

```

# Roadmap
- [ ] Support bidirectional connections
- [ ] Creates more test cases
- [ ] Add cache for expression generation
- [ ] Add how it works explanations
