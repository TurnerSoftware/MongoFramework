using System;

namespace MongoFramework.Infrastructure.Diagnostics
{
	public class NoOpDiagnosticListener : IDiagnosticListener
	{
		public void OnCompleted() { /* No-Op */ }

		public void OnError(Exception error) { /* No-Op */ }

		public void OnNext(DiagnosticCommand value) { /* No-Op */ }
	}
}
