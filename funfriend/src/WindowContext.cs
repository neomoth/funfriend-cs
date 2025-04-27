using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace funfriend;

public abstract class WindowContext
{
	public unsafe Window* Window { get; }
	public bool Closed { get; private set; }

	protected GLFWBindingsContext context;

	protected WindowContext(string title, int width, int height, bool passthrough)
	{
		GLFW.DefaultWindowHints();
		GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
		GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
		GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
		GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
		GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
		
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Resizable, false);
		GLFW.WindowHint(WindowHintBool.Floating, true);
		GLFW.WindowHint(WindowHintBool.Focused, false);
		GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
		GLFW.WindowHint(WindowHintInt.AlphaBits, 8);
		GLFW.WindowHint(WindowHintBool.FocusOnShow, false);
		GLFW.WindowHint(WindowHintBool.MousePassthrough, passthrough);

		unsafe
		{
			Window = GLFW.CreateWindow(width, height, title, null, null);
			GLFW.MakeContextCurrent(Window);
		}

		context = new GLFWBindingsContext();
		
		GL.LoadBindings(context);
		
		
		GL.GetInteger(GetPName.AlphaBits, out int alphaBits);
		Logger.GetLogger<WindowContext>().LogInformation("Alpha bits in framebuffer: {AlphaBits}", alphaBits);
		
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
		Logger.GetLogger<WindowContext>().LogInformation("Closing context {WindowContext2}", this);
		unsafe
		{
			GLFW.DestroyWindow(Window);
		}
		Closed = true;
		FunFriend.QueueRemoveContext(this);
	}
	
	public virtual void CleanUp(){}
}