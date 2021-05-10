using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using SignatureCreator.Options;

namespace SignatureCreator
{
	public class FileSignatureCreator
	{
		private readonly ConcurrentDictionary<int, string> _result = new();

		private readonly SignatureCreatorOptions _options;

		public FileSignatureCreator(SignatureCreatorOptions options)
		{
			_options = options;
		}

		public IReadOnlyDictionary<int, string> ComputeFileSignature()
		{
			var fileSize = GetFileSize();

			var blocksCount = (int) Math.Ceiling((double) fileSize / _options.BlockSize);

			var threads = new List<Thread>();

			if (_options.ThreadsCreationType == ThreadsCreationType.PerBlock)
				threads = CreatePerBlockThreads(blocksCount);

			if (_options.ThreadsCreationType == ThreadsCreationType.Constant)
				threads = CreateConstantNumberThreads(blocksCount);

			foreach (var thread in threads)
			{
				thread.Join();
			}

			return _result;
		}

		private void ComputeBlockHashThreadStart(object obj)
		{
			var param = (PerBlockThreadStartParameters) obj;

			var blockHash = ComputeBlockHash(param.Offset);
			_result.TryAdd(param.BlockNumber, blockHash);
		}

		private void ComputeBlockHashConstantThread(object obj)
		{
			var param = (ConstantThreadStartParameters) obj;

			var blockStart = param.ThreadNumber * param.BlocksInThreadCount;
			var blockEnd = blockStart + param.BlocksInThreadCount;
			if (blockEnd > param.BlocksCount)
				blockEnd = param.BlocksCount;

			var offset = blockStart * _options.BlockSize;

			for (var blockNumber = blockStart; blockNumber < blockEnd; blockNumber++)
			{
				var blockHash = ComputeBlockHash(offset);
				_result.TryAdd(blockNumber, blockHash);

				offset += _options.BlockSize;
			}
		}

		private long GetFileSize()
		{
			using var fs = new FileStream(_options.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			return fs.Length;
		}

		private string ComputeBlockHash(long offset)
		{
			var blockEnd = offset + _options.BlockSize;
			var buffer = new byte[_options.GetResultBlockBufferSize()];

			using var sha256 = new SHA256Managed();
			using var fs = new FileStream(_options.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);

			while (true)
			{
				fs.Seek(offset, SeekOrigin.Begin);

				var leftToReadBlock = blockEnd - offset;
				var readCount = leftToReadBlock < buffer.Length ? leftToReadBlock : buffer.Length;

				var bytesRead = fs.Read(buffer, 0, (int) readCount);

				offset += bytesRead;

				if (bytesRead > 0)
					sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);
				else
				{
					sha256.TransformFinalBlock(buffer, 0, bytesRead);
					return ConvertBytesToString(sha256.Hash);
				}
			}
		}

		private List<Thread> CreatePerBlockThreads(int blocksCount)
		{
			var threads = new List<Thread>();

			long offset = 0;
			for (var i = 0; i < blocksCount; ++i)
			{
				var thread = new Thread(ComputeBlockHashThreadStart);
				thread.Start(new PerBlockThreadStartParameters
				{
					Offset = offset,
					BlockNumber = i
				});
				threads.Add(thread);

				offset += _options.BlockSize;
			}

			return threads;
		}

		private List<Thread> CreateConstantNumberThreads(int blocksCount)
		{
			var threads = new List<Thread>();

			var threadsCount = blocksCount < _options.DefaultMaxThreadsCount ? blocksCount : _options.DefaultMaxThreadsCount;
			var blocksInThreadCount = (int) Math.Ceiling((double) blocksCount / threadsCount);

			for (var i = 0; i < threadsCount; i++)
			{
				var thread = new Thread(ComputeBlockHashConstantThread);
				thread.Start(new ConstantThreadStartParameters
				{
					BlocksCount = blocksCount,
					BlocksInThreadCount = blocksInThreadCount,
					ThreadNumber = i
				});
				threads.Add(thread);
			}

			return threads;
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