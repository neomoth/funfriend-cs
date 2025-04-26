using OpenTK.Graphics.OpenGL;

namespace funfriend;

public static class FontManager
{
	private static List<string> QuotedSpaceSplit(string input)
	{
		List<string> result = new List<string>();
		List<char> chars = new List<char>();
		bool quoted = false;

		foreach (char c in input)
		{
			if (!quoted && c == ' ')
			{
				if (chars.Count > 0 && chars[^1] != ' ')
				{
					result.Add(new string(chars.ToArray()));
				}

				chars.Clear();
			}
			else
			{
				if (c == '"')
				{
					quoted = !quoted;
				}

				chars.Add(c);
			}
		}

		if (chars.Count > 0 && chars[^1] != ' ')
		{
			result.Add(new string(chars.ToArray()));
		}

		return result;
	}

	public struct BmCommon
	{
		public int LineHeight;
		public int Base;
		public int ScaleW;
		public int ScaleH;
	}

	public struct BmChar
	{
		public int Id;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public int XOffset;
		public int YOffset;
		public int XAdvance;
		public char Letter;
	}

	public struct BmKerning
	{
		public int First;
		public int Second;
		public int Amount;
	}

	public class BmSheet
	{
		public BmCommon Common;
		public List<BmChar> Chars;
		public List<BmKerning> Kernings;
	}

	public static BmSheet ParseBm(string data)
	{
		BmCommon common = new BmCommon();
		List<BmChar> chars = new List<BmChar>();
		List<BmKerning> kernings = new List<BmKerning>();

		foreach (var line in data.Split(['\n'], StringSplitOptions.RemoveEmptyEntries))
		{
			var words = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
			string? key = words.FirstOrDefault();
			var argsArray = QuotedSpaceSplit(string.Join(" ", words.Skip(1))).Select(x => x.Split('=')).ToList();
			var args = argsArray
				.Where(x => x.Length == 2)
				.ToDictionary(x => x[0], x => x[1]);

			switch (key)
			{
				case "common":
					common = new BmCommon
					{
						LineHeight = int.Parse(args["lineHeight"]),
						Base = int.Parse(args["base"]),
						ScaleW = int.Parse(args["scaleW"]),
						ScaleH = int.Parse(args["scaleH"])
					};
					break;

				case "char":
					chars.Add(new BmChar
					{
						Id = int.Parse(args["id"]),
						X = int.Parse(args["x"]),
						Y = int.Parse(args["y"]),
						Width = int.Parse(args["width"]),
						Height = int.Parse(args["height"]),
						XOffset = int.Parse(args["xoffset"]),
						YOffset = int.Parse(args["yoffset"]),
						XAdvance = int.Parse(args["xadvance"]),
						Letter = args.TryGetValue("letter", out string? letterStr) && !string.IsNullOrEmpty(letterStr.Trim('"'))
							? letterStr.Trim('"')[0]
							: '?'
					});
					break;

				case "kerning":
					kernings.Add(new BmKerning
					{
						First = int.Parse(args["first"]),
						Second = int.Parse(args["second"]),
						Amount = int.Parse(args["amount"])
					});
					break;
			}
		}

		return new BmSheet
		{
			Common = common,
			Chars = chars,
			Kernings = kernings
		};
	}

	public static int TextWidth(string text, BmSheet sheet)
	{
		return text.Sum(c => sheet.Chars.First(ch => ch.Letter == c).XAdvance);
	}

	// TODO: Implement kerning logic
	public static (int Width, int Height, List<(int X, int Y, BmChar Char)> Positions) PositionText(string text,
		BmSheet sheet)
	{
		List<(int X, int Y, BmChar Char)> positions = new List<(int, int, BmChar)>();
		int x = 0;

		foreach (char c in text)
		{
			BmChar bmChar = sheet.Chars.First(ch => ch.Letter == c);
			positions.Add((
				x + bmChar.XOffset,
				sheet.Common.Base - bmChar.Height - bmChar.YOffset + (sheet.Common.LineHeight - sheet.Common.Base),
				bmChar
			));

			x += bmChar.XAdvance;
		}

		return (x, sheet.Common.LineHeight, positions);
	}

	public static (float X, float Y, float W, float H) GetLetterCrop(BmChar character, BmSheet sheet)
	{
		float x = (character.X / (float)sheet.Common.ScaleW);
		float y = (character.Y / (float)sheet.Common.ScaleH);
		float w = (character.Width / (float)sheet.Common.ScaleW);
		float h = (character.Height / (float)sheet.Common.ScaleH);

		return (x, y, w, h);
	}

	public static (int VertexArray, int VertexBuffer) GetTextMesh(string text, BmSheet sheet, int offsetX, int offsetY,
		int width, int height)
	{
		List<float> vertices = new List<float>();
		List<int> indices = new List<int>();

		var positionData = PositionText(text, sheet);

		int i = 0;
		foreach (var letter in positionData.Positions)
		{
			BmChar character = letter.Char;
			var (x, y, w, h) = GetLetterCrop(character, sheet);

			float posX = ((letter.X + offsetX) / (float)width) * 2 - 1;
			float posW = (character.Width / (float)width) * 2;
			float posY = ((letter.Y + offsetY) / (float)height) * 2 - 1;
			float posH = (character.Height / (float)height) * 2;

			vertices.AddRange(new float[]
			{
				posX + posW, posY + posH, 0.0f, x + w, y, // top right
				posX + posW, posY, 0.0f, x + w, y + h, // bottom right
				posX, posY, 0.0f, x, y + h, // bottom left
				posX, posY + posH, 0.0f, x, y // top left
			});

			indices.AddRange(new int[]
			{
				0, 1, 3, // first triangle
				1, 2, 3 // second triangle
			}.Select(index => index + (i * 4)));

			i++;
		}

		// Create vertex array object
		int vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		// Create and populate vertex buffer
		int vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

		// Create and populate element buffer
		int ebo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);

		// Set up vertex attributes
		// Position attribute
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);
		// Texture coordinate attribute
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(1);


		return (vao, vbo);
	}
}