using System;
using System.Collections.Generic;

namespace NHibernate.Engine.Query
{
	/// <summary> 
	/// Extends an HQLQueryPlan to maintain a reference to the collection-role name
	/// being filtered. 
	/// </summary>
	[Serializable]
	public class FilterQueryPlan : HQLExpressionQueryPlan
	{
		private readonly string collectionRole;

		public FilterQueryPlan(string hql, IQueryExpression queryExpression, string collectionRole, bool shallow, 
							   IDictionary<string, IFilter> enabledFilters, ISessionFactoryImplementor factory)
			: base(hql, queryExpression, collectionRole, shallow, enabledFilters, factory)
		{
			this.collectionRole = collectionRole;
		}

		public string CollectionRole
		{
			get { return collectionRole; }
		}
	}
}