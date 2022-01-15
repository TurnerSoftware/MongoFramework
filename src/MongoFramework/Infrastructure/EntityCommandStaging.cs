using System.Collections.Generic;
using MongoFramework.Infrastructure.Commands;

namespace MongoFramework.Infrastructure
{
	public class EntityCommandStaging
	{
		private HashSet<IWriteCommand> Commands { get; } = new HashSet<IWriteCommand>();

		public void Add(IWriteCommand command)
		{
			Commands.Add(command);
		}

		public void Clear()
		{
			Commands.Clear();
		}

		public IEnumerable<IWriteCommand> GetCommands()
		{
			return Commands;
		}

		internal void CommitChanges()
		{
			Clear();
		}
	}
}
