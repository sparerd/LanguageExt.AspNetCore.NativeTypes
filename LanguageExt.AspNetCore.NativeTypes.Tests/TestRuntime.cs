using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace LanguageExt.AspNetCore.NativeTypes.Tests;

public readonly struct TestRuntime : HasCancel<TestRuntime>, HasSubsystem<TestRuntime>
{
	public readonly int Val1;
	public readonly int SubVal;

	public TestRuntime(int val1, int subVal)
	{
		Val1 = val1;
		SubVal = subVal;
		CancellationTokenSource = new CancellationTokenSource();
	}

	public CancellationTokenSource CancellationTokenSource { get; }
	public CancellationToken CancellationToken => CancellationTokenSource.Token;
	public TestRuntime LocalCancel => new(Val1, SubVal);

	public Eff<TestRuntime, SubsystemIO> SubsystemEff =>
		Eff<TestRuntime, SubsystemIO>(rt => new TestSubsystem(rt.SubVal));
}

public interface HasSubsystem<RT> 
	where RT : struct, HasSubsystem<RT>
{
	Eff<RT, SubsystemIO> SubsystemEff { get; }
}

public interface SubsystemIO
{
	int SubFunction();
}

public readonly struct TestSubsystem : SubsystemIO
{
	public readonly int SubVal;

	public TestSubsystem(int subVal)
	{
		SubVal = subVal;
	}

	public int SubFunction() => SubVal * 2;
}

public static class Subsystem<RT> where RT : struct, HasSubsystem<RT>
{
	public static Eff<RT, int> CallSubFunction() => 
		default(RT).SubsystemEff.Map(io => io.SubFunction());
}