﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Event;

namespace NHibernate.Test.NHSpecificTest.NH2322
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class PostUpdateEventListener : IPostUpdateEventListener
	{
		Task IPostUpdateEventListener.OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
		{
			try
			{
				if (@event.Entity is Person)
				{
					return @event.Session
						.CreateSQLQuery("update Person set Name = :newName")
						.SetString("newName", "new updated name")
						.ExecuteUpdateAsync(cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}
	}
}
