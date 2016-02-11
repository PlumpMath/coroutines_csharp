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
		public void TestCase_CanExecuteOne_OneAtATime ()
		{
			// Given
			int count = 0;
			Func<int> func =  () =>  ++count;
			Func<int> func2 = () => ++count;

			IEnumerator<Job> jobs = func.AsCoRoutine ()
				.ContinueWith (func2)
				.ToJobs ();

			// When
			Job firstJob = jobs.DoWork ();
	
			// Then
			Assert.AreEqual (1, count);
			Assert.AreEqual (1, firstJob.Result);
			Assert.AreEqual (Progress.InProgress, firstJob.Progress);

			// When
			Job secondJob = jobs.DoWork();

			// Then
			Assert.AreEqual (2, count);
			Assert.AreEqual (2, secondJob.Result);
			Assert.AreEqual (Progress.InProgress, secondJob.Progress);

			// When
			Job nullableJob = jobs.DoWork();
	
			// Then
			Assert.AreEqual (2, count); // There is no fnal job, the count should not increase
			Assert.AreEqual (Job.NONE, nullableJob.Result);
			Assert.AreEqual (Progress.Done, nullableJob.Progress);
		}

		[Test ()]
		public void TestCase_CanExecute_All ()
		{
			// Given
			int count = 0;
			Func<int> func =  () => ++count;
			Func<int> func2 = () => ++count;

			// When
			func.AsCoRoutine ()
				.ContinueWith (func2)
				.ToJobs ()
				.DoAllWork ();

			// Then
			Assert.AreEqual (2, count);
		}

		[Test ()]
		public void TestCase_OnlyExecutes_OnIteration ()
		{
			// Given
			int count = 0;
			Func<int> func =  () => ++count;
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

