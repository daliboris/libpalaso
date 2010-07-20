﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;

namespace Palaso.DictionaryServices.Queries
{
	class GuidQuery:IQuery<LexEntry>
	{
		public IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			tokenFieldsAndValues.Add("Guid", entryToQuery.Guid);
			return new[] { tokenFieldsAndValues };
		}

		public SortDefinition[] SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Guid", Comparer<Guid>.Default);
				return sortOrder;
			}
		}

		public string Label
		{
			get { return "GuidQuery"; }
		}
	}
}
