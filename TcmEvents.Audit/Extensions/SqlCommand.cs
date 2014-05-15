#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: SqlCommand Extensions
// ---------------------------------------------------------------------------------
//	Date Created	: January 28, 2014
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Data;
using System.Data.SqlClient;

namespace TcmEvents.Extensions
{
	/// <summary>
	/// Extension functions for <see cref="T:System.Data.SqlClient.SqlCommand" />
	/// </summary>
	public static class SqlCommandExtensions
	{
		/// <summary>
		/// Adds a <see cref="T:System.Data.SqlClient.SqlParameter" /> to the <see cref="T:System.Data.SqlClient.SqlCommand" />
		/// </summary>
		/// <param name="Command"><see cref="T:System.Data.SqlClient.SqlCommand" /></param>
		/// <param name="parameterName">Parameter name</param>
		/// <param name="dbType"><see cref="T:System.Data.SqlDbType" /></param>
		/// <param name="value">Value</param>
		public static void AddParameter(this SqlCommand command, String parameterName, SqlDbType dbType, Object value)
		{
			SqlParameter sqlParameter = new SqlParameter(parameterName, dbType);
			sqlParameter.Value = value != null ? value : DBNull.Value;
			command.Parameters.Add(sqlParameter);
		}

		/// <summary>
		/// Adds a <see cref="T:System.Data.SqlClient.SqlParameter" /> to the <see cref="T:System.Data.SqlClient.SqlCommand" />
		/// </summary>
		/// <param name="Command"><see cref="T:System.Data.SqlClient.SqlCommand" /></param>
		/// <param name="parameterName">Parameter name</param>
		/// <param name="dbType"><see cref="T:System.Data.SqlDbType" /></param>
		/// <param name="value">Value</param>
		public static void AddParameter(this SqlCommand command, String parameterName, SqlDbType dbType)
		{
			command.AddParameter(parameterName, dbType, null);
		}

		/// <summary>
		/// Sets a value for the <see cref="T:System.Data.SqlClient.SqlParameter" /> of the  <see cref="T:System.Data.SqlClient.SqlCommand" />
		/// </summary>
		/// <param name="command" <see cref="T:System.Data.SqlClient.SqlCommand" /></param>
		/// <param name="parameterName">Parameter Name</param>
		/// <param name="value">Value.</param>
		public static void SetValue(this SqlCommand command, String parameterName, Object value)
		{
			SqlParameter sqlParameter = command.Parameters[parameterName];

			if (sqlParameter != null)
				sqlParameter.Value = value != null ? value : DBNull.Value;
		}
	}
}
