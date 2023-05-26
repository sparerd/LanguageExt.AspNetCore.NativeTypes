using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{CONTROLLER_SOURCE}/[controller]")]
[ApiController]
public class SeqController
{
	private readonly LanguageExtAspNetCoreOptions _options;

	public SeqController(LanguageExtAspNetCoreOptions options)
	{
		_options = options;
	}

	[HttpPost("body")]
	public IActionResult FromBody([FromBody] Seq<int> num) => JsonResult(num.Sum());

	[HttpPost("body/complex")]
	public IActionResult FromBodyComplex([FromBody] RecordWithSeqs value) => JsonResult(
		new RecordWithSeqs(
			First: value.First.Map(i => Increment(i)),
			Second: value.Second.Map(i => Increment(i))));

	[HttpGet("query")]
	public IActionResult FromQuery([FromQuery] Seq<int> num) => JsonResult(num.Sum());

	[HttpGet("route/{num?}")]
	public IActionResult FromRoute([FromRoute] Seq<int> num) => JsonResult(num.Sum());

	[HttpGet("header")]
	public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Seq<int> num) => JsonResult(num.Sum());

	[HttpPost("form")]
	public IActionResult FromForm([FromForm] Seq<int> num) => JsonResult(num.Sum());

	private IActionResult JsonResult(object value) => TestPrelude.JsonResult(_options, value);
}