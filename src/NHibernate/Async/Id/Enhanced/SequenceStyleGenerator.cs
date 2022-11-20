﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;

using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Id.Enhanced
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class SequenceStyleGenerator : IPersistentIdentifierGenerator, IConfigurable
	{

		#region Implementation of IIdentifierGenerator

		public virtual Task<object> GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Optimizer.GenerateAsync(DatabaseStructure.BuildCallback(session), cancellationToken);
			}
			catch (System.Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion
	}
}