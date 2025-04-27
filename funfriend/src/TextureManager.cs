using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace funfriend;

public static class TextureManager
{
	// static TextureManager()
	// {
	// 	StbImage.stbi_set_flip_vertically_on_load(1);
	// }
	
	public static readonly Dictionary<TextureParameterName, int> DefaultTextureParameters = new()
	{
		{ TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder },
		{ TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder },
		{ TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest },
		{ TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest },
	};

	public struct SizedTexture(int tex, int width, int height)
	{
		public int Tex = tex;
		public int Width = width;
		public int Height = height;
	}

	public static SizedTexture LoadTexture(string filepath, Dictionary<TextureParameterName, int>? parameters = null)
	{
		// StbImage.stbi_set_flip_vertically_on_load(filepath.Contains("font") ? 0 : 1); // for some reason buddies are upside down??? but not text??? idfk man

		parameters ??= DefaultTextureParameters;

		string fullPath = Path.Combine(FunFriend.AssetsDirectory, filepath);
		// Logger.GetLogger("texture manager").LogInformation($"filepath: {fullPath}");
		
		int texture = GL.GenTexture();
		int width = 0, height = 0;
		
		ImageResult image = ImageResult.FromStream(File.OpenRead(fullPath), ColorComponents.RedGreenBlueAlpha);

		var data = image.Data.ToArray();
		
		GL.BindTexture(TextureTarget.Texture2D, texture);
		
		foreach(var param in parameters)
			GL.TexParameter(TextureTarget.Texture2D, param.Key, param.Value);
		
		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
		
		return new SizedTexture(texture, image.Width, image.Height);
	}

	public class TextureBasket
	{
		public List<SizedTexture> Textures { get; }
		public float Fps { get; }
		public float T;

		public TextureBasket(List<SizedTexture> textures, float fps)
		{
			Textures = textures;
			Fps = fps;
			T = 0;
		}

		public int Frame => (int)Math.Floor(T / Fps) % Textures.Count;
		
		public SizedTexture Texture => Textures[Frame];

		public void Update(float delta)
		{
			T += delta;
		}
	}
}