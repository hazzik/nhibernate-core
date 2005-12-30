using System;
using System.Collections;

namespace NHibernate.Test.NHSpecificTest.NH517
{
	public class NH517Parent
	{
		#region fields
		public NH517ParentKey Identity
		{
			get
			{
				return nh517ParentKey;
			}
			set
			{
				nh517ParentKey = value;
			}
		}

		private NH517ParentKey nh517ParentKey = new NH517ParentKey();

		private IList children, income;

		public IList Children
		{
			get
			{
				return children;
			}
			set
			{
				children = value;
			}
		}

		public IList Income
		{
			get
			{
				return income;
			}
			set
			{
				income = value;
			}
		}
		#endregion

		public NH517Parent()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}

	public class NH517ParentKey
	{
		#region fields
		int parentID;

		public int ParentID
		{
			get
			{
				return parentID;
			}
			set
			{
				parentID = value;
			}
		}
		#endregion

		public NH517ParentKey()
		{
		}


		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			NH517ParentKey compareMe = (NH517ParentKey)obj;

			return this.parentID.Equals(compareMe.parentID);
		}



		public override int GetHashCode()
		{
			return parentID.GetHashCode();
		}
	}



	public class NH517Child
	{
		#region fields
		public NH517ChildKey Identity
		{
			get
			{
				return nh517ChildKey;
			}
			set
			{
				nh517ChildKey = value;
			}
		}

		private NH517ChildKey nh517ChildKey = new NH517ChildKey();

		private string name;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		private NH517Parent parent;

		public NH517Parent Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}
		#endregion

		public NH517Child()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}

	public class NH517ChildKey
	{
		#region fields
		int parentID, counter;

		public int ParentID
		{
			get
			{
				return parentID;
			}
			set
			{
				parentID = value;
			}
		}

		public int Counter
		{
			get
			{
				return counter;
			}
			set
			{
				counter = value;
			}
		}
		#endregion

		public NH517ChildKey()
		{
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			NH517ChildKey compareMe = (NH517ChildKey)obj;

			return this.counter.Equals(compareMe.Counter) && this.parentID.Equals(compareMe.parentID);
		}


		public override int GetHashCode()
		{
			return parentID.GetHashCode() ^ counter.GetHashCode();
		}
	}


	public class Income
	{
		#region fields
		public IncomeKey Identity
		{
			get
			{
				return incomeKey;
			}
			set
			{
				incomeKey = value;
			}
		}

		private IncomeKey incomeKey = new IncomeKey();
		#endregion

		public Income()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}

	public class IncomeKey
	{
		#region fields
		int incomeCategory;
		NH517Parent client;

		public int IncomeCategory
		{
			get
			{
				return incomeCategory;
			}
			set
			{
				incomeCategory = value;
			}
		}

		public NH517Parent Client
		{
			get
			{
				return client;
			}
			set
			{
				client = value;
			}
		}
		#endregion

		public IncomeKey()
		{
		}


		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			IncomeKey compareMe = (IncomeKey)obj;

			return this.incomeCategory.Equals(compareMe.IncomeCategory) && this.client.Equals(compareMe.Client);
		}


		public override int GetHashCode()
		{
			return incomeCategory.GetHashCode() ^ Client.Identity.GetHashCode();
		}
	}
}
