using funfriend.buddies;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
namespace funfriend;

public static class FunFriend
{
	public static readonly string AssetsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
	
	public static string Version = "0.1.0";
	private static readonly List<WindowContext> Contexts = [];
	private static readonly List<WindowContext> ContextsToAdd = [];
	private static readonly List<WindowContext> ContextsToRemove = [];

	private static bool ShouldClose() => Contexts.Count <= 0;

	private static void InitContexts()
	{
		AddContext(new BuddyContext(new CatfriendBuddy()));
	}

	public static void QueueAddContext(WindowContext context) => ContextsToAdd.Add(context);
	public static void QueueRemoveContext(WindowContext context) => ContextsToRemove.Add(context);
	
	public static void AddContext(WindowContext context) => Contexts.Add(context);

	public static List<WindowContext> GetContexts() => Contexts;
	
	public static void Main(string[] args)
	{
		// GLFW.InitHint(InitHintInt.WaylandLibDecor, 0);
		GLFW.InitHint(InitHintANGLEPlatformType.ANGLEPlatformType, ANGLEPlatformType.OpenGL);
		if(Environment.OSVersion.Platform is PlatformID.Unix) GLFW.InitHint(InitHintPlatform.Platform, Platform.X11);
		
		Console.WriteLine("GDK_BACKEND: " + Environment.GetEnvironmentVariable("GDK_BACKEND"));
		Console.WriteLine("LIBGL_ALWAYS_INDIRECT: " + Environment.GetEnvironmentVariable("LIBGL_ALWAYS_INDIRECT"));
		GLFW.Init();
		ConfigManager.Init();
		Logger.Init();
		SoundManager.Init();

		var logger = Logger.GetLogger("main");
		
		InitContexts();

		float lastTime = 0;
		while (!ShouldClose())
		{
			foreach (WindowContext context in ContextsToAdd)
				if (!Contexts.Contains(context)) Contexts.Add(context);
			Contexts.RemoveAll(context => ContextsToRemove.Contains(context));
			ContextsToAdd.Clear();
			ContextsToRemove.Clear();
			
			float delta = (float)GLFW.GetTime() - lastTime;
			lastTime = (float)GLFW.GetTime();

			GLFW.PollEvents();
			
			foreach (WindowContext context in Contexts)
			{
				if (!context.Closed)
				{
					logger.LogInformation("Rendering context {Context}", context);
					unsafe
					{
						if (GLFW.WindowShouldClose(context.Window))
						{
							context.Close();
							context.CleanUp();
							continue;
						}

						GLFW.MakeContextCurrent(context.Window);
						context.Update(delta);
						if(!context.Closed) GLFW.SwapBuffers(context.Window);
					}
				}
			}
			
			// GLFW.WaitEventsTimeout((float)1/120);
		}
	}
}