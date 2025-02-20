using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SIL.Code
{
	/// <summary>
	/// This utility can be used to wrap any call and retry it in a loop
	/// until specific exceptions no longer occur.
	///
	/// Number of retry attempts, pause time between attempts, and exception types
	/// can all be specified or left to the defaults.
	///
	/// This class came about as an attempt to mitigate certain IO issues, so the
	/// defaults are specified along those lines.
	/// </summary>
	public static class RetryUtility
	{
		// Nothing special about these numbers. Just attempting to balance issues of user experience.
		public const int kDefaultMaxRetryAttempts = 10;
		public const int kDefaultRetryDelay = 200;
		private static readonly ISet<Type> kDefaultExceptionTypesToRetry = new HashSet<Type> { Type.GetType("System.IO.IOException") };

		public static void Retry(Action action, int maxRetryAttempts = kDefaultMaxRetryAttempts, int retryDelay = kDefaultRetryDelay, ISet<Type> exceptionTypesToRetry = null, string memo = "")
		{
			Retry<object>(() =>
			{
				action();
				return null;
			}, maxRetryAttempts, retryDelay, exceptionTypesToRetry, memo);
		}

		public static T Retry<T>(Func<T> action, int maxRetryAttempts = kDefaultMaxRetryAttempts, int retryDelay = kDefaultRetryDelay, ISet<Type> exceptionTypesToRetry = null, string memo = "")
		{
			if (exceptionTypesToRetry == null)
				exceptionTypesToRetry = kDefaultExceptionTypesToRetry;

			for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
			{
				try
				{
					var result = action();
					//Debug.WriteLine("Successful after {0} attempts", attempt);
					return result;
				}
				catch (Exception e)
				{
					if (TypesIncludes(exceptionTypesToRetry, e.GetType()))
					{
						Debug.WriteLine($"Retry<{nameof(T)}> attempt {attempt}/{maxRetryAttempts}: {e.GetType().Name}   {memo}");
						if (attempt == maxRetryAttempts)
						{
							Debug.WriteLine($"Retry<{nameof(T)}> exceeded max attempts ({maxRetryAttempts}): {e.GetType().Name}   {memo}");
							throw;
						}
						Thread.Sleep(retryDelay);
						continue;
					}
					Debug.WriteLine($"Retry<{nameof(T)}> attempt {attempt}: Not retrying for this exception type {e.GetType().Name}   {memo}");
					throw;
				}
			}
			return default(T);
		}

		internal static bool TypesIncludes(ISet<Type> types, Type typeToTest)
		{
			if (types.Contains(typeToTest)) // fast check for root types
				return true;
			foreach (var type in types)
			{
				if (typeToTest.IsSubclassOf(type))
					return true;
			}

			return false;
		}
	}
}
