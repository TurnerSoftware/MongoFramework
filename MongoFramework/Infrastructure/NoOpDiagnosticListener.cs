using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure
{
	public class NoOpDiagnosticListener : IDiagnosticListener
	{
		public void OnCompleted() { }

		public void OnError(Exception error) { }

		public void OnNext(DiagnosticCommand value) { }
	}
}
