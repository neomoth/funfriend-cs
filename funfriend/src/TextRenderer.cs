namespace funfriend;
using OpenTK.Graphics.OpenGL;

public class TextRenderer
{
	public string Text { get; }
	public string Font { get; }
	public FontManager.BmSheet Sheet { get; }
	public int Width { get; }
	public int Height { get; }

	public int ShaderProgram { get; private set; }
	public int VertexArray { get; private set; }
	public int VertexBuffer { get; private set; }
	public int FontTexture { get; private set; }

	public TextRenderer(string text, string font, FontManager.BmSheet sheet, int width, int height)
	{
		Text = text;
		Font = font;
		Sheet = sheet;
		Width = width;
		Height = height;

		ShaderProgram = InitShaders();
		(VertexArray, VertexBuffer) = InitBuffers();
		FontTexture = InitTextures();
	}

	private int InitTextures()
	{
		// Load the texture with parameters
		var textureParams = new Dictionary<TextureParameterName, int>()
		{
			{ TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder },
			{ TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder },
			{ TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest },
			{ TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest },
		};

		return TextureManager.LoadTexture($"{Font}.png", textureParams).Tex;
	}

	private (int, int) InitBuffers()
	{
		// Calculate text width and height
		int textWidth = FontManager.TextWidth(Text, Sheet);
		int textHeight = Sheet.Common.LineHeight;

		// Generate the text mesh (for now, assume it returns a tuple with vertex array and buffer)
		return FontManager.GetTextMesh(Text, Sheet, Width / 2 - textWidth / 2, Height / 2 - textHeight / 2, Width, Height);
	}

	private int InitShaders()
	{
		// Load shaders
		return GLHelper.Shader("nop.frag", "nop.vert");
	}

	public void Render(float dt)
	{
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);

		// Bind the texture
		GL.BindTexture(TextureTarget.Texture2D, FontTexture);

		// Use the shader program
		GL.UseProgram(ShaderProgram);

		// Set the uniform for the texture
		int textureLocation = GL.GetUniformLocation(ShaderProgram, "texture1");
		GL.Uniform1(textureLocation, 0);

		// Bind vertex array and draw the elements
		GL.BindVertexArray(VertexArray);
		GL.DrawElements(PrimitiveType.Triangles, 6 * Text.Length, DrawElementsType.UnsignedInt, 0);
	}

	public void CleanUp()
	{
		// Delete the vertex array and buffer
		GL.DeleteVertexArray(VertexArray);
		GL.DeleteBuffer(VertexBuffer);
	}
}