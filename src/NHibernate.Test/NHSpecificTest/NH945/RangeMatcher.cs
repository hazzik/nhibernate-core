namespace NHibernate.Test.NHSpecificTest.NH945
{
    public class RangeMatcher : IMatcher
    {
        private int _end;
        private int _start;

        public virtual int Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public virtual int End
        {
            get { return _end; }
            set { _end = value; }
        }
    }
}