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
		public static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("[TRACE] ### START ###");

				// ========== コンフィギュレーション ==========
				var conf = Configuration.GetInstance();
				conf.Configure(args);

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
			var conf = Configuration.GetInstance();

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
				var conf = Configuration.GetInstance();

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
}
