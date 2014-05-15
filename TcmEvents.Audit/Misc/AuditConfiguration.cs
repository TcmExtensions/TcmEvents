#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: Audit Configuration Section
// ---------------------------------------------------------------------------------
//	Date Created	: January 28, 2014
//	Author			: Rob van Oostenrijk
// ---------------------------------------------------------------------------------
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tridion.Logging;

namespace TcmEvents.Misc
{
	public class AuditConfiguration : ConfigurationSection
	{
		private static AuditConfiguration mAuditConfiguration = null;

		private static Assembly ConfigResolveEventHandler(object sender, ResolveEventArgs args)
		{
			Logger.Write(String.Format("Resolving assembly: {0}", args.Name), "TcmEvents.Audit", LoggingCategory.General, TraceEventType.Information);

			return Assembly.GetExecutingAssembly();
		}

		private static T GetCustomConfig<T>(String sectionName) where T : ConfigurationSection
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ConfigResolveEventHandler);

			String configurationPath = Path.Combine(Tridion.ContentManager.ConfigurationSettings.GetTcmHomeDirectory(), @"config\TcmEvents.config");

			if (!File.Exists(configurationPath))
				throw new Exception(String.Format("Configuration file \"{0}\" not found.", configurationPath));

			// Create config file map to point to the configuration file
			ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = configurationPath
			};

			// Create configuration object that contains the content of the custom configuration file
			Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

			T section = configuration.GetSection(sectionName) as T;
			AppDomain.CurrentDomain.AssemblyResolve -= ConfigResolveEventHandler;

			return section;
		}

		public static AuditConfiguration Instance
		{
			get
			{
				if (mAuditConfiguration == null)
					mAuditConfiguration = GetCustomConfig<AuditConfiguration>("TcmEvents.Audit");

				return mAuditConfiguration;
			}
		}

		[ConfigurationProperty("database", IsRequired = true, DefaultValue = null)]
		public DatabaseElement Database
		{
			get
			{
				return base["database"] as DatabaseElement;
			}
		}
	}

	public class DatabaseElement : ConfigurationElement
	{
		[ConfigurationProperty("connectionString", IsRequired = true)]
		public String ConnectionString
		{
			get
			{
				return base["connectionString"] as String;
			}
		}
	}
}
