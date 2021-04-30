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

				// ========== バケットを列挙 ==========
				{
					Console.WriteLine("[TRACE] バケットを列挙しています...");
					ListBucketsFromAmazonS3();
					Console.WriteLine();
				}

				// ========== バケット内のエントリーを列挙 ==========
				{
					Console.WriteLine("[TRACE] バケット内のエントリーを列挙しています...");
					EnumerateObject3("バケット名");
					Console.WriteLine();
				}

				// ========== バケット内のエントリーをダウンロード ==========
				{
					DownloadDirectory("バケット名", "パス", "tmp");
					Console.WriteLine();
				}

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

			using var s3 = new Amazon.S3.AmazonS3Client(
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
			try
			{
				Console.WriteLine("[TRACE] --- BUCKET: [" + bucketName + "] ---");

				uint objectCount = 0;

				var conf = new Configuration();

				// リージョン違う
				var regionEndPoint = Amazon.RegionEndpoint.APNortheast1;

				using var s3 = new Amazon.S3.AmazonS3Client(
					conf.AccessKeyId,
					conf.SecretAccessKey,
					regionEndPoint);

				string token = "";

				while (true)
				{
					var request = new Amazon.S3.Model.ListObjectsV2Request { BucketName = bucketName, Prefix = "" };
					if (token != "")
					{
						// 次のページ
						request.ContinuationToken = token;
					}

					using var response = s3.ListObjectsV2Async(request);
					var result = response.Result;
					foreach (var e in result.S3Objects)
					{
						Console.WriteLine("[TRACE] S3 Object: " + e.Key + " (Truncated: " + result.IsTruncated + ")");
						objectCount++;
					}

					// 次のページの有無を判断
					if (!result.IsTruncated)
					{
						break;
					}
					token = "" + result.ContinuationToken;
					if (token == "")
					{
						break;
					}

					Console.WriteLine("[TRACE] この応答には次のページがあります。 token: [" + token + "]");
				}

				Console.WriteLine("[TRACE] " + objectCount + " 個のオブジェクトがみつかりました。");
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] 予期しない例外です。" + e.Message);
			}
		}

		/// <summary>
		/// コンテンツをダウンロードします。
		/// </summary>
		/// <param name="bucketName">S3 バケットの名前</param>
		/// <param name="key"></param>
		/// <param name="localLocation"></param>
		private static void DownloadDirectory(string bucketName, string key, string localLocation)
		{
			try
			{
				var conf = new Configuration();

				var s3 = new Amazon.S3.AmazonS3Client(
					conf.AccessKeyId,
					conf.SecretAccessKey,
					Amazon.RegionEndpoint.APNortheast1);

				string token = "";

				while (true)
				{
					var request = new Amazon.S3.Model.ListObjectsV2Request { BucketName = bucketName, Prefix = "" + key };

					if (token != "")
					{
						// 次のページ
						request.ContinuationToken = token;
					}

					using var listObjectsResponse = s3.ListObjectsV2Async(request);
					var result = listObjectsResponse.Result;
					foreach (var e in result.S3Objects)
					{
						// オブジェクト取り出し要求
						var gor = new Amazon.S3.Model.GetObjectRequest();
						gor.BucketName = bucketName;
						gor.Key = e.Key;
						using var objectResponse = s3.GetObjectAsync(gor);
						using var res = objectResponse.Result;
						var lastModified = res.LastModified;

						if (!e.Key.StartsWith(key))
							throw new Exception("Key が不正です。");

						if (e.Key.EndsWith("/"))
						{
							// Must be application/x-directory
							var relativeKey = e.Key.Substring(key.Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
							relativeKey = RemoveTailSlash(relativeKey);
							var localPathName = System.IO.Path.Combine(localLocation, relativeKey);
							System.IO.Directory.CreateDirectory(localPathName);
							Console.WriteLine("[TRACE] create directory... [" + e.BucketName + "/" + e.Key + "] (" + res.Headers.ContentType + ")\n     >> [" + localPathName + "] (Truncated: " + result.IsTruncated + ")");
							Console.WriteLine();
						}
						else
						{
							// Something else
							var relativeKey = e.Key.Substring(key.Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
							var localPathName = System.IO.Path.Combine(localLocation, relativeKey);
							CreateParentDirectory(localPathName);
							System.Threading.CancellationToken cancellationToken;
							res.WriteResponseStreamToFileAsync(localPathName, false, cancellationToken).Wait();
							Console.WriteLine("[TRACE] DOWNLOADING FILE... [" + e.BucketName + "/" + e.Key + "] (" + res.Headers.ContentType + ")\n     >> [" + localPathName + "] (Truncated: " + result.IsTruncated + ")");
							Console.WriteLine();
						}
					}

					// 次のページの有無を判断
					if (!result.IsTruncated)
					{
						break;
					}
					token = "" + result.ContinuationToken;
					if (token == "")
					{
						break;
					}
					Console.WriteLine("[TRACE] 次のページ: [" + token + "]");

				}
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] 予期しない例外です。" + e.Message);
				Console.WriteLine();
			}
		}

		private static void CreateParentDirectory(string path)
		{
			var parent = System.IO.Path.GetDirectoryName(path);
			if (System.IO.Directory.Exists(parent))
				return;
			System.IO.Directory.CreateDirectory(parent);
		}

		private static string RemoveTailSlash(string key)
		{
			if (key == "") return key;
			return key.Substring(0, key.Length - 1);
		}
	}

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
	}
}
