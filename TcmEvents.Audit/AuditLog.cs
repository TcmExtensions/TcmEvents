#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: TcmEvents AuditLog
// ---------------------------------------------------------------------------------
//	Date Created	: January 28, 2014
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Xml.Linq;
using TcmEvents.Extensions;
using TcmEvents.Misc;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Publishing.Resolving;
using Tridion.Logging;

namespace TcmEvents
{
	/// <summary>
	/// <see cref="AuditLog" /> is a event system TcmExtension which tracks Tridion activity in an audit database
	/// </summary>
	[TcmExtension("AuditLog")]
	public class AuditLog : TcmExtension
	{
		private String mDatabase;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuditLog"/> class.
		/// </summary>
		public AuditLog()
		{
			if (!String.IsNullOrEmpty(AuditConfiguration.Instance.Database.ConnectionString))
			{
				mDatabase = AuditConfiguration.Instance.Database.ConnectionString;

				// Audit any publish transactions
				EventSystem.SubscribeAsync<PublishTransaction, SaveEventArgs>(AuditPublish, EventPhases.TransactionCommitted);

				// Audit localizations
				EventSystem.SubscribeAsync<RepositoryLocalObject, LocalizeEventArgs>(AuditLocalize, EventPhases.TransactionCommitted);

				// Audit UnLocalizations
				EventSystem.SubscribeAsync<RepositoryLocalObject, UnLocalizeEventArgs>(AuditUnLocalize, EventPhases.TransactionCommitted);

				// Audit object deletions and object version deletions
				EventSystem.SubscribeAsync<RepositoryLocalObject, DeleteEventArgs>(AuditDelete, EventPhases.TransactionCommitted);

				// Audit version purges
				EventSystem.SubscribeAsync<RepositoryLocalObject, PurgeOldVersionsEventArgs>(AuditPurgeOldVersions, EventPhases.TransactionCommitted);

				// Audit version rollbacks
				EventSystem.SubscribeAsync<RepositoryLocalObject, RollbackEventArgs>(AuditRollback, EventPhases.TransactionCommitted);

				// Audit updates to versionless items
				EventSystem.SubscribeAsync<Publication, SaveEventArgs>(AuditVersionless, EventPhases.Initiated | EventPhases.TransactionCommitted);
				EventSystem.SubscribeAsync<Keyword, SaveEventArgs>(AuditVersionless, EventPhases.Initiated | EventPhases.TransactionCommitted);
				EventSystem.SubscribeAsync<Folder, SaveEventArgs>(AuditVersionless, EventPhases.Initiated | EventPhases.TransactionCommitted);
				EventSystem.SubscribeAsync<StructureGroup, SaveEventArgs>(AuditVersionless, EventPhases.Initiated | EventPhases.TransactionCommitted);
				EventSystem.SubscribeAsync<Category, SaveEventArgs>(AuditVersionless, EventPhases.Initiated | EventPhases.TransactionCommitted);

				Logger.Write("Initialized successfully.", "TcmEvents.Audit", LoggingCategory.General, TraceEventType.Information);
			}
			else
				Logger.Write("No database configuration found.", "TcmEvents.Audit", LoggingCategory.General, TraceEventType.Warning);
		}

		/// <summary>
		/// Returns a <see cref="T:System.Data.SqlClient.SqlConnection" /> to the auditing database
		/// </summary>
		/// <value>
		/// <see cref="T:System.Data.SqlClient.SqlConnection" />
		/// </value>
		private SqlConnection Connection
		{
			get
			{
				SqlConnection connection = new SqlConnection(mDatabase);
				connection.Open();

				return connection;
			}
		}

