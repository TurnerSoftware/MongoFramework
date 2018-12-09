using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoFramework.Tests
{
	public class ExpectedExceptionPatternAttribute : ExpectedExceptionBaseAttribute
	{
		private Type ExpectedExceptionType { get; }
		private Regex MessagePattern { get; }
		private string RawPattern { get; }

		public ExpectedExceptionPatternAttribute(Type expectedExceptionType, string exceptionMessagePattern)
		{
			ExpectedExceptionType = expectedExceptionType;
			MessagePattern = new Regex(exceptionMessagePattern);
			RawPattern = exceptionMessagePattern;
		}

		protected override void Verify(Exception exception)
		{
			Assert.IsNotNull(exception, $"\"{nameof(exception)}\" is null");

			var thrownExceptionType = exception.GetType();

			if (ExpectedExceptionType != thrownExceptionType)
			{
				throw new Exception($"Test method threw exception {thrownExceptionType.FullName}, but exception {ExpectedExceptionType.FullName} was expected. Exception message: {exception.Message}");
			}

			if (!MessagePattern.IsMatch(exception.Message))
			{
				throw new Exception($"Thrown exception message \"{exception.Message}\" does not match pattern \"{RawPattern}\".");
			}
		}
	}
}
