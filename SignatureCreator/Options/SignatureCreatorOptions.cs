namespace SignatureCreator.Options
{
	public class SignatureCreatorOptions
	{
		public string FileName { get; set; }
		public long BlockSize { get; set; }
		public long DefaultBlockBufferSize { get; set; } = 1024 * 300;
		public int DefaultMaxThreadsCount { get; set; } = 10;
		public ThreadsCreationType ThreadsCreationType { get; set; } = ThreadsCreationType.Constant;

		public long GetResultBlockBufferSize() => DefaultBlockBufferSize > BlockSize ? BlockSize : DefaultBlockBufferSize;
	}
}