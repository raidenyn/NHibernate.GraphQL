# Tools for GraphQL API implementation based on NHibernate

[![Build status](https://ci.appveyor.com/api/projects/status/a3ym70967jj8m6ne/branch/master?svg=true)](https://ci.appveyor.com/project/raidenyn/nhibernate-graphql/branch/master)

## Main methods

### Select data optimization

GraphQL allows you to optimize the amount of data being sent between server and client sides, but it would be also great to optimize traffic between application and database.

The method `OptimizeQuery` keeps only explicitly requested fields in a LINQ query:

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

// Now the query retrieves only Login and Name, but Email and FirstName are skipped.
var result = query.ToList(); 
```

> Be aware that this technique decreases traffic between database and application,
> but increases the variety of sql queries. This may have an impact on the database's
> ability to cache query plans.

### Creating Connections

GraphQL [suggests](https://graphql.org/learn/pagination/) using Relay-style pagination based on cursors technique.

`ToConnection` and `ToConnectionAsync` methods allow you to create Connection objects easily based on your LINQ query.

``` cs
var query = session.Query<User>()
                   .Where(user => Email.Contains("@gmail.com"));

Connection<ExposedUser> connection = query.ToConnection(
    orderBy: user => user.Id,                        // Order by field
    select: user => new ExposedUser                 // Select statement
    {
        Login = user.Login,
        Email = user.Email,
        FirstName = user.FirstName,
        Name = user.FirstName + user.LastName
    },
    request: new Request
    {
        First = 2,       // Count of items in the current result (optional)
        After = "MQ=="   // Cursor value from previous response (optional)
    });

```

#### Creating connections with sorting by multiple fields

Sometimes sorting by ID is not enough. For example, we may want to sort our users by `CreatedAt` field. But the field is not unique and different users can receive the same value for the field. In this case we cannot create a unique cursor based only on `CreatedAt` field. So we have to add some unique field to the sorting to guarantee that our sorting is always consistent. ID is good for this.

We can use an anonymous type to sort our objects by multiple fields:

``` cs
var query = session.Query<User>()
                   .Where(user => Email.Contains("@gmail.com"));

Connection<ExposedUser> connection = query.ToConnection(
    orderBy: user => { user.CreatedAt, user.Id },   // Order by fields (order of fields is important!)
    select: user => new ExposedUser                 // Select statement
    {
        Login = user.Login,
        Email = user.Email,
        FirstName = user.FirstName,
        Name = user.FirstName + user.LastName
    },
    request: new Request
    {
        First = 2,       // Count of items in the current result (optional)
    });

```

### Define sorting direction

OK, now we can sort users by creation time, but what if we want to do it in descending order? We can use `SortBy.Descending` method for this purpose and separately set sorting direction for each field:

``` cs
var query = session.Query<User>()
                   .Where(user => Email.Contains("@gmail.com"));

Connection<ExposedUser> connection = query.ToConnection(
    orderBy: user => {
        SortBy.Descending(user.CreatedAt),  // Now `CreatedAt` will be sorted in descending order
        user.Id
    },
    select: user => new ExposedUser            // Select statement
    {
        Login = user.Login,
        Email = user.Email,
        FirstName = user.FirstName,
        Name = user.FirstName + user.LastName
    },
    request: new Request
    {
        First = 2,       // Count of items in the current result (optional)
    });

```

### Selecting objects from aggregated ids with Many To One relationship

DataLoader is a nice way to optimize the count of select queries to your database, and it solves the N+1 select problem. But sometimes the amount of requested data might be too large to pass it to your database in one SQL request, since most SQL clients can only support so much data in a single request.

Methods `BulkSelect` and `BulkSelectAsync` allow you to request data from the database and put the result to `IDictionary`:

``` cs

// incoming parameters from DataLoader
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
            House = item.address.House,
            Text = item.address.Street + " " + item.address.House + ", " + item.address.Zip
        }

    // expression to extract id value from the junction
    getId: item => item.address.Id,

    // required ids
    ids: userIds);

```

### Selecting objects from aggregated ids with Many To Many relationship

Sometimes we need to select aggregated objects with many-to-many relationship. Methods `BulkSelectMany` and `BulkSelectManyAsync` help you with it and create `ILookup` result:

``` cs

// incoming parameters from DataLoader
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
    getResultId: junction => junction.role.Id,

    // expression to extract id value of joined object from the junction
    getJoinedId: junction => junction.user.Id,

    // required ids
    ids: userIds);

```

## Roadmap

- [ ] Add how it works explanations and documentation
- [ ] Create more test cases
- [ ] Support bidirectional connections
- [ ] Add cache for expression generation
- [ ] Support splitting by id strategy in bulk selection
- [ ] Support connections with bulk selection
