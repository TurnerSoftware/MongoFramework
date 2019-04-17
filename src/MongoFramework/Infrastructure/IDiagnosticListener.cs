using System;

namespace MongoFramework.Infrastructure
{
	public interface IDiagnosticListener : IObserver<DiagnosticCommand>
	{
		
	}
}
