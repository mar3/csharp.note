using System;

namespace download
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				ListBucketsFromAmazonS3();

				EnumerateObject3("バケット名");
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
			var accessKeyId = "" + System.Configuration.ConfigurationManager.AppSettings["aws_access_key_id"];
			var secretAccessKey = "" + System.Configuration.ConfigurationManager.AppSettings["aws_secret_access_key"];

			var s3 = new Amazon.S3.AmazonS3Client(
				accessKeyId,
				secretAccessKey,
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
			var client = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.APNortheast1);

			string key = "";

			while (true)
			{
				var request = new Amazon.S3.Model.ListObjectsV2Request { BucketName = bucketName, Prefix = "" };

				if (key != "")
				{
					// 次のページ
					request.ContinuationToken = key;
				}

				Console.WriteLine("[TRACE] (ListObjects)");

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
	}
}
