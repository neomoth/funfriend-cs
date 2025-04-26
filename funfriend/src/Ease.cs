namespace funfriend.glsl;

public static class Ease
{
	public static float InSine(float x) => 1 - MathF.Cos(x * MathF.PI * 0.5f);
	
	public static float OutSine(float x) => MathF.Sin(x * MathF.PI * 0.5f);
	
	public static float InOutSine(float x) => 0.5f - 0.5f * MathF.Cos(x * MathF.PI);
}