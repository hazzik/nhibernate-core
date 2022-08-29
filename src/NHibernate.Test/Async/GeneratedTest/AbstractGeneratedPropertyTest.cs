﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;

using NUnit.Framework;

namespace NHibernate.Test.GeneratedTest
{
	using System.Threading.Tasks;
	[TestFixture]
	public abstract class AbstractGeneratedPropertyTestAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		[Test]
		public async Task GeneratedPropertyAsync()
		{
			GeneratedPropertyEntity entity = new GeneratedPropertyEntity();
			entity.Name = "entity-1";
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.SaveAsync(entity));
			await (s.FlushAsync());
			Assert.IsNotNull(entity.LastModified, "no timestamp retrieved");
			await (t.CommitAsync());
			s.Close();

			byte[] bytes = entity.LastModified;

			s = OpenSession();
			t = s.BeginTransaction();
			entity = (GeneratedPropertyEntity) await (s.GetAsync(typeof(GeneratedPropertyEntity), entity.Id));
			Assert.IsTrue(NHibernateUtil.Binary.IsEqual(bytes, entity.LastModified));
			await (t.CommitAsync());
			s.Close();

			Assert.IsTrue(NHibernateUtil.Binary.IsEqual(bytes, entity.LastModified));

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.DeleteAsync(entity));
			await (t.CommitAsync());
			s.Close();
		}
	}
}
