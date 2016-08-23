using NLog;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Teltec.Everest.Settings
{
	[Serializable]
	public class Properties
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		static Properties()
		{
			_Current = new Properties();
			Load();
		}

		public static Properties _Current;
		public static Properties Current
		{
			get { return _Current; }
		}

		private static string SettingsDirectory
		{
			get
			{
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.bin");

		public static int EstimatedOptimalThreadCount
		{
			get
			{
				int threadCount = 4 * (Environment.ProcessorCount > 4 ? Environment.ProcessorCount : 4);
				return threadCount;
			}
		}

		public static readonly int MaxThreadCountMin = 1;
		public static readonly int MaxThreadCountMax = 256;
		private int _MaxThreadCount = EstimatedOptimalThreadCount;
		public int MaxThreadCount
		{
			get { return _MaxThreadCount; }
			set
			{
				if (value < MaxThreadCountMin || value > MaxThreadCountMax)
					value = EstimatedOptimalThreadCount;

				_MaxThreadCount = value;
			}
		}

		public static readonly int UploadChunkSizeMin = 1; // MiB
		public static readonly int UploadChunkSizeMax = 5120; // MiB
		public static readonly int UploadChunkSizeDefault = 5; // MiB
		private int _UploadChunkSize = UploadChunkSizeDefault;
		public int UploadChunkSize // In MiB
		{
			get { return _UploadChunkSize; }
			set
			{
				if (value < UploadChunkSizeMin || value > UploadChunkSizeMax)
					value = UploadChunkSizeDefault;

				_UploadChunkSize = value;
			}
		}

		public static void Save()
		{
			logger.Info("Saving settings...");
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, _Current);
			stream.Close();
		}

		private void Sanitize()
		{
			if (MaxThreadCount < MaxThreadCountMin || MaxThreadCount > MaxThreadCountMax)
				MaxThreadCount = EstimatedOptimalThreadCount;

			if (UploadChunkSize < UploadChunkSizeMin || UploadChunkSize > UploadChunkSizeMax)
				UploadChunkSize = UploadChunkSizeDefault;
		}

		public static void Load()
		{
			try
			{
				logger.Info("Loading settings...");
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				_Current = (Properties)formatter.Deserialize(stream);
				_Current.Sanitize();
				stream.Close();
			}
			catch (Exception)
			{
				Console.WriteLine("{0} file doesn't exist. Will create it when needed.", SettingsFilePath);
			}
		}
	}
}
