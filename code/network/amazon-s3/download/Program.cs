using System;

namespace download
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				EnumerateObject3("バケット名");
			}
			catch (Exception e)
			{
				Console.Write("[TRACE] ");
				Console.WriteLine(e);
				Console.WriteLine("[ERROR] 予期しないエラーが発生しました。");
			}
		}

		private static void EnumerateObject3(string bucketName)
		{
			var client  = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.APNortheast1);

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
