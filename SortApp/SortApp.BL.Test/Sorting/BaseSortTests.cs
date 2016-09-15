﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SortApp.BL.Sortings;

namespace SortApp.BL.Test.Sorting
{
	[TestClass]
	public abstract class BaseSortTests
	{
		/// <summary>
		/// Creates implementation with specific sorting algorithm.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		protected abstract Sorter<int> CreateSorter();

		/// <summary>
		/// Performs a base logic for test towards the logics of sorting algorithm when incoming array is empty.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		[TestMethod]
		public void SortEmptyArray()
		{
			int[] originalArray = { };
			int[] expectedArray = { };

			Sorter<int> sorter = this.CreateSorter();
			sorter.Sort(originalArray);

			CollectionAssert.AreEqual(expectedArray, originalArray);
		}

		/// <summary>
		/// Performs a base logic for test towards the logics of sorting algorithm when incoming array is filled.
		/// </summary>
		/// <owner>Oleh Petrenko</owner>
		[TestMethod]
		public void SortFilledArray()
		{
			int[] originalArray = { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
			int[] expectedArray = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			Sorter<int> sorter = this.CreateSorter();
			sorter.Sort(originalArray);

			CollectionAssert.AreEqual(expectedArray, originalArray);
		}
	}
}
