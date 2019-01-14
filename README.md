# Tools for GraphQL API implementation based on NHibernate

## Select data optimization
GraphQL allows optimize amount of data sending between server and client sides, but it would be also great to optimize traffic between application and database level also.

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

`ToConnection` and `ToConnectionAsync` methods allow create the connection easily based on your LINQ query.

```
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