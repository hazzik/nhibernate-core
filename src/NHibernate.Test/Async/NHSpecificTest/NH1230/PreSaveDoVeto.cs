﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using log4net;
using NHibernate.Event;

namespace NHibernate.Test.NHSpecificTest.NH1230
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class PreSaveDoVeto : IPreInsertEventListener
	{

		#region IPreInsertEventListener Members

		/// <summary> Return true if the operation should be vetoed</summary>
		/// <param name="event"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
		{
			try
			{
				log.Debug("OnPreInsert: The entity will be vetoed.");

				return Task.FromResult<bool>(true);
			}
			catch (System.Exception ex)
			{
				return Task.FromException<bool>(ex);
			}
		}

		#endregion
	}
}
