#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: Configuration
// ---------------------------------------------------------------------------------
//	Date Created	: January 28, 2014
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace TcmEvents.Misc
{
	/// <summary>
	/// <see cref="Configuration" /> exposes a <see cref="T:System.Configuration.Configuration" /> for custom files.
	/// </summary>
	public static class Configuration
	{
		private static System.Configuration.Configuration mConfiguration = null;

		/// <summary>
		/// Retrieve the TcmEvents <see cref="EventBase"/> configuration.
		/// </summary>
		/// <value>
		/// <see cref="T:System.Configuration.Configuration" />
		/// </value>
		public static System.Configuration.Configuration Instance
		{
			get
			{
				if (mConfiguration == null)
				{
					String configurationPath = Path.Combine(Tridion.ContentManager.ConfigurationSettings.GetTcmHomeDirectory(), @"config\TcmEvents.config");

					if (!File.Exists(configurationPath))
						throw new Exception(String.Format("Configuration file \"{0}\" not found.", configurationPath));

					// Create config file map to point to the configuration file
					ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
					{
						ExeConfigFilename = configurationPath
					};

					// Create configuration object that contains the content of the custom configuration file
					return ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
				}

				return mConfiguration;
			}
		}
	}
}
