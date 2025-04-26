using funfriend.buddies;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace funfriend;

public class BuddyRenderer
{
	public Buddy Buddy { get; }
	
	public int BuddyShader { get; }
	public int BgShader { get; }
	public int VertexArray { get; }
	public int VertexBuffer { get; }
	public int IndexBuffer { get; }
	public TextureManager.TextureBasket Textures { get; }
	public TextureManager.SizedTexture? BgTexture { get; }

	private readonly ILogger<BuddyRenderer> logger;
	
	public BuddyRenderer(Buddy buddy)
	{
		Buddy = buddy;
		(BuddyShader, BgShader) = InitShaders();
		(VertexArray, VertexBuffer, IndexBuffer) = InitBuffers();
		Textures = buddy.Textures();
		BgTexture = buddy.BgTexture();
		
		logger = Logger.GetLogger<BuddyRenderer>();
		logger.LogInformation("Texture count: {Count}", Textures.Textures.Count);
		foreach (var tex in Textures.Textures)
		{
			logger.LogInformation("Texture {Id}: {Width}x{Height}", tex.Tex, tex.Width, tex.Height);
		}

	}

	private Vec2 FunfriendSize
	{
		get
		{
			var size = (int)ConfigManager.Config["window"]["funfriend_size"];
			return new Vec2(size);
		}
	}
	
	private (int, int, int) InitBuffers()
	{
		float[] vertices =
		[
			// Positions         // Texture coordinates
			1.0f,   1.0f,   0.0f,   1.0f,   1.0f,  // Top right
			1.0f,  -1.0f,   0.0f,   1.0f,   0.0f,  // Bottom right
			-1.0f,  -1.0f,   0.0f,   0.0f,   0.0f,  // Bottom left
			-1.0f,   1.0f,   0.0f,   0.0f,   1.0f   // Top left
		];

		uint[] indices =
		[
			0, 1, 3,  // First triangle
			1, 2, 3   // Second triangle
		];

		var vBuf = GL.GenBuffer();
		var iBuf = GL.GenBuffer();
		var vArr = GL.GenVertexArray();

		GL.BindVertexArray(vArr);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vBuf);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);  // Position
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));  // TexCoords
		GL.EnableVertexAttribArray(1);

		GL.BindBuffer(BufferTarget.ElementArrayBuffer, iBuf);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
		
		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		
		return (vArr, vBuf, iBuf);
	}

	private (int, int) InitShaders()
	{
		var buddyShader = GLHelper.Shader("funfriend.frag", "nop.vert");
		var bgShader = GLHelper.Shader("nop.frag", "nop.vert");
		return (buddyShader, bgShader);
	}

	public void Render(float delta, int width, int height)
	{
		GL.Viewport(0, 0, width, height);
		GL.ClearColor(0f, 0f, 0f, 0f);
		GL.Clear(ClearBufferMask.ColorBufferBit);
		
		Textures.Update(delta);
		var frame = Textures.Texture;

		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		if (BgTexture is not null)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, BgTexture.Value.Tex);
			GL.UseProgram(BgShader);
			GL.Uniform1(GL.GetUniformLocation(BgShader, "texture1"), 0);
			GL.BindVertexArray(VertexArray);
			GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
			GL.UseProgram(0);
			GL.BindVertexArray(0);
		}
		
		GL.ActiveTexture(TextureUnit.Texture0);
		GL.BindTexture(TextureTarget.Texture2D, frame.Tex);
		GL.UseProgram(BuddyShader);
		
		int texLoc = GL.GetUniformLocation(BuddyShader, "texture1");
		int sizeLoc = GL.GetUniformLocation(BuddyShader, "funfriendSize");
		int resLoc = GL.GetUniformLocation(BuddyShader, "resolution");
		int timeLoc = GL.GetUniformLocation(BuddyShader, "time");

		logger.LogInformation("Uniform locations - tex: {Tex}, size: {Size}, res: {Res}, time: {Time}",
			texLoc, sizeLoc, resLoc, timeLoc);

		GL.Uniform1(GL.GetUniformLocation(BuddyShader, "texture1"), 0);
		GL.Uniform2(GL.GetUniformLocation(BuddyShader, "funfriendSize"), FunfriendSize.X, FunfriendSize.Y);
		GL.Uniform2(GL.GetUniformLocation(BuddyShader, "resolution"), width, height);
		GL.Uniform1(GL.GetUniformLocation(BuddyShader, "time"), (float)GLFW.GetTime());

		
		GL.BindVertexArray(VertexArray);
		GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
		GL.UseProgram(0);
		GL.BindVertexArray(0);
		GL.BindTexture(TextureTarget.Texture2D, 0);
	}

	public void CleanUp()
	{
		GL.DeleteBuffer(VertexBuffer);
		GL.DeleteVertexArray(VertexArray);
		GL.DeleteProgram(BuddyShader);
		GL.DeleteProgram(BgShader);
	}
}