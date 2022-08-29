﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Proxy;

namespace NHibernate.Test.NHSpecificTest.NH1789
{
	using System.Threading.Tasks;
	using System.Threading;
	public abstract partial class DomainObject : IDomainObject
	{

		#region IDomainObject Members

		#endregion

		/// <summary>
		/// Turn a proxy object into a "real" object. If the <paramref name="proxy"/> you give in parameter is not a INHibernateProxy, it will returns the same object without any change.
		/// </summary>
		/// <typeparam name="T">Type in which the unproxied object should be returned</typeparam>
		/// <param name="proxy">Proxy object</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>Unproxied object</returns>
		public static async Task<T> UnProxyAsync<T>(object proxy, CancellationToken cancellationToken = default(CancellationToken))
		{
			//If the object is not a proxy, just cast it and returns it
			if (!(proxy is INHibernateProxy))
			{
				return (T) proxy;
			}

			//Otherwise, use the NHibernate methods to get the implementation, and cast it
			var p = (INHibernateProxy) proxy;
			return (T) await (p.HibernateLazyInitializer.GetImplementationAsync(cancellationToken));
		}

		/// <summary>
		/// Turn a proxy object into a "real" object. If the <paramref name="proxy"/> you give in parameter is not a INHibernateProxy, it will returns the same object without any change.
		/// </summary>
		/// <param name="proxy">Proxy object</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>Unproxied object</returns>
		public static Task<object> UnProxyAsync(object proxy, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UnProxyAsync<object>(proxy, cancellationToken);
		}
	}
}
