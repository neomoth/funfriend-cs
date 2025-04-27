using Microsoft.Extensions.Logging;

namespace funfriend;

public static class ConfigManager
{
	private const string ApplicationName = "funfriend";
	private const string ConfigName = "cfg.ini";

	public static Dictionary<string, Dictionary<string, object>> DefaultConfig = new()
	{
		{ "window", new Dictionary<string, object> { { "funfriend_size", 75 } } },
		{ "sound", new Dictionary<string, object> { { "volume", 0.2f } } },
		{ "buddies", new Dictionary<string, object> { { "types", "funfriend" } } }
	};

	private static bool _configInitialized;
	private static Dictionary<string, Dictionary<string, object>> _config = [];

	public static Dictionary<string, Dictionary<string, object>> Config
	{
		get
		{
			if (!_configInitialized)
			{
				throw new InvalidOperationException("Config read before initialization");
			}

			return _config;
		}
	}

	private static string GetConfigPath()
	{
		string path;

		switch (Environment.OSVersion.Platform)
		{
			case PlatformID.Win32NT:
				path = Path.Combine(Environment.GetEnvironmentVariable("APPDATA") ?? string.Empty, ApplicationName);
				break;
			case PlatformID.MacOSX:
				path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library",
					"Application Support", ApplicationName);
				break;
			case PlatformID.Unix:
			{
				var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
				path = Path.Combine(
					xdgConfigHome ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
						".config"), ApplicationName);
				break;
			}
			default:
				throw new NotSupportedException("Platform not supported.");
		}

		Console.WriteLine(path);
		
		return path;
	}

	public static void Init()
	{
		var configPath = GetConfigPath();
		Directory.CreateDirectory(configPath);

		var configFile = Path.Combine(configPath, ConfigName);

		// Logger.GetLogger("config manager").LogInformation($"config path: {configFile}");
		
		if (!File.Exists(configFile))
		{
			_config = new Dictionary<string, Dictionary<string, object>>(DefaultConfig);
			File.WriteAllText(configFile, BuildIni(_config));
		}
		else
		{
			var parsed = ParseIni(File.ReadAllText(configFile));
			_config = new Dictionary<string, Dictionary<string, object>>(DefaultConfig);

			foreach (var section in parsed)
			{
				if (!DefaultConfig.ContainsKey(section.Key)) continue;
				foreach (var keyValuePair in section.Value)
				{
					if (DefaultConfig[section.Key].TryGetValue(keyValuePair.Key, out object? value))
					{
						object castedValue = CastValue(value,
							keyValuePair.Value);
						if (castedValue is not null)
						{
							_config[section.Key][keyValuePair.Key] = castedValue;
							Console.WriteLine(castedValue);
						}
					}
					else
					{
						_config[section.Key][keyValuePair.Key] = keyValuePair.Value;
					}
				}
			}

			File.WriteAllText(configFile, BuildIni(_config));
		}

		_configInitialized = true;
	}

	private static object CastValue(object defaultValue, string value)
	{
		switch (defaultValue)
		{
			case string _:
				return value;
			case int _:
				return (int.TryParse(value, out int intValue) ? (object)intValue : null) ?? throw new InvalidOperationException();
			case float _:
				return (float.TryParse(value, out float floatValue) ? (object)floatValue : null) ?? throw new InvalidOperationException();
			case double _:
				return (double.TryParse(value, out double doubleValue) ? (object)doubleValue : null) ?? throw new InvalidOperationException();
			case bool _:
				return value.Equals("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase);
			default:
				Console.WriteLine($"obj: {defaultValue.GetType()}, str: {value}");
				throw new InvalidCastException("Invalid config value type");
		}
	}

	private static string BuildIni(Dictionary<string, Dictionary<string, object>> config)
	{
		var iniContent = "";

		foreach (var section in config)
		{
			iniContent += $"[{section.Key}]\n";
			foreach (var keyValuePair in section.Value)
			{
				iniContent += $"{keyValuePair.Key}={keyValuePair.Value}\n";
			}

			iniContent += "\n";
		}

		return iniContent;
	}

	private static Dictionary<string, Dictionary<string, string>> ParseIni(string iniContent)
	{
		var sections = new Dictionary<string, Dictionary<string, string>>();
		string? currentSection = null;

		foreach (var line in iniContent.Split('\n'))
		{
			var trimmedLine = line.Trim();
			if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(';'))
				continue;

			if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
			{
				currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
				sections[currentSection] = new Dictionary<string, string>();
			}
			else
			{
				var keyValue = trimmedLine.Split(['='], 2);
				if (keyValue.Length == 2 && currentSection != null)
				{
					sections[currentSection][keyValue[0].Trim()] = keyValue[1].Trim();
				}
			}
		}

		return sections;
	}
}