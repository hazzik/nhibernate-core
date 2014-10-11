using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2658
{
	public static class ObjectExtensions
	{
		public static T GetProperty<T>(this object o, string propertyName)
		{
			//no implementation for this test
			throw new NotImplementedException();
		}
	}

	[TestFixture]
	public class Fixture : BugTestCase
	{
		public class DynamicPropertyGenerator : BaseHqlGeneratorForMethod
		{
			public DynamicPropertyGenerator()
			{
				//just registering for string here, but in a real implementation we'd be doing a runtime generator
				SupportedMethods = new[] { ReflectionHelper.GetMethodDefinition(() => ObjectExtensions.GetProperty<string>(null, null)) };
			}

			public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, 
				ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
			{
				var propertyName = (string) ((ConstantExpression) arguments[1]).Value;

				return treeBuilder.Dot(
						visitor.Visit(arguments[0]).AsExpression(),
						treeBuilder.Ident(propertyName)).AsExpression();
			}
		}

		protected override void BuildSessionFactory()
		{
			base.BuildSessionFactory();
		
			//add our linq extension
			sessions.Settings.LinqToHqlGeneratorsRegistry.Merge(new DynamicPropertyGenerator());
		}

		[Test]
		public void Does_Not_Cache_NonParameters()
		{
			using (var session = OpenSession())
			{
				//PASSES
				using (var spy = new SqlLogSpy())
				{
					//Query by name
					var products =
						(from p in session.Query<Product>() where p.GetProperty<string>("Name") == "Value" select p).ToList();

					Assert.That(spy.GetWholeLog(), Is.StringContaining("Name=@p0"));
				}

				//FAILS
				//Because this query is considered the same as the top query the hql will be reused from the top statement
				//Even though GetProperty has a parameter that never get passed to sql or hql
				using (var spy = new SqlLogSpy())
				{
					//Query by description
					var products =
						(from p in session.Query<Product>() where p.GetProperty<string>("Description") == "Value" select p).ToList();

					Assert.That(spy.GetWholeLog(), Is.StringContaining("Description=@p0"));
				}               
			}
		}
	}
}
