using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Property;

namespace NHibernate.Bytecode.Lightweight
{
    /// <summary>
    /// Represents optimized entity identifier access.
    /// </summary>
    public class PropertyAccessOptimizer : IPropertyAccessOptimizer
    {
        private GetPropertyValueInvoker getDelegate;
        private SetPropertyValueInvoker setDelegate;
        private IGetter getter;
        private ISetter setter;
        private PropertyGetterCallback getterCallback;
        private PropertySetterCallback setterCallback;

        public PropertyAccessOptimizer(GetPropertyValueInvoker getDelegate, SetPropertyValueInvoker setDelegate, IGetter getter, ISetter setter)
        {
            this.setter = setter;
            this.getter = getter;
            this.getDelegate = getDelegate;
            this.setDelegate = setDelegate;

            this.setterCallback = new PropertySetterCallback(OnSetterCallback);
            this.getterCallback = new PropertyGetterCallback(OnGetterCallback);
        }

        private void OnSetterCallback(object target, object value)
        {
            setter.Set(target, value);
        }

        private object OnGetterCallback(object target)
        {
            return getter.Get(target);
        }


        public object GetValue(object target)
        {
            return this.getDelegate(target, this.getterCallback);
        }

        public void SetValue(object target, object value)
        {
            this.setDelegate(target,value, this.setterCallback);
        }
    }
}
