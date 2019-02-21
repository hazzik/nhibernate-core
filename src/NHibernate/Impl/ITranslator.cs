using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Engine.Query.Sql;
using NHibernate.Hql;
using NHibernate.Loader.Custom;
using NHibernate.Loader.Custom.Sql;
using NHibernate.Type;

namespace NHibernate.Impl
{
	public interface ITranslator
	{
		Loader.Loader Loader { get; }
		IType[] ReturnTypes { get; }
		string[] ReturnAliases { get; }
		ICollection<string> QuerySpaces { get; }
	}

	internal class HqlTranslatorWrapper : ITranslator
	{
		private readonly IQueryTranslator innerTranslator;

		public HqlTranslatorWrapper(IQueryTranslator translator)
		{
			innerTranslator = translator;
		}

		public Loader.Loader Loader
		{
			get { return innerTranslator.Loader; }
		}

		public IType[] ReturnTypes
		{
			get { return innerTranslator.ActualReturnTypes; }
		}

		public ICollection<string> QuerySpaces
		{
			get { return innerTranslator.QuerySpaces; }
		}

		public string[] ReturnAliases
		{
			get { return innerTranslator.ReturnAliases; }
		}
	}

	internal class SqlTranslator : ITranslator
	{
		private readonly CustomLoader loader;

		public SqlTranslator(ISQLQuery sqlQuery, ISessionFactoryImplementor sessionFactory)
		{
			var sqlQueryImpl = (SqlQueryImpl) sqlQuery;
			NativeSQLQuerySpecification sqlQuerySpec = sqlQueryImpl.GenerateQuerySpecification(sqlQueryImpl.NamedParams);
			var sqlCustomQuery = new SQLCustomQuery(sqlQuerySpec.SqlQueryReturns, sqlQuerySpec.QueryString, sqlQuerySpec.QuerySpaces, sessionFactory);
			loader = new CustomLoader(sqlCustomQuery, sessionFactory);
		}

		public IType[] ReturnTypes
		{
			get { return loader.ResultTypes; }
		}

		public Loader.Loader Loader
		{
			get { return loader; }
		}

		public ICollection<string> QuerySpaces
		{
			get { return loader.QuerySpaces; }
		}

		public string[] ReturnAliases
		{
			get { return loader.ReturnAliases; }
		}
	}
}
