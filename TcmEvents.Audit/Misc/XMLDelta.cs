#region Header
////////////////////////////////////////////////////////////////////////////////////
//
//	File Description: XML Delta
// ---------------------------------------------------------------------------------
//	Date Created	: January 28, 2014
//	Author			: Rob van Oostenrijk
//
////////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TcmEvents.Misc
{
	/// <summary>
	/// <see cref="XMLDelta" /> generates a difference between two <see cref="T:System.Xml.Linq.XElement" />.
	/// </summary> 
	/// <remarks>
	/// Adapted from an implementation found @ https://gist.github.com/gmoothart/1943265
	/// </remarks>
	public class XMLDelta
	{
		private class AttributeNameComparer : IComparer<XAttribute>
		{
			int IComparer<XAttribute>.Compare(XAttribute x, XAttribute y)
			{
				return x.Name.ToString().CompareTo(y.Name.ToString());
			}
		}

		private class ElementNameComparer : IComparer<XElement>
		{
			int IComparer<XElement>.Compare(XElement x, XElement y)
			{
				return x.Name.ToString().CompareTo(y.Name.ToString());
			}
		}

		/// <summary>
		/// Iterates two sequences, looking for duplicate and unique items.
		/// The supplied delegate is executed for each case (items are equal,
		/// item1 is unique, item2 is uniqu).
		/// 
		/// Sequences are assumed to be sorted by the same criteria used in 
		/// objComparer!
		/// </summary>
		private static void MergeSequences<T>(IEnumerable<T> seq1, IEnumerable<T> seq2,
			IComparer<T> objComparer, Action<T, T> equalFunc, Action<T> obj1UniqueFunc,
			Action<T> obj2UniqueFunc)
		{
			var seq1Enum = seq1.GetEnumerator();
			var seq2Enum = seq2.GetEnumerator();
			bool seq1HasMoreEls = seq1Enum.MoveNext();
			bool seq2HasMoreEls = seq2Enum.MoveNext();
			while (seq1HasMoreEls || seq2HasMoreEls)
			{
				var obj1 = seq1Enum.Current;
				var obj2 = seq2Enum.Current;

				// compare obj1 and obj2
				// if we are at the end of one sequence but not the other,
				// hard-code the value so we don't compare the last item
				// more than once
				int nameComparison;
				if (seq1HasMoreEls && seq2HasMoreEls)
					nameComparison = objComparer.Compare(obj1, obj2);
				else if (seq1HasMoreEls)
					nameComparison = -1;
				else /* seq2HasMoreEls */
					nameComparison = 1;

				// objects are equal
				if (nameComparison == 0)
				{
					equalFunc(obj1, obj2);

					// advance both
					seq1HasMoreEls = seq1Enum.MoveNext();
					seq2HasMoreEls = seq2Enum.MoveNext();
				}
				// obj1 is unique
				else if (nameComparison < 0)
				{
					obj1UniqueFunc(obj1);

					// advance seq1 elements
					seq1HasMoreEls = seq1Enum.MoveNext();
				}
				// obj2 is unique
				else /* (nameComparison > 0) */
				{
					obj2UniqueFunc(obj2);

					// advance seq2 elements
					seq2HasMoreEls = seq2Enum.MoveNext();
				}
			}
		}

		public static XElement Compare(XElement d1, XElement d2)
		{
			XElement xeResult = new XElement(d1.Name);

			//
			// Compare text if elements have no children
			//
			if (!d1.Elements().Any() && !d2.Elements().Any() && d1.Value != d2.Value)
			{
				xeResult.Add(new XComment(" <<< Original : Start "));
				xeResult.Add(new XText(d1.Value));
				xeResult.Add(new XComment(" <<< Original : End "));

				xeResult.Add(new XComment(" >>> Updated : Start "));
				xeResult.Add(new XText(d2.Value));
				xeResult.Add(new XComment(" >>> Updated : End "));
			}

			//
			// compare attributes
			//
			var attr1Enum = d1.Attributes().OrderBy(a => a.Name.ToString());
			var attr2Enum = d2.Attributes().OrderBy(a => a.Name.ToString());
			MergeSequences(attr1Enum, attr2Enum, new AttributeNameComparer(),
				equalFunc: (a1, a2) =>
				{
					if (a1.Value != a2.Value)
					{
						xeResult.SetAttributeValue(a1.Name + "_org", a1.Value);
						xeResult.SetAttributeValue(a2.Name + "_upd", a2.Value);
					}
				},
				obj1UniqueFunc: (a1) =>
				{
					xeResult.SetAttributeValue(a1.Name + "_org", a1.Value);
				},
				obj2UniqueFunc: (a2) =>
				{
					xeResult.SetAttributeValue(a2.Name + "_upd", a2.Value);
				}
			);

			//
			// compare children
			//
			var els1Enum = d1.Elements().OrderBy(el => el.Name.ToString());
			var els2Enum = d2.Elements().OrderBy(el => el.Name.ToString());
	
			MergeSequences(els1Enum, els2Enum, new ElementNameComparer(),
				equalFunc: (el1, el2) =>
				{
					xeResult.Add(Compare(el1, el2));
				},
				obj1UniqueFunc: (el1) =>
				{
					xeResult.AddFirst(new XComment(" <<< Original : Start "));
					xeResult.Add(el1);
					xeResult.Add(new XComment(" <<< Original : End "));
				},
				obj2UniqueFunc: (el2) =>
				{
					xeResult.AddFirst(new XComment(" >>> New : Start "));
					xeResult.Add(el2);
					xeResult.Add(new XComment(" >>> New : End "));
				}
			);

			// return null if nothing has been added to xeResult
			if (!xeResult.Attributes().Any() && !xeResult.Elements().Any() &&
				String.IsNullOrWhiteSpace(xeResult.Value))
			{
				return null;
			}
			return xeResult;
		}
	}
}
