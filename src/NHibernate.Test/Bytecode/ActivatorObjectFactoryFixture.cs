using System;
using NHibernate.Bytecode;
using NUnit.Framework;

namespace NHibernate.Test.Bytecode
{
	[TestFixture]
	public class ActivatorObjectFactoryFixture
	{
		public class WithOutPublicParameterLessCtor
		{
			public string Something { get; set; }
			protected WithOutPublicParameterLessCtor() { }

			public WithOutPublicParameterLessCtor(string something)
			{
				Something = something;
			}
		}

		public class PublicParameterLessCtor
		{
		}

		public struct ValueType
		{
			public string Something { get; set; }
		}

		protected virtual IObjectsFactory GetObjectsFactory()
		{
			return new ActivatorObjectsFactory();
		}

		[Test]
		public void CreateInstanceDefCtor()
		{
			IObjectsFactory of = GetObjectsFactory();
			Assert.Throws<ArgumentNullException>(() => of.CreateInstance(null));
			Assert.Throws<MissingMethodException>(() => of.CreateInstance(typeof(WithOutPublicParameterLessCtor)));
			var instance = of.CreateInstance(typeof(PublicParameterLessCtor));
			Assert.That(instance, Is.Not.Null);
			Assert.That(instance, Is.InstanceOf<PublicParameterLessCtor>());
		}
	}
}
