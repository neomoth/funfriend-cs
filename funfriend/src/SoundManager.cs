using System.Media;
using System.Runtime.InteropServices;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using NVorbis;

namespace funfriend;

public static class SoundManager
{
	private static ALDevice _device;
	private static ALContext _context;

	private static readonly Dictionary<string, int> SoundBuffers = [];
	private static readonly Dictionary<string, int> SoundSources = [];

	public static void Init()
	{
		_device = ALC.OpenDevice(null);
		if (_device == IntPtr.Zero) throw new Exception("Failed to open default audio device.");

		_context = ALC.CreateContext(_device, null as int[]);
		if(_context==IntPtr.Zero) throw new Exception("Failed to create OpenAL context.");

		ALC.MakeContextCurrent(_context);

		AppDomain.CurrentDomain.ProcessExit += (_, _) =>
		{
			AL.DeleteSources(SoundSources.Values.ToArray());
			AL.DeleteBuffers(SoundBuffers.Values.ToArray());
			ALC.DestroyContext(_context);
			ALC.CloseDevice(_device);
		};
		
		var volume = (float)ConfigManager.Config["sound"]["volume"];
		AL.Listener(ALListenerf.Gain, (float)Math.Clamp(volume, 0, 1));
	}

	public static void PlaySound(string filepath)
	{
		string fullPath = Path.Combine(FunFriend.AssetsDirectory, filepath);
		
		if (!SoundBuffers.ContainsKey(fullPath))
		{
			int buffer = LoadOgg(fullPath);
			SoundBuffers[fullPath] = buffer;

			int source = AL.GenSource();
			AL.Source(source, ALSourcei.Buffer, buffer);
			SoundSources[fullPath] = source;
		}
		
		AL.SourcePlay(SoundSources[fullPath]);
	}

	private static int LoadOgg(string filepath)
	{
		using var vorbis = new VorbisReader(File.OpenRead(filepath), false);
		int channels = vorbis.Channels;
		int sampleRate = vorbis.SampleRate;

		List<float> floatSamples = [];
		float[] buffer = new float[4096];

		int samplesRead;
		while ((samplesRead = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
		{
			for (int i = 0; i < samplesRead; i++)
				floatSamples.Add(buffer[i]);
		}

		short[] pcm = new short[floatSamples.Count];
		for (int i = 0; i < floatSamples.Count; i++)
			pcm[i] = (short)(Math.Clamp(floatSamples[i], -1f, 1f) * short.MaxValue);

		IntPtr pcmPtr = Marshal.AllocHGlobal(pcm.Length * sizeof(short));
		Marshal.Copy(pcm, 0, pcmPtr, pcm.Length);
		
		int bufferId = AL.GenBuffer();
		ALFormat format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
		AL.BufferData(bufferId, format, pcmPtr, pcm.Length * sizeof(short), sampleRate);
		
		Marshal.FreeHGlobal(pcmPtr);

		return bufferId;
	}
}