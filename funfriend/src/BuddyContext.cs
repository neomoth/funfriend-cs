using funfriend.buddies;
using funfriend.glsl;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace funfriend;

public enum Behavior
{
	Wander,
	Follow,
	Stay,
}

public class BuddyContext : WindowContext
{
	private const float ChatterTime = 3;
	private const float StayStillAfterHeld = 1;
	private const float WanderTime = 4;
	private const int FollowDist = 120;
	
	private BuddyRenderer Renderer { get; }
	private Buddy Buddy { get; }
	private float ChatterTimer { get; set; } = 1;
	private int ChatterIndex { get; set; } = 0;
	private List<string> ChatterArray { get; set; }

	private bool Held { get; set; } = false;
	private Vec2 HeldAt { get; set; } = Vec2.Zero;
	private Vec2 StartedHoldingAt { get; set; } = Vec2.Zero;
	private float HeldTimer { get; set; } = 0;
	private bool WaitingForStablePos { get; set; } = false;
	private float WanderTimer { get; set; } = WanderTime;
	
	private Vec2 StaticPos { get; set; }
	
	private Vec2 EasingFrom { get; set; } = Vec2.Zero;
	private Vec2 EasingTo { get; set; } = Vec2.Zero;
	private double EasingDur { get; set; } = 0.0;
	private double EasingT { get; set; } = 0.0;

	private readonly ILogger<BuddyContext> logger;

	private GLFWCallbacks.MouseButtonCallback mcb;
	private GLFWCallbacks.KeyCallback kcb;
	
	public BuddyContext(Buddy buddy) : base(
		title: $"??_{buddy.Name()}__??", width: WindowSize().Xi(), height: WindowSize().Yi(), false)
	{
		Buddy = buddy;

		Renderer = new BuddyRenderer(Buddy);
		
		logger = Logger.GetLogger<BuddyContext>();

		unsafe
		{
			GLFW.MakeContextCurrent(Window);

			mcb = (window, button, action, mods) =>
			{
				if (button is MouseButton.Left)
				{
					if (action is InputAction.Press)
					{
						Buddy.TalkSound();
						EasingDur = 0;
						Held = true;
						GLFW.GetWindowPos(window, out var wx, out var wy);
						GLFW.GetCursorPos(window, out var cx, out var cy);
						HeldAt = new Vec2((float)cx, (float)cy);

						if (HeldTimer <= 0)
						{
							StartedHoldingAt = new Vec2(wx, wy);
							logger.LogInformation($"Began holding at {StartedHoldingAt}");
						}

						HeldTimer = StayStillAfterHeld;
						GLFW.SetCursor(Window, GLFW.CreateStandardCursor(CursorShape.ResizeAll));
					}
					else if (action is InputAction.Release)
					{
						Held = false;
						GLFW.SetCursor(Window, GLFW.CreateStandardCursor(CursorShape.Arrow));
					}
				}
			};
			
			GLFW.SetMouseButtonCallback(Window, mcb);

			kcb = (window, key, code, action, mods) =>
			{
				if (action is InputAction.Press && key is Keys.Escape)
				{
					Close();
					CleanUp();
				}
			};
			
			GLFW.SetKeyCallback(Window, kcb);

			var primaryMonitor = GLFW.GetPrimaryMonitor();
			GLFW.GetMonitorPos(primaryMonitor, out var x, out var y);
			var videoMode = GLFW.GetVideoMode(primaryMonitor);
			int randomX = x + (int)(videoMode->Width * Random.Shared.NextDouble());
			int randomY = y + (int)(videoMode->Height * Random.Shared.NextDouble());
			GLFW.SetWindowPos(Window, randomX, randomY);
			StaticPos = new Vec2(randomX, randomY);

			var chatter = Buddy.Dialog(DialogType.Chatter);
			ChatterArray = chatter.ElementAt(Random.Shared.Next(0, chatter.Count));
			
			if (GLFW.GetError(out var errorMessage) is not ErrorCode.NoError)
			{
				Console.WriteLine("GLFW Error: " + errorMessage);
			}

		}
	}
	
	public static Vec2 WindowSize()
	{
		var size = (int)ConfigManager.Config["window"]["funfriend_size"];
		return new Vec2((int)Math.Floor(size * 1.3));
	}

