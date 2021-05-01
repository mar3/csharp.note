using System;
using System.Collections.Generic;
using System.Text;

namespace download
{
	/// <summary>
	/// 各種汎用操作
	/// </summary>
	internal static class Util
	{
		/// <summary>
		/// ファイルパスのディレクトリ部分を作成します。
		/// </summary>
		/// <param name="path"></param>
		public static void CreateParentDirectory(string path)
		{
			// パス区切り文字が無ければ無視
			if (!path.Contains(System.IO.Path.DirectorySeparatorChar))
				return;

			// 最後の区切り文字までを取り出し
			var parent = System.IO.Path.GetDirectoryName(path);

			// ディレクトリが無ければ作成
			if (System.IO.Directory.Exists(parent))
				return;
			System.IO.Directory.CreateDirectory(parent);
		}

		/// <summary>
		/// 終端が "/" だった場合は取り除きます。
		/// </summary>
		/// <param name="key">S3 の Key</param>
		/// <returns></returns>
		public static string RemoveTailSlash(string key)
		{
			if (string.IsNullOrEmpty(key))
				return "";

			if (key.EndsWith("/"))
			{
				Chop(ref key);
				return key;
			}

			return key;
		}

		/// <summary>
		/// 終端の chop
		/// </summary>
		/// <param name="s"></param>
		public static void Chop(ref string s)
		{
			if (s == null || s == "")
				return;
			s = s.Remove(s.Length - 1);
		}

		/// <summary>
		/// 先頭の chop
		/// </summary>
		/// <param name="s"></param>
		public static void ChopHead(ref string s)
		{
			if (s == null || s == "")
				return;
			s = s.Remove(0, 1);
		}

		/// <summary>
		/// 文字列の終端にパス区切り文字を付加します。
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string TerminatePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return "";
			if (path.EndsWith(System.IO.Path.DirectorySeparatorChar))
				return path;
			path += System.IO.Path.DirectorySeparatorChar;
			return path;
		}

		/// <summary>
		/// パス結合
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static string MakePath(string left, string right)
		{
			// 先頭がパス区切り文字で始まっている場合は取り除く
			while (right.StartsWith(System.IO.Path.DirectorySeparatorChar))
				ChopHead(ref right);

			return TerminatePath(left) + right;
		}
	}
}
