namespace NHibernate.Test.NHSpecificTest.NH945
{
    public class UnionMatcher : IMatcher
    {
        private Group _groupA;
        private Group _groupB;

        public virtual Group GroupA
        {
            get { return _groupA; }
            set { _groupA = value; }
        }

        public virtual Group GroupB
        {
            get { return _groupB; }
            set { _groupB = value; }
        }
    }
}