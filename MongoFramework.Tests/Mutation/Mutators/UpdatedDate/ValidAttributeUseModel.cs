﻿using System;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mutation.Mutators.UpdatedDate
{
	public class ValidAttributeUseModel
	{
		public string Id { get; set; }

		[UpdatedDate]
		public DateTime UpdatedDate { get; set; }
	}
}