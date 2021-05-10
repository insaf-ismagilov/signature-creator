using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SignatureCreator
{
	class Program
	{
		static void Main(string[] args)
		{
			var filePath = args[0];
			var blockSize = long.Parse(args[1]);

			var hashes = ComputeBlockHashes(filePath, blockSize);

			foreach (var (blockNumber, hash) in hashes)
			{
				Console.WriteLine($"{blockNumber}: {hash}");
			}
		}
		
		private static string ConvertBytesToString(byte[] hashBytes)
		{
			var stringBuilder = new StringBuilder();

			foreach (var b in hashBytes)
				stringBuilder.AppendFormat("{0:X2}", b);

			return stringBuilder.ToString();
		}
		
		private static List<(int, string)> ComputeBlockHashes(string filePath, long blockSize)
		{
			var result = new List<(int, string)>();

			long offset = 0;
			long blockEnd = blockSize;

			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

			var blocksCount = (int) Math.Ceiling((double) fs.Length / blockSize);

			for (int i = 0; i < blocksCount; ++i)
			{
				var buffer = new byte[blockSize];
				
				using var sha256 = new SHA256Managed();
				
				while (true)
				{
					fs.Seek(offset, SeekOrigin.Begin);
					var bytesRead = fs.Read(buffer, 0, buffer.Length);

					offset += buffer.Length;

					if (blockEnd > offset)
						sha256.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
					else
					{
						sha256.TransformFinalBlock(buffer, 0, bytesRead);
						result.Add((i, ConvertBytesToString(sha256.Hash)));
						break;
					}
				}
				
				blockEnd += blockSize;
			}

			return result;
		}
	}
}