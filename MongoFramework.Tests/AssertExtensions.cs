using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoFramework.Tests
{
	public static class AssertExtensions
	{
		/// <summary>
		/// Author: Gilles Leblanc
		/// Source: https://gillesleblanc.wordpress.com/2014/03/17/testing-that-an-exception-isnt-thrown-in-c/
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expressionUnderTest"></param>
		/// <param name="exceptionMessage"></param>
		public static void DoesNotThrow<T>(Action expressionUnderTest,
			string exceptionMessage = "Expected exception was thrown by target of invocation.") where T : Exception
		{
			try
			{
				expressionUnderTest();
			}
			catch (T)
			{
				Assert.Fail(exceptionMessage);
			}
			catch (Exception)
			{
				Assert.IsTrue(true);
			}

			Assert.IsTrue(true);
		}

		/// <summary>
		/// Author: Gilles Leblanc
		/// Source: https://gillesleblanc.wordpress.com/2014/03/17/testing-that-an-exception-isnt-thrown-in-c/
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expressionUnderTest"></param>
		/// <param name="exceptionMessage"></param>
		public static async Task DoesNotThrowAsync<T>(Func<Task> expressionUnderTest,
			string exceptionMessage = "Expected exception was thrown by target of invocation.") where T : Exception
		{
			try
			{
				await expressionUnderTest();
			}
			catch (T)
			{
				Assert.Fail(exceptionMessage);
			}
			catch (Exception)
			{
				Assert.IsTrue(true);
			}

			Assert.IsTrue(true);
		}
	}
}