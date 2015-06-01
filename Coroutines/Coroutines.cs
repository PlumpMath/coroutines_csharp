using System;
using System.Collections.Generic;

namespace Coroutines
{
	public static class Extensions {

		/*
		 * Executes the next job in the pipeline, and returns it once it finishes.
		 * */
		public static Job DoWork(this IEnumerator<Job> jobs) {
			jobs.MoveNext ();
			return jobs.Current;
		}

		// Performs all of the work in the jobs
		public static IList<object> DoAllWork(this IEnumerator<Job> jobs) {
			IList<object> work = new List<object> ();
			while(jobs.MoveNext()) {
				work.Add (jobs.Current);
			}

			return work;
		}

		public static IEnumerator<Job> ToJobs(this IEnumerable<CoRoutine> coroutines) {
			return coroutines.GetJobs ().GetEnumerator ();
		}

		// Executes a list of coroutines, and returns all of their jobs
		// you must iterate over all the jobs in order for them to be
		// executed
		private static IEnumerable<Job> GetJobs(this IEnumerable<CoRoutine> coroutines) {
			// A batch is executed as a single unit
			// So a batch of batches would execute one complete batch at a time
			CoRoutineBatch batch = new CoRoutineBatch (coroutines);
			IEnumerator<Job> workInBatch = batch.execute ().GetEnumerator();
			do {
				// execute next coroutine
				workInBatch.MoveNext ();
				yield return workInBatch.Current;
			} while (workInBatch.Current.Progress != Progress.Done);
		}

		// Groups together a list of coroutines, so they are executed as a single batch,
		// this forces these coroutines to be executed as a single unit (all or none)
		public static IEnumerable<CoRoutine> Batch<T>(this IEnumerable<CoRoutine> coroutines) {
			yield return new CoRoutineBatch(coroutines);
		}

		public static IEnumerable<CoRoutine> ContinueWith<T>(this IEnumerable<CoRoutine> coroutines, Func<T> routine) {
			foreach (CoRoutine coroutine in coroutines) {
				yield return coroutine;
			}

			foreach (CoRoutine coroutine in CoRoutine<T>.coroutine(routine)) {
				yield return coroutine;
			}
		}

		public static IEnumerable<CoRoutine> AsCoRoutine<T>(this Func<T> routine) {
			return CoRoutine<T>.coroutine (routine);
		}
	}

	public enum Progress {
		Failed, Done, InProgress
	}

	public class Job {
		public static object NONE = new object ();

		public Progress Progress {
			get;
			private set;
		}

		public object Result {
			get;
			private set;
		}

		public static Job Done(object result) {
			return new Job { Progress = Progress.Done, Result = result };
		}

		public static Job InProgress(object result) {
			return new Job { Progress = Progress.InProgress, Result = result };
		}

		public static Job Failed(Exception exception) {
			return new Job { Progress = Progress.Failed, Result = exception };
		}
	}

	public interface CoRoutine {
		IEnumerable<Job> execute ();
	}

	class CoRoutine<T> : CoRoutine
	{
		private readonly Func<T> _routine;

		private CoRoutine (Func<T> routine)
		{
			this._routine = routine;
		}

		public static IEnumerable<CoRoutine> coroutine<U>(Func<U> routine) {
			yield return new CoRoutine<U> (routine);
		}

		public IEnumerable<Job> execute () {
			yield return Job.Done (this._routine());
		}
	}

	class CoRoutineBatch : CoRoutine {

		private readonly IEnumerable<CoRoutine> routines;

		public CoRoutineBatch(IEnumerable<CoRoutine> routines) {
			this.routines = routines;
		}

		#region CoRoutine implementation
		public IEnumerable<Job> execute ()
		{
			foreach(CoRoutine routine in routines) {
				foreach (Job job in routine.execute()) {
					yield return Job.InProgress (job.Result);
				}
			}

			yield return Job.Done (Job.NONE);
		}

		#endregion
	}
}

