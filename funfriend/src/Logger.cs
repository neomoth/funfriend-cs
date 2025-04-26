using Microsoft.Extensions.Logging;
namespace funfriend;

public static class Logger
{
	private static ILoggerFactory _loggerFactory;
	private static readonly object _lock = new();

	public static void Init(LogLevel minimumLevel = LogLevel.Trace)
	{
		if (_loggerFactory is not null) return;

		lock (_lock)
			_loggerFactory ??= LoggerFactory.Create(builder => { builder.SetMinimumLevel(minimumLevel).AddConsole(); });
	}

	public static ILogger<T> GetLogger<T>()
	{
		if (_loggerFactory is null)
			throw new InvalidOperationException("Logger is not initialized.");
		return _loggerFactory.CreateLogger<T>();
	}

	public static ILogger GetLogger(string category)
	{
		if (_loggerFactory is null)
			throw new InvalidOperationException("Logger is not initialized.");
		return _loggerFactory.CreateLogger(category);
	}
}