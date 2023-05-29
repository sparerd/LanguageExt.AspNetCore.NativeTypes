using LanguageExt.AspNetCore.NativeTypes.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.AspNetCore.NativeTypes.Tests.TestPrelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests.Controllers;

[Route($"{CONTROLLER_SOURCE}/[controller]")]
[ApiController]
public class SeqController : ControllerBase
{
	[HttpPost("body")]
	public IActionResult FromBody([FromBody] Seq<int> num) => Ok(num.Sum());

	[HttpPost("body/complex")]
	public IActionResult FromBodyComplex([FromBody] RecordWithSeqs value) => Ok(
		new RecordWithSeqs(
			First: value.First.Map(i => Increment(i)),
			Second: value.Second.Map(i => Increment(i))));

	[HttpGet("query")]
	public IActionResult FromQuery([FromQuery] Seq<int> num) => Ok(num.Sum());

	[HttpGet("route/{num?}")]
	public IActionResult FromRoute([FromRoute] Seq<int> num) => Ok(num.Sum());

	[HttpGet("header")]
	public IActionResult FromHeader([FromHeader(Name = "X-OPT-NUM")] Seq<int> num) => Ok(num.Sum());

	[HttpPost("form")]
	public IActionResult FromForm([FromForm] Seq<int> num) => Ok(num.Sum());
}