using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SignatureCreator
{
	class Program
	{
		static void Main(string[] args)
		{
			// TODO: inputs validation.
			var fileName = args[0];
			var blockSize = long.Parse(args[1]);

			var signatureCreator = new FileSignatureCreator(new SignatureCreatorOptions
			{
				FileName = fileName,
				BlockSize = blockSize
			});

			var sw = Stopwatch.StartNew();
			var signature = signatureCreator.ComputeFileSignature();
			sw.Stop();
			
			PrintSignature(signature);
			Console.WriteLine("-----------------------------------------");
			Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds} ms");
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