	private bool Moving => EasingDur != 0 && EasingT <= EasingDur;
	private bool Speaking => ChatterIndex < ChatterArray.Count(x => true);

	private Behavior CurrentBehavior => Speaking ? Behavior.Follow : Behavior.Wander;

	private void UpdateWander(float delta)
	{
		if (Moving)
		{
			EasingT += delta;
			float a = Ease.InOutSine((float)(EasingT / EasingDur));
			Vec2 newPos = EasingFrom * (1 - a) + EasingTo * a;
			unsafe { GLFW.SetWindowPos(Window, (int)newPos.X, (int)newPos.Y); }

			WanderTimer = WanderTime;
		}
		else
		{
			switch (CurrentBehavior)
			{
				case Behavior.Wander:
					WanderTimer -= delta;
					if (WanderTimer <= 0)
					{
						Vec2 offset = Vec2.Rand(40);
						Goto(StaticPos + offset, 4f, false);
					}
					break;
				case Behavior.Follow:
					if (!Moving)
					{
						unsafe
						{
							GLFW.GetCursorPos(Window, out var xDist, out var yDist);
							GLFW.GetWindowPos(Window, out var xTarget, out var yTarget);

							if (Math.Abs(xDist) > FollowDist) xTarget += (int)xDist - FollowDist * Math.Sign(xDist);
							if (Math.Abs(yDist) > FollowDist) yTarget += (int)yDist - FollowDist * Math.Sign(yDist);

							Vec2 targetPos = new Vec2(xTarget, yTarget);
							Goto(targetPos, 1);
						}
					}
					break;
			}
		}
	}

	private void UpdatePos(float delta)
	{
		if (Held)
		{
			unsafe
			{
				GLFW.GetCursorPos(Window, out var cx, out var cy);
				GLFW.GetWindowPos(Window, out var wx, out var wy);
				StaticPos = new Vec2(wx, wy) - HeldAt + new Vec2((float)cx, (float)cy);
				GLFW.SetWindowPos(Window, StaticPos.Xi(), StaticPos.Yi());
				logger.LogInformation($"Held at: {HeldAt}, static pos: {StaticPos}, cursor pos: {cx}, {cy}");
			}
		}
		else
		{
			HeldTimer -= delta;
			if (HeldTimer <= 0)
			{
				UpdateWander(delta);

				if (WaitingForStablePos)
				{
					WaitingForStablePos = false;

					if (!Speaking)
					{
						Say(StaticPos.Distance(StartedHoldingAt) > 50
							? Buddy.Dialog(DialogType.Moved)
							: Buddy.Dialog(DialogType.Touched));
					}
				}
			}
			else WaitingForStablePos = true;
		}
	}
	
	public void Goto(Vec2 destination, float duration, bool setStatic = true)
	{
		if (setStatic) StaticPos = destination;

		unsafe
		{
			GLFW.GetWindowPos(Window, out var wx, out var wy);

			EasingFrom = new Vec2(wx, wy);
			EasingTo = destination;
			EasingT = 0;
			EasingDur = duration;
		}
	}

	public override void Update(float delta)
	{
		ChatterTimer -= delta;
		if (ChatterTimer <= 0)
		{
			ChatterTimer += ChatterTime;

			if (ChatterArray?.ElementAtOrDefault(ChatterIndex) is not null) Say(ChatterArray[ChatterIndex]);
			ChatterIndex++;
		}

		UpdatePos(delta);
		
		Renderer.Render(delta, WindowSize().Xi(), WindowSize().Yi());
	}

	public void Say(string text)
	{
		foreach (var context in FunFriend.GetContexts())
		{
			if (context is ChatterContext chatter)
			{
				chatter.Bump();
			}
		}

		unsafe
		{
			GLFW.GetWindowPos(Window, out var wx, out var wy);
			GLFW.GetWindowSize(Window, out var ww, out var wh);
			FunFriend.QueueAddContext(new ChatterContext(text, Buddy.Font(), new Vec2(wx+ww/2,wy-20), 6, this));
		}
		Buddy.TalkSound();
	}

	public void Say(List<string> text)
    {
        ChatterArray = text;
        ChatterTimer = 0;
        ChatterIndex = 0;
    }

	public void Say(List<List<string>> text)
	{
		if(text.Count>0) Say(text[new Random().Next(text.Count)]);
	}

	public override void CleanUp()
	{
		Renderer.CleanUp();
	}
}