using NHibernate.Engine;
using NHibernate.Hql.Ast.ANTLR.Tree;

namespace NHibernate
{
    public interface IQueryExpression
    {
        IASTNode Translate(ISessionFactoryImplementor sessionFactory);
        string Key { get; }
        System.Type Type { get; }
    }
}