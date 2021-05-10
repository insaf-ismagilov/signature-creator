using System;
using System.Collections.Generic;
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

			var signatureCreator = new FileSignatureCreator(fileName, blockSize);

			var signature = signatureCreator.ComputeFileSignature();

			PrintSignature(signature);
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