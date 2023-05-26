# LanguageExt.AspNetCore.NativeTypes
Extensions and middleware for ASP.NET Core that allow you to use LanguageExt types directly in controllers.

**This library is in early development and should not be used in a production setting.**
**I've made this mostly to satisfy my own curiosity in how far we can push non-functional code to the edges in C#/AspNetCore.**

# Installation
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

# Inbound Type Binding
The following types are supported in controllers as input arguments.

## Option<T>
The following binding sources are supported for `Option<T>` and the implicit `OptionNone` types.

```csharp
[HttpPost("body")]
public IActionResult FromBody([FromBody] Option<int> num) => new OkResult();

// public record RecordWithOptions(Option<int> Num1, Option<int> Num2);
[HttpPost("body/complex")]
public IActionResult FromBodyComplex([FromBody] RecordWithOptions value) => new OkResult();

[HttpGet("query")]
public IActionResult FromQuery([FromQuery] Option<int> num) => new OkResult();

[HttpGet("route/{num?}")]
public IActionResult FromRoute([FromRoute] Option<int> num) => new OkResult();

[HttpGet("header")]
public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Option<int> num) => new OkResult();

[HttpPost("form")]
public IActionResult FromForm([FromForm] Option<int> num) => new OkResult();
```
