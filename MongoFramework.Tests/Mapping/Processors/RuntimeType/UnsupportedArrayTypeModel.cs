﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoFramework.Attributes;

namespace MongoFramework.Tests.Mapping.Processors.RuntimeType
{
	public class UnsupportedArrayTypeModel
	{
		public string Id { get; set; }

		[RuntimeTypeDiscovery]
		public LinkedList<KnownBaseModel> UnsupportedList { get; set; }
	}
}
