# LanguageExt.AspNetCore.NativeTypes
Extensions and middleware for ASP.NET Core that allow you to use LanguageExt types directly in controllers.
The 3 main goals of this library are to enable:
- Model binding support for LanguageExt types in controller methods
- Configurable JSON serialization support for LanguageExt types
    - Currently, there is only support for `System.Text.Json`. 
    - `NewtonsoftJson` support will be added eventually.
- Transparent `Monad -> IActionResult` mapping to allow defining controller methods in terms of monadic chains
    ```csharp
    [HttpGet]
    public Eff<string> Hello() => SuccessEff("world");
    ```

**This library is in early development and should not be used in a production setting.**

**I've made this mostly to satisfy my own curiosity in how far we can push non-functional code to the edges in C#/AspNetCore.**

# Installation
**TODO: Add nuget package install note once published**

Call `AddLanguageExtTypeSupport()` after `AddMvc()` within your `ConfigureServices()` method to enable support for LanguageExt types.

```csharp
var builder = WebApplication.CreateBuilder();

builder.WebHost
    .ConfigureServices((IServiceCollection services) =>
    {
        services
            .AddMvc()
            .AddLanguageExtTypeSupport() // <-- Add this
            ;
    });
```

`AddLanguageExtTypeSupport` also supports an overload which takes a `LanguageExtAspNetCoreOptions` instance. This allows you to configure certain aspects of the model binding system.

```csharp
var builder = WebApplication.CreateBuilder();

builder.WebHost
    .ConfigureServices((IServiceCollection services) =>
    {
        services
            .AddMvc()
            .AddLanguageExtTypeSupport(new LanguageExtAspNetCoreOptions {
                // override default options
            })
            ;
    });
```

# Inbound Type Binding
The following types are supported in controllers as input arguments.

## Option<T>
The following binding sources are supported for `Option<T>` and the implicit `OptionNone` types.

```csharp
[HttpPost("body")]
public IActionResult FromBody([FromBody] Option<int> num);

// public record RecordWithOptions(Option<int> Num1, Option<int> Num2);
[HttpPost("body/complex")]
public IActionResult FromBodyComplex([FromBody] RecordWithOptions value);

[HttpGet("query")]
public IActionResult FromQuery([FromQuery] Option<int> num);

[HttpGet("route/{num?}")]
public IActionResult FromRoute([FromRoute] Option<int> num);

[HttpGet("header")]
public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Option<int> num);

[HttpPost("form")]
public IActionResult FromForm([FromForm] Option<int> num);
```

## Collection types
The following LanguageExt collection types are supported:
- `Seq<T>`
- `Lst<T>`

*Note: The examples below show only `Seq<T>`, but all supported collection types are interchangable.*

```csharp
[HttpPost("body")]
IActionResult FromBody([FromBody] Seq<int> num);

// public record RecordWithSeqs(Seq<int> First, Seq<int> Second);
[HttpPost("body/complex")]
public IActionResult FromBodyComplex([FromBody] RecordWithSeqs value);

[HttpGet("query")]
public IActionResult FromQuery([FromQuery] Seq<int> num);

[HttpGet("route/{num?}")]
public IActionResult FromRoute([FromRoute] Seq<int> num);

[HttpGet("header")]
public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Seq<int> num);

[HttpPost("form")]
public IActionResult FromForm([FromForm] Seq<int> num);
```

# JSON Serialization
In addition to allowing LangExt types in controller arguments, these types can be returned in JSON return types with configurable representations.
## Option
There are 2 serialization strategies available for `Option<T>`. You may choose the strategy when calling `AddLanguageExtTypeSupport`. All serialization strategies are capable of deserializing `Option<T>` from any form. These strategies only affect how serializing `Option<T> -> string` is performed.

### AsNullable
This strategy uses JSON `null` to represent `None` and transparently converts `Some` values to their normal JSON representations.
This is the default serialization strategy for `Option<T>`.

Set with: 
```csharp
new LanguageExtAspNetCoreOptions { 
    OptionSerializationStrategy = OptionSerializationStrategy.AsNullable 
}
```

