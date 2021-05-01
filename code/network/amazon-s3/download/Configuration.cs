using System;
using System.Collections.Generic;
using System.Text;

namespace download
{
	/// <summary>
	/// コンフィギュレーションクラス
	/// </summary>
	internal sealed class Configuration
	{
		/// <summary>
		/// ACCESS_KEY_ID
		/// </summary>
		private string accessKeyId = "";

		/// <summary>
		/// SECRET_ACCESS_KEY
		/// </summary>
		private string secretAccessKey = "";

		/// <summary>
		/// 
		/// </summary>
		private Amazon.RegionEndpoint region = Amazon.RegionEndpoint.APNortheast1;

		/// <summary>
		/// 
		/// </summary>
		private string bucket = "";

		/// <summary>
		/// 
		/// </summary>
		private string key = "";

		/// <summary>
		/// 
		/// </summary>
		private string destination = "";

		/// <summary>
		/// 
		/// </summary>
		private static Configuration _instance = null;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static Configuration GetInstance()
		{
			if (_instance == null)
				_instance = new Configuration();
			return _instance;
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		private Configuration()
		{
		}

		/// <summary>
		/// コンフィギュレーション
		/// </summary>
		/// <param name="args">コマンドライン引数</param>
		public void Configure(string[] args)
		{
			// 消去
			{
				this.region = Amazon.RegionEndpoint.APNortheast1;
				this.bucket = "";
				this.key = "";
				this.destination = "";
			}

			foreach (var e in args)
			{
				Console.WriteLine("[TRACE] 引数: [" + e + "]");
			}

			// 引数チェック
			if (args.Length < 2)
			{
				throw new ApplicationError("引数エラー");
			}

			// コピー元
			var uri = new Amazon.S3.Util.AmazonS3Uri(args[0]);
			this.region = uri.Region;
			this.bucket = uri.Bucket;
			this.key = uri.Key;

			// コピー先
			this.destination = args[1];

			// credentials
			this.accessKeyId = "" + System.Configuration.ConfigurationManager.AppSettings["aws_access_key_id"];
			this.secretAccessKey = "" + System.Configuration.ConfigurationManager.AppSettings["aws_secret_access_key"];

			// 環境変数によるコンフィギュレーションの書き換え
			this.accessKeyId = SelectValid(
				Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), this.accessKeyId);
			this.secretAccessKey = SelectValid(
				Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"), this.secretAccessKey);
		}

		/// <summary>
		/// 有効な値の選択(left 優先)
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		private static string SelectValid(string left, string right)
		{
			return string.IsNullOrEmpty(left) ? right : left;
		}

		/// <summary>
		/// ACCESS_KEY_ID
		/// </summary>
		public string AccessKeyId
		{
			get
			{
				return "" + this.accessKeyId;
			}
		}

		/// <summary>
		/// SECRET_ACCESS_KEY
		/// </summary>
		public string SecretAccessKey
		{
			get
			{
				return "" + this.secretAccessKey;
			}
		}

		public Amazon.RegionEndpoint Region
		{
			get
			{
				return this.region;
			}
		}

		public string Bucket
		{
			get
			{
				return this.bucket;
			}
		}

		public string Key
		{
			get
			{
				return this.key;
			}
		}

		public string Destination
		{
			get
			{
				return this.destination;
			}
		}
	}
}
