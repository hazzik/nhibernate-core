#if NET_2_0
using System;
using NHibernate.Bytecode.Lightweight;
using NHibernate.Property;
using NUnit.Framework;
using System.Reflection;

namespace NHibernate.Test.ReflectionOptimizerTest
{
	[TestFixture]
	public class LcgFixture
	{
		public class NoSetterClass
		{
			public int Property
			{
				get { return 0; }
			}
		}
		
		[Test, ExpectedException(typeof(PropertyNotFoundException))]
		public void NoSetter()
		{
			IGetter[] getters = new IGetter[]
				{
					new BasicGetter(typeof (NoSetterClass), typeof (NoSetterClass).GetProperty("Property"), "Property")
				};
			ISetter[] setters = new ISetter[]
				{
					new BasicSetter(typeof (NoSetterClass), typeof (NoSetterClass).GetProperty("Property"), "Property")
				};

            new ReflectionOptimizer(typeof(NoSetterClass), getters, setters, null, null);
		}

		public class NoGetterClass
		{
			public int Property
			{
				set { }
			}
		}
		
		[Test, ExpectedException(typeof(PropertyNotFoundException))]
		public void NoGetter()
		{
			IGetter[] getters = new IGetter[]
				{
					new BasicGetter(typeof (NoGetterClass), typeof (NoGetterClass).GetProperty("Property"), "Property")
				};
			ISetter[] setters = new ISetter[]
				{
					new BasicSetter(typeof (NoGetterClass), typeof (NoGetterClass).GetProperty("Property"), "Property")
				};

			new ReflectionOptimizer(typeof (NoGetterClass), getters, setters, null, null);
		}

        public class IdWithEntity
        {
            int m_id;

            public int Id
            {
                get { return m_id; }
                set { m_id = value; }
            }

            int m_data;

            public int Data
            {
                get { return m_data; }
                set { m_data = value; }
            }

        }

