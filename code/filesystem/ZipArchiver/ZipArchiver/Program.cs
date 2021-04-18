using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ZipArchiver
{
	/// <summary>
	/// アプリケーションのメインクラス
	/// </summary>
	internal static class Program
	{
		/// <summary>
		/// アプリケーションのエントリーポイントです。
		/// </summary>
		/// <param name="args">コマンドライン引数</param>
		public static void Main(string[] args)
		{
			try
			{
				// 圧縮のテスト
				string path = 0 < args.Length ? args[0] : "";
				if (path == "")
				{
					return;
				}

				// 圧縮
				new Compressor().Compress(path);

				System.Threading.Thread.Sleep(1300);
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] " + e);
			}
		}
	}
}
