using System;

internal sealed class Stopwatch
{
	private long _start = Environment.TickCount;

	public override string ToString()
	{
		long milliseconds = Environment.TickCount - _start;

		return ToString(milliseconds);
	}

	private static string ToString(long milliseconds)
	{
		long seconds = milliseconds / 1000;
		milliseconds = milliseconds % 1000;

		long minutes = seconds / 60;
		seconds = seconds % 60;

		long hours = minutes / 60;
		minutes = minutes % 60;

		return String.Format(
			"{0:00}:{1:00}:{2:00}.{3:000}",
			hours, minutes, seconds, milliseconds);
	}
}
