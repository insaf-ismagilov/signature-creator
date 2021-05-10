using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SignatureCreator.Options;

namespace SignatureCreator
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				var options = GetOptions(args);

				var signatureCreator = new FileSignatureCreator(options);

				var sw = Stopwatch.StartNew();
				var signature = signatureCreator.ComputeFileSignature();
				sw.Stop();

				PrintSignature(signature);
				Console.WriteLine("-----------------------------------------");
				Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static SignatureCreatorOptions GetOptions(string[] args)
		{
			if (args == null || args.Length < 2)
				throw new ArgumentException(nameof(args));

			string fileName = !string.IsNullOrEmpty(args[0])
				? args[0]
				: throw new ArgumentException(nameof(fileName));

			if (!long.TryParse(args[1], out var blockSize))
				throw new ArgumentException(nameof(blockSize));

			return new SignatureCreatorOptions
			{
				FileName = fileName,
				BlockSize = blockSize
			};
		}

		private static void PrintSignature(IReadOnlyDictionary<int, string> signature)
		{
			foreach (var (blockNumber, hash) in signature.OrderBy(x => x.Key))
			{
				Console.WriteLine($"{blockNumber}: {hash}");
			}
		}
	}
}