using System;

namespace MongoFramework.Infrastructure.Diagnostics
{
	public interface IDiagnosticListener : IObserver<DiagnosticCommand>
	{
	}
}