        [Test]
        public void TestOptimizedGetterSetters()
        {
            IGetter[] getters = new IGetter[] { new BasicGetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Data"), "Property") };
            ISetter[] setters = new ISetter[] { new BasicSetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Data"), "Property") };
            IGetter idGetter = new BasicGetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Id"), "Property");
            ISetter idSetter = new BasicSetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Id"), "Property");

            ReflectionOptimizer optimizer = new ReflectionOptimizer(typeof(IdWithEntity), getters, setters, idGetter, idSetter);

            int expectedId = 15;
            int expectedData = 20;
            IdWithEntity o = new IdWithEntity();
            Assert.IsNotNull(optimizer.IdentifierAccessOptimizer, "IdentifierAccessOptimizer not creaded");
            Assert.IsNotNull(optimizer.AccessOptimizer, "AccessOptimizernot creaded");
            optimizer.IdentifierAccessOptimizer.SetValue(o, expectedId);
            optimizer.AccessOptimizer.SetPropertyValues(o, new object[] { expectedData });

            int actualId = (int)optimizer.IdentifierAccessOptimizer.GetValue(o);
            int actualData = (int)(optimizer.AccessOptimizer.GetPropertyValues(o)[0]);

            Assert.AreEqual(expectedData, actualData, "data not same");
            Assert.AreEqual(expectedId, actualId, "id not same");
        }

        public sealed class NonOptimizedSetter : ISetter
        {
            private System.Type clazz;
            private PropertyInfo property;
            private string propertyName;

            /// <summary>
            /// Initializes a new instance of <see cref="BasicSetter"/>.
            /// </summary>
            /// <param name="clazz">The <see cref="System.Type"/> that contains the Property <c>set</c>.</param>
            /// <param name="property">The <see cref="PropertyInfo"/> for reflection.</param>
            /// <param name="propertyName">The name of the mapped Property.</param>
            public NonOptimizedSetter(System.Type clazz, PropertyInfo property, string propertyName)
            {
                this.clazz = clazz;
                this.property = property;
                this.propertyName = propertyName;
            }

            public PropertyInfo Property
            {
                get { return property; }
            }

            #region ISetter Members

            /// <summary>
            /// Sets the value of the Property on the object.
            /// </summary>
            /// <param name="target">The object to set the Property value in.</param>
            /// <param name="value">The value to set the Property to.</param>
            /// <exception cref="PropertyAccessException">
            /// Thrown when there is a problem setting the value in the target.
            /// </exception>
            public void Set(object target, object value)
            {
                try
                {
                    property.SetValue(target, value, new object[0]);
                }
                catch (ArgumentException ae)
                {
                    // if I'm reading the msdn docs correctly this is the only reason the ArgumentException
                    // would be thrown, but it doesn't hurt to make sure.
                    if (property.PropertyType.IsAssignableFrom(value.GetType()) == false)
                    {
                        // add some details to the error message - there have been a few forum posts an they are 
                        // all related to an ISet and IDictionary mixups.
                        string msg =
                            String.Format("The type {0} can not be assigned to a property of type {1}", value.GetType().ToString(),
                                          property.PropertyType.ToString());
                        throw new PropertyAccessException(ae, msg, true, clazz, propertyName);
                    }
                    else
                    {
                        throw new PropertyAccessException(ae, "ArgumentException while setting the property value by reflection", true,
                                                          clazz, propertyName);
                    }
                }
                catch (Exception e)
                {
                    throw new PropertyAccessException(e, "could not set a property value by reflection", true, clazz, propertyName);
                }
            }

            /// <summary>
            /// Gets the name of the mapped Property.
            /// </summary>
            /// <value>The name of the mapped Property or <see langword="null" />.</value>
            public string PropertyName
            {
                get { return property.Name; }
            }

            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> for the mapped Property.
            /// </summary>
            /// <value>The <see cref="PropertyInfo"/> for the mapped Property.</value>
            public MethodInfo Method
            {
                get { return property.GetSetMethod(true); }
            }

            #endregion
        }

        public sealed class NonOptimizedGetter : IGetter
        {
            private System.Type clazz;
            private PropertyInfo property;
            private string propertyName;

            /// <summary>
            /// Initializes a new instance of <see cref="BasicGetter"/>.
            /// </summary>
            /// <param name="clazz">The <see cref="System.Type"/> that contains the Property <c>get</c>.</param>
            /// <param name="property">The <see cref="PropertyInfo"/> for reflection.</param>
            /// <param name="propertyName">The name of the Property.</param>
            public NonOptimizedGetter(System.Type clazz, PropertyInfo property, string propertyName)
            {
                this.clazz = clazz;
                this.property = property;
                this.propertyName = propertyName;
            }

            public PropertyInfo Property
            {
                get { return property; }
            }

            #region IGetter Members

            /// <summary>
            /// Gets the value of the Property from the object.
            /// </summary>
            /// <param name="target">The object to get the Property value from.</param>
            /// <returns>
            /// The value of the Property for the target.
            /// </returns>
            public object Get(object target)
            {
                try
                {
                    return property.GetValue(target, new object[0]);
                }
                catch (Exception e)
                {
                    throw new PropertyAccessException(e, "Exception occurred", false, clazz, propertyName);
                }
            }

            /// <summary>
            /// Gets the <see cref="System.Type"/> that the Property returns.
            /// </summary>
            /// <value>The <see cref="System.Type"/> that the Property returns.</value>
            public System.Type ReturnType
            {
                get { return property.PropertyType; }
            }

            /// <summary>
            /// Gets the name of the Property.
            /// </summary>
            /// <value>The name of the Property.</value>
            public string PropertyName
            {
                get { return property.Name; }
            }

            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> for the Property.
            /// </summary>
            /// <value>
            /// The <see cref="PropertyInfo"/> for the Property.
            /// </value>
            public MethodInfo Method
            {
                get { return property.GetGetMethod(true); }
            }

            #endregion
        }

        [Test]
        public void TestNonOptimizedGetterSetters()
        {
            IGetter[] getters = new IGetter[] { new NonOptimizedGetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Data"), "Property") };
            ISetter[] setters = new ISetter[] { new NonOptimizedSetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Data"), "Property") };
            IGetter idGetter = new NonOptimizedGetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Id"), "Property");
            ISetter idSetter = new NonOptimizedSetter(typeof(IdWithEntity), typeof(IdWithEntity).GetProperty("Id"), "Property");

            ReflectionOptimizer optimizer = new ReflectionOptimizer(typeof(IdWithEntity), getters, setters, idGetter, idSetter);

            int expectedId = 15;
            int expectedData = 20;
            IdWithEntity o = new IdWithEntity();
            Assert.IsNotNull(optimizer.IdentifierAccessOptimizer, "IdentifierAccessOptimizer not creaded");
            Assert.IsNotNull(optimizer.AccessOptimizer, "AccessOptimizernot creaded");
            optimizer.IdentifierAccessOptimizer.SetValue(o, expectedId);
            optimizer.AccessOptimizer.SetPropertyValues(o, new object[] { expectedData });

            int actualId = (int)optimizer.IdentifierAccessOptimizer.GetValue(o);
            int actualData = (int)(optimizer.AccessOptimizer.GetPropertyValues(o)[0]);

            Assert.AreEqual(expectedId, actualId, "id not same");
            Assert.AreEqual(expectedData, actualData, "data not same");
        }

	}
}
#endif