namespace NHibernate.Test.NHSpecificTest.NH945
{
    public class Group
    {
        private int _id;
        private IMatcher _matcher;
        private string _name;

        public virtual int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual IMatcher Matcher
        {
            get { return _matcher; }
            set { _matcher = value; }
        }
    }
}