#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: Identifiable Object Extensions
// ---------------------------------------------------------------------------------
//	Date Created	: January 28, 2014
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;

namespace TcmEvents.Extensions
{
	/// <summary>
	/// Extensions for <see cref="T:Tridion.ContentManager.IdentifiableObject" />
	/// </summary>
	public static class IdentifiableObjectExtensions
	{
		/// <summary>
		/// Returns the Tcm id including the version if its a <see cref="T:Tridion.ContentManager.ContentManagement.VersionedItem" />
		/// </summary>
		/// <param name="identifiableObject"><see cref="T:Tridion.ContentManager.IdentifiableObject" /></param>
		/// <returns>Tcm id including version or Tcm id</returns>
		public static String VersionedItemId(this IdentifiableObject identifiableObject)
		{
			VersionedItem versionedItem = identifiableObject as VersionedItem;

			if (versionedItem != null && versionedItem.Id.IsVersionless)
				return String.Format("{0}-v{1}", versionedItem.Id.ToString(), versionedItem.Version);

			return identifiableObject.Id.ToString();
		}
	}
}
