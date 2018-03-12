﻿using System;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mutation.Mutators.CreatedDate
{
	public class ValidAttributeUseModel
	{
		public string Id { get; set; }

		[CreatedDate]
		public DateTime CreatedDate { get; set; }
	}
}