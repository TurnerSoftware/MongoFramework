using System;
using System.Collections.Generic;
using System.Text;

namespace MongoFramework.Infrastructure
{
	public interface IDiagnosticListener : IObserver<DiagnosticCommand>
	{
		
	}
}
