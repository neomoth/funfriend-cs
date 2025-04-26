using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Buffer = System.Buffer;

namespace funfriend;

public static class GLHelper
{
	private static readonly string GLSLDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "glsl");
	
	public static void BufferDataArray<T>(BufferTarget target, List<T> data, BufferUsageHint usage) where T : struct
	{
		int dataSize = data.Count * System.Runtime.InteropServices.Marshal.SizeOf<T>();
		byte[] dataArray = new byte[dataSize];

		for (int i = 0; i < data.Count; i++)
		{
			byte[] byteData = StructToBytes(data[i]);
			Array.Copy(byteData, 0, dataArray, i * byteData.Length, byteData.Length);
		}

		int bufferId = GL.GenBuffer();
		GL.BindBuffer(target, bufferId);
		GL.BufferData(target, dataSize, dataArray, usage);
	}

	private static byte[] StructToBytes<T>(T obj) where T : struct
	{
		int size = System.Runtime.InteropServices.Marshal.SizeOf<T>();
		byte[] bytes = new byte[size];
		IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
		System.Runtime.InteropServices.Marshal.StructureToPtr(obj, ptr, false);
		System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, size);
		System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
		return bytes;
	}
	
	public static int Shader(string fragmentFilename, string vertexFilename)
	{
		// Build the full path for both fragment and vertex shader files
		string vertexShaderPath = Path.Combine(GLSLDirectory, vertexFilename);
		string fragmentShaderPath = Path.Combine(GLSLDirectory, fragmentFilename);

		// Check if the files exist
		if (!File.Exists(vertexShaderPath))
		{
			Console.WriteLine($"Vertex shader file not found: {vertexShaderPath}");
			return -1;
		}
        
		if (!File.Exists(fragmentShaderPath))
		{
			Console.WriteLine($"Fragment shader file not found: {fragmentShaderPath}");
			return -1;
		}

		string vertexShaderSource = File.ReadAllText(vertexShaderPath);
		string fragmentShaderSource = File.ReadAllText(fragmentShaderPath);

		int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
		int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

		int program = LinkProgram(vertexShader, fragmentShader);

		GL.DeleteShader(vertexShader);
		GL.DeleteShader(fragmentShader);

		return program;
	}

	private static int CompileShader(ShaderType type, string source)
	{
		int shader = GL.CreateShader(type);
		GL.ShaderSource(shader, source);
		GL.CompileShader(shader);

		GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
		if (success == 0)
		{
			GL.GetShaderInfoLog(shader, out string infoLog);
			Console.WriteLine($"Shader compile error ({type}): {infoLog}");
			throw new Exception($"Shader compilation failed: {infoLog}");
		}

		return shader;
	}


	private static int LinkProgram(int vertexShader, int fragmentShader)
	{
		int program = GL.CreateProgram();
		GL.AttachShader(program, vertexShader);
		GL.AttachShader(program, fragmentShader);
		GL.LinkProgram(program);

		GL.GetProgramInfoLog(program, out string infoLog);
		if (!string.IsNullOrEmpty(infoLog))
		{
			Console.WriteLine($"Program link error: {infoLog}");
		}

		return program;
	}
}