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
					ListBucketsFromAmazonS3(
						conf.AccessKeyId, conf.SecretAccessKey, conf.Region);
					Console.WriteLine();
				}

				// ========== バケット内のエントリーを列挙 ==========
				{
					Console.WriteLine("[TRACE] バケット内のエントリーを列挙しています...");
					EnumerateObject3(
						conf.AccessKeyId, conf.SecretAccessKey, conf.Region, conf.Bucket);
					Console.WriteLine();
				}

				// ========== バケット内のエントリーをダウンロード ==========
				{
					DownloadDirectory(
						conf.AccessKeyId, conf.SecretAccessKey, conf.Region, conf.Bucket, conf.Key, conf.Destination);
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
		private static void ListBucketsFromAmazonS3(
			string accessKeyId, string secretAccessKey, Amazon.RegionEndpoint regionEndPoint)
		{
			using var s3 = new Amazon.S3.AmazonS3Client(
				accessKeyId, secretAccessKey, regionEndPoint);

			var buckets = s3.ListBucketsAsync();

			foreach (var b in buckets.Result.Buckets)
			{
				Console.WriteLine("[TRACE] bucket: [{0}]", b.BucketName);
			}
		}

		/// <summary>
		/// 特定バケット内のオブジェクトを列挙します。
		/// </summary>
		/// <param name="accessKeyId"></param>
		/// <param name="secretAccessKey"></param>
		/// <param name="regionEndPoint"></param>
		/// <param name="bucketName">バケット</param>
		private static void EnumerateObject3(
			string accessKeyId, string secretAccessKey, Amazon.RegionEndpoint regionEndPoint, string bucketName)
		{
			try
			{
				Console.WriteLine("[TRACE] --- BUCKET: [" + bucketName + "] ---");

				uint objectCount = 0;

				using var s3 = new Amazon.S3.AmazonS3Client(accessKeyId, secretAccessKey, regionEndPoint);

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
		/// <param name="accessKeyId"></param>
		/// <param name="secretAccessKey"></param>
		/// <param name="regionEndPoint"></param>
		/// <param name="bucketName"></param>
		/// <param name="key"></param>
		/// <param name="localLocation"></param>
		private static void DownloadDirectory(
			string accessKeyId, string secretAccessKey, Amazon.RegionEndpoint regionEndPoint,
			string bucketName, string key, string localLocation)
		{
			// TODO もっとシンプルに
			try
			{
				Console.WriteLine("[TRACE] オブジェクトをダウンロードしています... [" + localLocation + "]");

				var s3 = new Amazon.S3.AmazonS3Client(accessKeyId, secretAccessKey, regionEndPoint);

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

						if (e.Key.EndsWith("/")) // application/x-directory
						{
							// パス体系を変更
							var relativeKey = e.Key.Substring(key.Length);
							relativeKey = Util.RemoveTailSlash(relativeKey);
							relativeKey = relativeKey.Replace('/', System.IO.Path.DirectorySeparatorChar);

							// ローカルパスを生成
							var localPathName = Util.MakePath(localLocation, relativeKey);
							System.IO.Directory.CreateDirectory(localPathName);

							Console.Write("[TRACE] create [" + e.Key + "]");
							Console.Write(" >> [" + localPathName + "]");
							Console.Write(" (ContentType: " + res.Headers.ContentType + "");
							Console.Write(", Truncated: " + result.IsTruncated + ")");
							Console.WriteLine();
						}
						else // ファイル
						{
							// パス体系を変更
							var relativeKey = e.Key.Substring(key.Length);
							relativeKey = relativeKey.Replace('/', System.IO.Path.DirectorySeparatorChar);

							// ローカルパスを生成
							var localPathName = Util.MakePath(localLocation, relativeKey);

							// ファイルをダウンロード
							Util.CreateParentDirectory(localPathName);
							System.Threading.CancellationToken cancellationToken;
							res.WriteResponseStreamToFileAsync(localPathName, false, cancellationToken).Wait();

							Console.Write("[TRACE] downloading [" + e.Key + "]");
							Console.Write(" >> [" + localPathName + "]");
							Console.Write(" (ContentType: " + res.Headers.ContentType + "");
							Console.Write(", Truncated: " + result.IsTruncated + ")");
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
	}
}
