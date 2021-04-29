using System;

namespace download
{
	/// <summary>
	/// アプリケーション本体
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// アプリケーションのエントリーポイント
		/// </summary>
		/// <param name="args">コマンドライン引数</param>
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("[TRACE] ### START ###");
				Console.WriteLine("[TRACE] バケットを列挙しています...");
				ListBucketsFromAmazonS3();
				Console.WriteLine();

				Console.WriteLine("[TRACE] バケット内のエントリーを列挙しています...");
				EnumerateObject3("バケット名");
				Console.WriteLine();

				DownloadDirectory("バケット名", "パス");
				Console.WriteLine();

				Console.WriteLine("[TRACE] --- END ---");
			}
			catch (Exception e)
			{
				Console.Write("[TRACE] ");
				Console.WriteLine(e);
				Console.WriteLine("[ERROR] 予期しないエラーが発生しました。");
			}
		}

		/// <summary>
		/// バケットを列挙します。
		/// </summary>
		private static void ListBucketsFromAmazonS3()
		{
			var conf = new Configuration();

			var s3 = new Amazon.S3.AmazonS3Client(
				conf.AccessKeyId,
				conf.SecretAccessKey,
				Amazon.RegionEndpoint.APNortheast1);

			var buckets = s3.ListBucketsAsync();

			foreach (var b in buckets.Result.Buckets)
			{
				Console.WriteLine("[TRACE] bucket: [{0}]", b.BucketName);
			}
		}

		/// <summary>
		/// 特定バケット内のオブジェクトを列挙します。
		/// </summary>
		/// <param name="bucketName">バケット</param>
		private static void EnumerateObject3(string bucketName)
		{
			var conf = new Configuration();

			var client = new Amazon.S3.AmazonS3Client(
				conf.AccessKeyId,
				conf.SecretAccessKey,
				Amazon.RegionEndpoint.APNortheast1);

			string key = "";

			while (true)
			{
				var request = new Amazon.S3.Model.ListObjectsV2Request { BucketName = bucketName, Prefix = "" };

				if (key != "")
				{
					// 次のページ
					request.ContinuationToken = key;
				}

				using (var response = client.ListObjectsV2Async(request))
				{
					var result = response.Result;
					foreach (var e in result.S3Objects)
					{
						Console.WriteLine("[TRACE] S3 Object: " + e.Key + " (Truncated: " + result.IsTruncated + ")");
					}

					// 次のページの有無を判断
					if (!result.IsTruncated)
					{
						break;
					}
					key = "" + result.ContinuationToken;
					if (key == "")
					{
						break;
					}
					Console.WriteLine("[TRACE] 次のページ: [" + key + "]");
				}
			}
		}

		private static void DownloadDirectory(string bucketName, string path)
		{

		}
	}

	internal sealed class Configuration
	{
		private string accessKeyId = "";

		private string secretAccessKey = "";

		/// <summary>
		/// コンストラクター
		/// </summary>
		public Configuration()
		{
			// 既定のコンフィギュレーション
			this.Configure();
			// 環境変数によるコンフィギュレーションの書き換え
			this.ConfigureFromEnv();
		}

		/// <summary>
		/// コンフィギュレーション
		/// </summary>
		private void Configure()
		{
			this.accessKeyId = "" + System.Configuration.ConfigurationManager.AppSettings["aws_access_key_id"];
			this.secretAccessKey = "" + System.Configuration.ConfigurationManager.AppSettings["aws_secret_access_key"];
		}

		/// <summary>
		/// 環境変数によるコンフィギュレーションの書き換え
		/// </summary>
		private void ConfigureFromEnv()
		{
			// 環境変数による設定を検出したら、値が上書きされます。
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

		public string AccessKeyId
		{
			get
			{
				return "" + this.accessKeyId;
			}
		}

		public string SecretAccessKey
		{
			get
			{
				return "" + this.secretAccessKey;
			}
		}
	}
}
