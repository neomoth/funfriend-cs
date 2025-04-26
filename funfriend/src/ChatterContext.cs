using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace funfriend;

public class ChatterContext : WindowContext
{
	private TextRenderer renderer;
	private WindowContext? parent;
	private Vec2 parentRelativePos;
	private float timer;

	private static readonly Vec2 DefaultWindowSize = new Vec2(256, 32);
	private const float DefaultDuration = 6f;
	private const int Padding = 10;

	public Vec2 WindowSize { get; }

	public ChatterContext(string text, string font, Vec2 pos, float duration = DefaultDuration,
		WindowContext? parent = null)
		: base("??__FUNFRIEND__?? > CHATTER", (int)DefaultWindowSize.X + Padding * 2,
			(int)DefaultWindowSize.Y + Padding * 2, false)
	{
		var fntPath = Path.Combine(FunFriend.AssetsDirectory, font);
		var sheet = FontManager.ParseBm(File.ReadAllText(fntPath+".fnt"));
		var positionData = FontManager.PositionText(text, sheet);
		
		WindowSize = new Vec2(positionData.Width, positionData.Height) + Padding * 2;

		unsafe
		{
			GLFW.MakeContextCurrent(Window);
			GL.LoadBindings(context);
		}

		timer = duration;
		renderer = new TextRenderer(text, fntPath, sheet, (int)WindowSize.X, (int)WindowSize.Y);
		
		var p = (pos - WindowSize / 2).XyI();

		this.parent = parent;
		
		unsafe
		{
			GLFW.SetWindowPos(Window, (int)p.X, (int)p.Y);
			if (parent is null) return;
			GLFW.GetWindowPos(parent.Window, out var x, out var y);
			GLFW.GetWindowSize(parent.Window, out var w, out var h);
			parentRelativePos = pos - (new Vec2(x, y) + new Vec2(w, h) / 2);
		}
	}

	public void UpdatePosition()
	{
		if (parent is not null)
		{
			unsafe
			{
				GLFW.GetWindowPos(parent.Window, out var x, out var y);
				GLFW.SetWindowPos(Window, x, y);
			}
		}
	}

	private void Render(float delta)
	{
		GL.Viewport(0, 0, (int)WindowSize.X, (int)WindowSize.Y);

		GL.ClearColor(0f, 0f, 0f, 0f);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		renderer.Render(delta);
	}

	public override void Update(float delta)
	{
		timer -= delta;
		if (timer <= 0f)
		{
			Close();
			CleanUp();
		}

		UpdatePosition();
		Render(delta);
	}

	public void Bump()
	{
		parentRelativePos.Y -= WindowSize.Y + 10;
		UpdatePosition();
	}

	public override void CleanUp()
	{
		renderer.CleanUp();
	}
}