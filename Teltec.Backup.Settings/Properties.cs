using NLog;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Teltec.Backup.Settings
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

		private int _MaxThreadCount = EstimatedOptimalThreadCount;
		public int MaxThreadCount
		{
			get { return _MaxThreadCount; }
			set
			{
				if (value < 0 || value > 256)
					value = EstimatedOptimalThreadCount;

				_MaxThreadCount = value;
			}
		}

		private int _UploadChunkSize = 5; // MiB
		public int UploadChunkSize // In MiB
		{
			get { return _UploadChunkSize; }
			set
			{
				if (value < 0 || value > 5120) // 5 GiB
					value = 5; // MiB

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

		public static void Load()
		{
			try
			{
				logger.Info("Loading settings...");
				IFormatter formatter = new BinaryFormatter();
				Stream stream = new FileStream(SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				_Current = (Properties)formatter.Deserialize(stream);
				stream.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0} file doesn't exist. Will create it when needed.", SettingsFilePath);
			}
		}
	}
}
