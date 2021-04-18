using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ZipArchiver
{
	internal sealed class Compressor
	{
		/// <summary>
		/// コンストラクター
		/// </summary>
		public Compressor()
		{

		}

		/// <summary>
		/// zip でアーカイブします。
		/// </summary>
		/// <param name="path"></param>
		public void Compress(string path)
		{
			// フルパスに変換します。
			if (!Directory.Exists(path))
			{
				return;
			}

			var date = GetDateString();

			path = Path.GetFullPath(path);

			string archiveName = path + "-" + date + ".zip";

			// ファイルがあれば削除
			if (File.Exists(archiveName))
			{
				Console.WriteLine("deleting file ... [" + archiveName + "]");
				File.Delete(archiveName);
			}

			// アーカイブ作成開始
			using (var zipStream = File.Create(archiveName))
			{
				using (var archiver = new ZipArchive(zipStream, ZipArchiveMode.Create))
				{
					this.Compress(archiver, "" /* root */, path);
				}
			}
		}

		/// <summary>
		/// zip でアーカイブします。
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="path"></param>
		public void Compress(ZipArchive archiver, string parent, string path)
		{
			path = Path.GetFullPath(path);

			// ルートからのパス
			Console.WriteLine("DIRECTORY: [{0}]", parent);

			var current = new DirectoryInfo(path);

			// ファイル
			foreach (var file in current.EnumerateFiles())
			{
				// zip 内におけるパス
				var entryName = BuildPath(parent, file.Name);

				Console.WriteLine("   adding file ... [{0}] << [{1}]", parent, file.FullName);

				this.AppendFile(archiver, entryName, file.FullName);
			}

			// ディレクトリをさらに掘り下げ
			foreach (var dir in current.EnumerateDirectories())
			{
				// zip 内におけるパス
				var entryName = BuildPath(parent, dir.Name);

				this.Compress(archiver, entryName, dir.FullName);
			}
		}

		private static string BuildPath(string parent, string name)
		{
			return parent == "" ? name : parent + "/" + name;
		}

		/// <summary>
		/// ファイルをアーカイブに追加します。
		/// </summary>
		/// <param name="archiver">アーカイバ</param>
		/// <param name="entryName">内部パス</param>
		/// <param name="path">ファイルの物理パス</param>
		private void AppendFile(ZipArchive archiver, string entryName, string path)
		{
			// zip 内に新しいエントリーを作成
			var entry = archiver.CreateEntry(entryName);

			// 最終更新日時
			entry.LastWriteTime = new FileInfo(path).LastWriteTime;

			// ファイルを開いてエントリーに書き込む(圧縮する)
			using (var sourceStream = File.OpenRead(path))
			{
				using (var entryStream = entry.Open())
				{
					sourceStream.CopyTo(entryStream);
				}
			}
		}

		private static string GetDateString()
		{
			return DateTime.Now.ToString("yyyyMMdd-HHmmss");
		}

		private static string GetCurrentTimestamp()
		{
			DateTime now = DateTime.Now;
			string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss") + String.Format(".{0:000}", now.Millisecond);
			return timestamp;
		}
	}
}