Examples:
| Input object              |   JSON 
|--------------             |---------
| `Some(7)`                 | `7`
| `new {count: Some(7)}`    | `{count: 7}`
| `None`                    | `null`
| `new {count: None}`       | `{count: null}`

*Attribution: Initial work for this converter came from [this GitHub comment](https://github.com/louthy/language-ext/discussions/1132#discussioncomment-3860993)*

### AsArray
This strategy treats the `Option<T>` as a special case array of `0..1` values. This will always produce a JSON array, but will only write a value into the array when in the `Some` state.

Set with: 
```csharp
new LanguageExtAspNetCoreOptions { 
    OptionSerializationStrategy = OptionSerializationStrategy.AsArray 
}
```

Examples:
| Input object              |   JSON 
|--------------             |---------
| `Some(7)`                 | `[7]`
| `new {count: Some(7)}`    | `{count: [7]}`
| `None`                    | `[]`
| `new {count: None}`       | `{count: []}`

*Attribution: The majority of the work for this converter came from [this GitHub comment](https://github.com/louthy/language-ext/discussions/1132#discussioncomment-3860993)*

## Collection types
All collection types (`Seq<T>`, etc) that we support from LanguageExt are serialized as arrays, as you would expect.

There are currently no serialization options for these types.

# Controller Return Types

## Eff/Aff
Returning an `Eff<T>` or `Aff<T>` from a controller method will cause the effect to be run. Successes will return 200 and return the value in the response. Error cases will return a 500 error by default. This can be customized when registering support for effectful endpoints.

```csharp
[HttpGet("{id:guid}")]
public Aff<User> FindUser(Guid id) => _db.FindUserAff(user => user.Id == id);
// => Success(User)  -> 200 Ok(User)
// => Err            -> 500 InternalServerError -or- customized error handler
```

Your effect can also return action results. This gives you more direct control over the mapping of internal values to results, while still remaining in the monad.
```csharp
[HttpGet("{id:guid}")]
public Aff<IActionResult> FindUser(Guid id) => 
    _db.FindUserAff(user => user.Id == id)
       .Map(user => new OkObjectResult(user) as IActionResult)
    | @catch(Errors.UserAccountSoftDeleted, _ => SuccessAff<IActionResult>(new NotFoundResult()));
// => Success(User)                 -> 200 Ok(User)
// => Err(UserAccountSoftDeleted)   -> 404 NotFound
// => Err                           -> 500 InternalServerError -or- customized error handler
```

To customize global effect result handling, pass a `Func<Fin<IActionResult>, IActionResult>` delegate into your call to `AddEffAffEndpointSupport`. This delegate is always called when the effect completes. Your delegate should handle both success and failure cases for the effect.

The following example overrides failure handling when the error matches a predefined application error.
```csharp
Error UserNotFound = Error.New(4009, "User not found");

Func<Fin<IActionResult>, IActionResult> unwrapper = fin => 
    fin.Match(
        identity, 
        error => error switch
        {
            {} err when err == UserNotFound => new NotFoundResult(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
        });

services
    .AddMvc()
    .AddLanguageExtTypeSupport()
    .AddEffAffEndpointSupport(unwrapper)
```

## Option
Returning an `Option<T>` from a controller method will convert `Some` to a 200 and return the value in the response and `None` becomes 404NotFound.

```csharp
[HttpGet("{id:guid}")]
public Option<User> FindUser(Guid id) => _db.Find(user => user.Id == id);
// => Some(User) -> 200 Ok(User)
// => None       -> 404 NotFound
```


# Minimal APIs
Please note that [minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview) endpoint definitions are currently **not supported**.

The minimal API system uses a completely different model binding approach that does not allow for injectable deserializers. This means that custom deserialization/binding must be [defined directly on the type](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-7.0#custom-binding). This would require defining these parsing/binding rules directly in the LanguageExt type definitions, which does not seem like a worthwhile exercise.

Luckily, it seems this limitation is a pain point for others and a feature request to add injectable binding is being tracked [in this github issue](https://github.com/dotnet/aspnetcore/issues/35489).