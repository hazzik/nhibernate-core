using System;
using System.Collections;

using Iesi.Collections;

namespace NHibernate.Test.NHSpecificTest.NH387
{
    public class _AssignedIdObject
	{
		private int _id;
        private string _text;
		private IList _children;

		public int _Id
		{
			get { return _id; }
			set { _id = value; }
		}

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

		public IList _Children
		{
			get { return _children; }
			set { _children = value; }
		}
	}
}
