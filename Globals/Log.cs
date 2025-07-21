using Godot;
using System;

/* Log
 * 2025-05-19
 */

public partial class Log : Node
{
	private enum LogLevel { Debug, Info, Warn, Error }

	private static readonly LogLevel logLevel = LogLevel.Info;

	public static void Debug(object source, string message)
	{
		if (logLevel != LogLevel.Debug)
			return;

		GD.Print(BuildMessageString(source, message));
	}

	public static void Debug(object source, string message, params object[] args)
	{
		if (logLevel != LogLevel.Debug)
			return;

		GD.Print(BuildMessageString(source, string.Format(message, args)));
	}

	public static void Info(object source, string message)
	{
		if (logLevel != LogLevel.Debug && logLevel != LogLevel.Info)
			return;

		string messageString = BuildMessageString(source, message);
		GD.Print(messageString);
	}

	public static void Info(object source, string message, params object[] args)
	{
		if (logLevel != LogLevel.Debug && logLevel != LogLevel.Info)
			return;

		string messageString = BuildMessageString(source, string.Format(message, args));
		GD.Print(messageString);
	}

	public static void Warning(object source, string message)
	{
		if (logLevel != LogLevel.Debug && logLevel != LogLevel.Info && logLevel != LogLevel.Warn)
			return;

		string messageString = BuildMessageString(source, message);
		GD.PrintErr("Warning: " + messageString);
		GD.PushWarning(messageString);
	}

	public static void Error(object source, string message)
	{
		if (logLevel != LogLevel.Debug && logLevel != LogLevel.Info && logLevel != LogLevel.Warn && logLevel != LogLevel.Error)
			return;

		string messageString = BuildMessageString(source, message);
		GD.PrintErr("Error: " + messageString);
		GD.PushError(messageString);
	}

	private static string BuildMessageString(object source, string message) => $"{source.GetType().Name}: {message}";
}
