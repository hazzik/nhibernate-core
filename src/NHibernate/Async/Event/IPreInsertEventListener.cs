﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace NHibernate.Event
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial interface IPreInsertEventListener
	{
		/// <summary> Return true if the operation should be vetoed</summary>
		/// <param name="event"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken);
	}
}
