using System;
using NHibernate.Engine;
using NHibernate.Hql.Ast;
using NHibernate.Metadata;
using NHibernate.Type;
using Remotion.Linq.EagerFetching;

namespace NHibernate.Linq.Visitors.ResultOperatorProcessors
{
    public class ProcessFetch
    {
	    public void Process(
		    FetchRequestBase resultOperator,
		    QueryModelVisitor queryModelVisitor,
		    IntermediateHqlTree tree)
	    {
		    var querySource = QuerySourceLocator.FindQuerySource(
			    queryModelVisitor.Model,
			    resultOperator.RelationMember.DeclaringType);

		    var sessionFactory = queryModelVisitor.VisitorParameters.SessionFactory;

		    Process(
			    resultOperator,
			    queryModelVisitor,
			    tree,
			    tree.TreeBuilder.Ident(querySource.ItemName),
			    new EntityPropertyTypeProvider(sessionFactory.GetClassMetadata(querySource.ItemType.FullName)),
			    sessionFactory);
	    }

	    public void Process(
		    FetchRequestBase resultOperator,
		    QueryModelVisitor queryModelVisitor,
		    IntermediateHqlTree tree,
		    HqlExpression source,
		    IPropertyTypeProvider propertyTypeProvider,
		    ISessionFactoryImplementor sessionFactory)
	    {
		    var propertyName = resultOperator.RelationMember.Name;
		    var type = propertyTypeProvider.GetPropertyType(propertyName);
		    if (type.IsComponentType)
		    {
			    var componentType = (IAbstractComponentType) type;
			    var ptp = new ComponentPropertyTypeProvider(componentType);

			    foreach (var innerFetch in resultOperator.InnerFetchRequests)
			    {
				    var join = tree.TreeBuilder.Dot(
					    source,
					    tree.TreeBuilder.Ident(propertyName));

				    Process(
					    innerFetch,
					    queryModelVisitor,
					    tree,
					    join,
					    ptp,
					    sessionFactory);
			    }
		    }
		    else if (type.IsAssociationType)
		    {
			    var entityType = (IAssociationType) type;

			    var ptp = new EntityPropertyTypeProvider(
				    sessionFactory.GetClassMetadata(entityType.GetAssociatedEntityName(sessionFactory)));
			    
			    var alias = queryModelVisitor.Model.GetNewName("_");

			    var join = tree.TreeBuilder.Dot(
				    source,
				    tree.TreeBuilder.Ident(propertyName));

			    tree.AddFromClause(tree.TreeBuilder.LeftFetchJoin(join, tree.TreeBuilder.Alias(alias)));
			    tree.AddDistinctRootOperator();

			    foreach (var innerFetch in resultOperator.InnerFetchRequests)
			    {
				    Process(
					    innerFetch,
					    queryModelVisitor,
					    tree,
					    tree.TreeBuilder.Ident(alias),
					    ptp,
					    queryModelVisitor.VisitorParameters.SessionFactory);
			    }
		    }
		    else
		    {
			    throw new NotSupportedException();
		    }
	    }

	    public interface IPropertyTypeProvider
	    {
		    IType GetPropertyType(string property);
	    }

	    private class EntityPropertyTypeProvider : IPropertyTypeProvider
	    {
		    private readonly IClassMetadata _classMetadata;

		    public EntityPropertyTypeProvider(IClassMetadata classMetadata)
		    {
			    _classMetadata = classMetadata;
		    }

		    public IType GetPropertyType(string property)
		    {
			    return _classMetadata.GetPropertyType(property);
		    }
	    }

	    private class ComponentPropertyTypeProvider: IPropertyTypeProvider
	    {
		    private readonly IAbstractComponentType _type;

		    public ComponentPropertyTypeProvider(IAbstractComponentType type)
		    {
			    _type = type;
		    }

		    public IType GetPropertyType(string property)
		    {
			    var index = Array.FindIndex(_type.PropertyNames, n => n == property);
			    return _type.Subtypes[index];
		    }
	    }
    }
}