﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace NHibernate.Action
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial interface IExecutable
	{

		/// <summary> Called before executing any actions</summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task BeforeExecutionsAsync(CancellationToken cancellationToken);

		/// <summary> Execute this action</summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task ExecuteAsync(CancellationToken cancellationToken);

	}
}
