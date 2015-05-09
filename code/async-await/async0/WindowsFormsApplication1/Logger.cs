using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace WindowsFormsApplication1
{
	internal static class Logger
	{
		private static string GetTimestamp()
		{
			DateTime now = DateTime.Now;
			string timestamp = now.ToString("yyyy-M-dd HH:mm:ss") + String.Format(".{0:000}", now.Millisecond);
			return timestamp;
		}

		public static void Info(params object[] parameters)
		{
			string current_timestamp = GetTimestamp();
			string thread_id = String.Format("{0:X8}", Thread.CurrentThread.ManagedThreadId);

			StringBuilder buffer = new StringBuilder();
			buffer.Append(current_timestamp);
			buffer.Append(" ");
			buffer.Append("[INFO]");
			buffer.Append(" ");
			buffer.Append("<THREAD:");
			buffer.Append(thread_id);
			buffer.Append("> ");

			foreach (var e in parameters)
			{
				buffer.Append(e);
			}

			Debug.WriteLine(buffer);
		}
	}
}
