using NUnit.Framework;
using System;
using Coroutines;
using System.Collections.Generic;

namespace Coroutines.Test
{
	[TestFixture ()]
	public class CoRoutinesTest
	{
		[Test ()]
		public void TestCaseCanExecute ()
		{
			// Given
			int count = 0;
			Func<object> func =  () =>  { return ++count; };

			Func<int> func2 = () => ++count;

			// When
			func.AsCoRoutine ()
				.ContinueWith (func2)
				.ToJobs ()
				.DoWork ();
			// Then
			Assert.AreEqual (2, count);
		}

		[Test ()]
		public void TestOnlyExecuteOnIteration ()
		{
			// Given
			int count = 0;
			Func<object> func =  () =>  { return ++count; };
			Func<int> func2 = () => ++count;

			// When
			func.AsCoRoutine ()
				.ContinueWith (func2)
				.ToJobs ();

			// Then
			Assert.AreEqual (0, count);
		}
	}
}