		/// <summary>
		/// Audits the content edit.
		/// </summary>
		/// <param name="action">Content Editing Action</param>
		/// <param name="identifiableObject">The identifiable object.</param>
		/// <param name="comments">Optional comments.</param>
		/// <param name="xml">Optional Xml blob.</param>
		private void AuditContentEdit(String action, IdentifiableObject identifiableObject, String comments, SqlXml xml)
		{
			try
			{
				using (SqlConnection connection = Connection)
				{
					// Register the content editing action
					using (SqlCommand sqlAuditContentEdit = new SqlCommand()
					{
						CommandText = "AuditContentEdit",
						CommandType = CommandType.StoredProcedure,
						Connection = connection
					})
					{					
						sqlAuditContentEdit.AddParameter("@Action", SqlDbType.VarChar, action);
						sqlAuditContentEdit.AddParameter("@ItemID", SqlDbType.VarChar, identifiableObject.VersionedItemId());
						sqlAuditContentEdit.AddParameter("@ItemTitle", SqlDbType.NVarChar, identifiableObject.Title);
						sqlAuditContentEdit.AddParameter("@Username", SqlDbType.VarChar, identifiableObject.Session.User.Title.ToUpper());
						sqlAuditContentEdit.AddParameter("@UserDescription", SqlDbType.NVarChar, identifiableObject.Session.User.Description);

						if (!String.IsNullOrEmpty(comments))
							sqlAuditContentEdit.AddParameter("@Comments", SqlDbType.NVarChar, comments);
						else
							sqlAuditContentEdit.AddParameter("@Comments", SqlDbType.NVarChar, DBNull.Value);
						
						if (xml != null)
							sqlAuditContentEdit.AddParameter("@Xml", SqlDbType.Xml, xml);
						else
							sqlAuditContentEdit.AddParameter("@Xml", SqlDbType.Xml, DBNull.Value);

						sqlAuditContentEdit.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Write(ex, "TcmEvents.Audit", LoggingCategory.General, TraceEventType.Error);
			}
		}

		/// <summary>
		/// Audits a publishing transaction
		/// </summary>
		/// <param name="transaction"><see cref="T:Tridion.ContentManager.Publishing.PublishTransaction" /></param>
		/// <param name="args">The <see cref="SaveEventArgs"/> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditPublish(PublishTransaction transaction, SaveEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.TransactionCommitted && transaction.State == PublishTransactionState.Success)
			{
				try
				{
					using (SqlConnection connection = Connection)
					{
						// Register the publish transaction
						using (SqlCommand sqlAuditPublishTransaction = new SqlCommand()
						{
							CommandText = "AuditPublishTransaction",
							CommandType = CommandType.StoredProcedure,
							Connection = connection
						})
						{
							sqlAuditPublishTransaction.AddParameter("@Transaction", SqlDbType.VarChar, transaction.Id.ToString());
							sqlAuditPublishTransaction.AddParameter("@Action", SqlDbType.VarChar, (transaction.Instruction is PublishInstruction ? "Publish" : "Unpublish"));
							sqlAuditPublishTransaction.AddParameter("@TimeStamp", SqlDbType.DateTime, transaction.StateChangeDateTime);
							sqlAuditPublishTransaction.AddParameter("@Username", SqlDbType.VarChar, transaction.Creator.Title.ToUpper());
							sqlAuditPublishTransaction.AddParameter("@UserDescription", SqlDbType.NVarChar, transaction.Creator.Description);

							int transactionId = Convert.ToInt32(sqlAuditPublishTransaction.ExecuteScalar());

							if (transactionId > 0)
							{
								using (SqlCommand sqlAuditPublishedItem = new SqlCommand()
								{
									CommandText = "AuditPublishedItem",
									CommandType = CommandType.StoredProcedure,
									Connection = connection
								})
								{
									// Register the publication transaction
									sqlAuditPublishedItem.AddParameter("@TransactionID", SqlDbType.Int, transactionId);
									sqlAuditPublishedItem.AddParameter("@PublicationTarget", SqlDbType.VarChar);
									sqlAuditPublishedItem.AddParameter("@Publication", SqlDbType.VarChar);
									sqlAuditPublishedItem.AddParameter("@ItemID", SqlDbType.VarChar);
									sqlAuditPublishedItem.AddParameter("@ItemTitle", SqlDbType.NVarChar);
									sqlAuditPublishedItem.AddParameter("@ItemTemplate", SqlDbType.VarChar);
									sqlAuditPublishedItem.AddParameter("@IsComponentTemplate", SqlDbType.Bit);
									sqlAuditPublishedItem.AddParameter("@IsDCP", SqlDbType.Bit);

									foreach (PublishContext publishContext in transaction.PublishContexts)
									{
										foreach (ProcessedItem processedItem in publishContext.ProcessedItems)
										{
											// Register each published item
											ResolvedItem resolvedItem = processedItem.ResolvedItem;

											sqlAuditPublishedItem.SetValue("@PublicationTarget", publishContext.PublicationTarget.Id.ToString());
											sqlAuditPublishedItem.SetValue("@Publication", publishContext.Publication.Id.ToString());
											sqlAuditPublishedItem.SetValue("@ItemID", resolvedItem.Item.VersionedItemId());
											sqlAuditPublishedItem.SetValue("@ItemTitle", resolvedItem.Item.Title);
											sqlAuditPublishedItem.SetValue("@ItemTemplate", resolvedItem.Template.VersionedItemId());
											sqlAuditPublishedItem.SetValue("@IsComponentTemplate", resolvedItem.IsComponentPresentation);
											sqlAuditPublishedItem.SetValue("@IsDCP", resolvedItem.IsDynamicComponentPresentation);

											sqlAuditPublishedItem.ExecuteNonQuery();
										}
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Write(ex, "TcmEvents.Audit", LoggingCategory.General, TraceEventType.Error);
				}
			}
		}

		/// <summary>
		/// Audits object localization.
		/// </summary>
		/// <param name="repositoryObject"><see cref="T:Tridion.ContentManager.ContentManagement.RepositoryLocalObject" /></param>
		/// <param name="args">The <see cref="LocalizeEventArgs" /> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditLocalize(RepositoryLocalObject repositoryObject, LocalizeEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.TransactionCommitted)
				AuditContentEdit("Localize", repositoryObject, null, new SqlXml(repositoryObject.ToXml().CreateNavigator().ReadSubtree()));
		}

		/// <summary>
		/// Audits the object unlocalizations
		/// </summary>
		/// <param name="repositoryObject"><see cref="T:Tridion.ContentManager.ContentManagement.RepositoryLocalObject" /></param>
		/// <param name="args">The <see cref="UnLocalizeEventArgs" /> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditUnLocalize(RepositoryLocalObject repositoryObject, UnLocalizeEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.TransactionCommitted)
				AuditContentEdit("UnLocalize", repositoryObject, null, new SqlXml(repositoryObject.ToXml().CreateNavigator().ReadSubtree()));
		}

		/// <summary>
		/// Audits object or version deletions
		/// </summary>
		/// <param name="repositoryObject"><see cref="T:Tridion.ContentManager.ContentManagement.RepositoryLocalObject" /></param>
		/// <param name="args">The <see cref="DeleteEventArgs" /> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditDelete(RepositoryLocalObject repositoryObject, DeleteEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.TransactionCommitted)
				// Discern between deleting objects and deleting object versions
				if (repositoryObject.Id.IsVersionless)
					AuditContentEdit("Delete", repositoryObject, null, new SqlXml(repositoryObject.ToXml().CreateNavigator().ReadSubtree()));
				else
					AuditContentEdit("DeleteVersion", repositoryObject, null, new SqlXml(repositoryObject.ToXml().CreateNavigator().ReadSubtree()));
		}

		/// <summary>
		/// Audits the purging of old versions
		/// </summary>
		/// <param name="repositoryObject"><see cref="T:Tridion.ContentManager.ContentManagement.RepositoryLocalObject" /></param>
		/// <param name="args">The <see cref="PurgeOldVersionsEventArgs" /> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditPurgeOldVersions(RepositoryLocalObject repositoryObject, PurgeOldVersionsEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.TransactionCommitted)
				AuditContentEdit("PurgeVersions", repositoryObject, null, null);
		}

		/// <summary>
		/// Audits object version rollbacks.
		/// </summary>
		/// <param name="repositoryObject"><see cref="T:Tridion.ContentManager.ContentManagement.RepositoryLocalObject" /></param>
		/// <param name="args">The <see cref="RollbackEventArgs" /> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditRollback(RepositoryLocalObject repositoryObject, RollbackEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.TransactionCommitted)
				AuditContentEdit("Rollback", repositoryObject, String.Format("Delete Versions: {0}", args.DeleteVersions), new SqlXml(repositoryObject.ToXml().CreateNavigator().ReadSubtree()));
		}

		/// <summary>
		/// Audits updates to versionless items
		/// </summary>
		/// <param name="repositoryObject"><see cref="T:Tridion.ContentManager.ContentManagement.RepositoryLocalObject" /></param>
		/// <param name="args">The <see cref="SaveEventArgs" /> instance containing the event data.</param>
		/// <param name="phase"><see cref="T:Tridion.ContentManager.Extensibility.EventPhases" /></param>
		private void AuditVersionless(IdentifiableObject identifiableObject, SaveEventArgs args, EventPhases phase)
		{
			if (phase == EventPhases.Initiated)
			{
				try
				{
					// Load the original unmodified item XML from the database
					IdentifiableObject oldIdentifiableObject = identifiableObject.Session.GetObject(identifiableObject.Id);

					XElement original = XElement.Parse(oldIdentifiableObject.ToXml().OuterXml);
					XElement updated = XElement.Parse(identifiableObject.ToXml().OuterXml);

					XElement difference = XMLDelta.Compare(original, updated);

					if (difference != null)
						AuditContentEdit("Update", identifiableObject, identifiableObject.GetType().Name, new SqlXml(difference.CreateReader()));
					else
						AuditContentEdit("Update", identifiableObject, identifiableObject.GetType().Name, null);
				}
				catch (Exception ex)
				{
					Logger.Write(ex, "TcmEvents.Audit", LoggingCategory.General, TraceEventType.Error);
				}
			}
		}
	}
}
