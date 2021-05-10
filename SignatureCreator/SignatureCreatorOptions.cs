namespace SignatureCreator
{
	public class SignatureCreatorOptions
	{
		public string FileName { get; set; }
		public long BlockSize { get; set; }
		public long DefaultBlockBufferSize { get; set; } = 1024 * 300;

		public long GetResultBlockBufferSize() => DefaultBlockBufferSize > BlockSize ? BlockSize : DefaultBlockBufferSize;
	}
}