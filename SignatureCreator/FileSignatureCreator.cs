using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SignatureCreator
{
	public class FileSignatureCreator
	{
		private readonly string _fileName;
		private readonly long _blockSize;
		
		public FileSignatureCreator(string fileName, long blockSize)
		{
			_fileName = fileName;
			_blockSize = blockSize;
		}
		
		public IReadOnlyDictionary<int, string> ComputeFileSignature()
		{
			// TODO: change to multi-thread approach.
			var result = new ConcurrentDictionary<int, string>();

			long offset = 0;
			var blockEnd = _blockSize;
			var fileSize = GetFileSize();

			var blocksCount = (int) Math.Ceiling((double) fileSize / _blockSize);

			for (int i = 0; i < blocksCount; ++i)
			{
				var blockHash = ComputeBlockHash(offset, blockEnd);
				result.TryAdd(i, blockHash);

				offset += _blockSize;
				blockEnd += _blockSize;
			}

			return result;
		}

		private long GetFileSize()
		{
			using var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			return fs.Length;
		}

		private string ComputeBlockHash(long offset, long blockEnd)
		{
			var buffer = new byte[_blockSize];
			using var sha256 = new SHA256Managed();
			using var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				
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
					return ConvertBytesToString(sha256.Hash);
				}
			}
		}
		
		private static string ConvertBytesToString(byte[] hashBytes)
		{
			var stringBuilder = new StringBuilder();

			foreach (var b in hashBytes)
				stringBuilder.AppendFormat("{0:X2}", b);

			return stringBuilder.ToString();
		}
	}
}