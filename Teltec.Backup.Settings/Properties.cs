using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Teltec.Backup.Settings
{
	[Serializable]
	public class Properties
	{
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

		public static void Save()
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, _Current);
			stream.Close();
		}

		public static void Load()
		{
			try
			{
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
