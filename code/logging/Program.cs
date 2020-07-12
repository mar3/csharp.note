using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logging
{
	/// <summary>
	/// シンプルなロガー
	/// </summary>
	internal static class Logger
	{
		private static string GetCurrentTimestamp()
		{
			return DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
		}

		public static void Trace(params object[] args)
		{
			// 実行中のプロセスのID
			var currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

			// 実行中のスレッドを指す一意な識別子
			var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

			// ロギング操作を呼び出しているスタックフレーム
			var frame = new System.Diagnostics.StackFrame(1);
			var methodName = frame.GetMethod().Name;
			var className = frame.GetMethod().ReflectedType.FullName;
			var line = new System.Text.StringBuilder();

			line.Append(GetCurrentTimestamp());
			line.Append(" [TRACE] ");
			line.Append("(");
			line.Append("process: ");
			line.Append(string.Format("0x{0:X8}", currentProcessId));
			line.Append(", thread: ");
			line.Append(string.Format("0x{0:X8}", currentThreadId));
			line.Append(")");
			line.Append(" <");
			line.Append(className);
			line.Append(".");
			line.Append(methodName);
			line.Append("> ");
			foreach (object unknown in args)
				line.Append(unknown);

			// コンソールに文字列を出力します。
			Console.WriteLine(line);

			// ファイルに文字列を出力します。
			System.IO.TextWriter writer = new System.IO.StreamWriter(GetTempPath(), true);
			writer.WriteLine(line);
			writer.Close();
		}

		private static string _path = "";

		private static string GetTempPath()
		{
			if (_path != null && _path != "")
			{
				return _path;
			}
			_path = "" + System.IO.Path.GetTempPath();
			if (_path == "")
			{
				return "application.log";
			}
			_path = System.IO.Path.Combine(_path, "application.log");
			return _path;
		}
	}

	internal sealed class Model1
	{
		public void run()
		{
			Logger.Trace("$$$ START $$$");
			Logger.Trace("--- END ---");
		}
	}

	/// <summary>
	/// アプリケーション本体のクラスです。
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// エントリーポイント
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Logger.Trace("### START ###");
			new Model1().run();
			Logger.Trace("--- END ---");
		}
	}
}

