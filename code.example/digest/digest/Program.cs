using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digest
{
	public sealed class Program
	{
		public static void Main(string[] args)
		{
			const string source = "abcdefg";
			Console.WriteLine("MD5: [" + md5(source) + "]");
			Console.WriteLine("SHA256: [" + sha256(source) + "]");
		}

		private static string md5(string source)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(source == null ? "" : source);
			bytes = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
			string response = BitConverter.ToString(bytes);
			return response.Replace("-", "");
		}

		private static string sha256(string source)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(source == null ? "" : source);
			bytes = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);
			string response = BitConverter.ToString(bytes);
			return response.Replace("-", "");
		}
	}
}
