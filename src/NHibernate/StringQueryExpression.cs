using NHibernate.Engine;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Hql.Ast.ANTLR.Tree;

namespace NHibernate
{
    class StringQueryExpression : IQueryExpression
    {
        public StringQueryExpression(string key)
        {
            Key = key;
        }

        public IASTNode Translate(ISessionFactoryImplementor factory, bool filter)
        {
            return new HqlParseEngine(Key, filter, factory).Parse();
        }

        public string Key { get; private set; }

        public System.Type Type
        {
            get { return typeof (object); }
        }
    }
}