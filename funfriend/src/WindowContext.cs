using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace funfriend;

public abstract class WindowContext
{
	public unsafe Window* Window { get; }
	public bool Closed { get; private set; }

	protected GLFWBindingsContext context;

	protected WindowContext(string title, int width, int height, bool transparent)
	{
		GLFW.DefaultWindowHints();
		if (transparent) GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
		GLFW.WindowHint(WindowHintBool.FocusOnShow, true);
		GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
		GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
		GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
		GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
		GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
		
		GLFW.WindowHint(WindowHintBool.Resizable, false);
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Floating, true);
		GLFW.WindowHint(WindowHintBool.Focused, false);

		unsafe
		{
			Window = GLFW.CreateWindow(width, height, title, null, null);
			GLFW.MakeContextCurrent(Window);
		}

		context = new GLFWBindingsContext();
		
		GL.LoadBindings(context);
		
		Logger.GetLogger<WindowContext>().LogInformation("Created context {WindowContext1}", this);
	}

	public virtual void Update(float delta)
	{
		unsafe
		{
			GLFW.MakeContextCurrent(Window);
			// GL.LoadBindings(context);
		}
	}

	public virtual void Close()
	{
		unsafe
		{
			GLFW.DestroyWindow(Window);
		}
		Closed = true;
		FunFriend.QueueRemoveContext(this);
	}
	
	public virtual void CleanUp(){}
}