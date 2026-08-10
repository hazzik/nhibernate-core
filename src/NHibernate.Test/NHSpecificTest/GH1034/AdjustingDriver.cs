using System;
using System.Data;
using System.Data.Common;
using NHibernate.Driver;

namespace NHibernate.Test.NHSpecificTest.GH1034
{
	/// <summary>
	/// A driver that mimics what <c>Sql2008ClientDriver</c> and <c>FirebirdClientDriver</c> do in
	/// real usage: it adjusts the command in <see cref="AdjustCommand"/>, after the command has
	/// already been built. The marker appended here should show up in the logged SQL if the logging
	/// is done after the adjustment, as it should be.
	/// </summary>
	public class AdjustingDriver : NpgsqlDriver
	{
		public const string AdjustmentMarker = "/* adjusted-by-driver */";

		public override void AdjustCommand(DbCommand command)
		{
			if (command.CommandType == CommandType.Text &&
				command.CommandText.TrimStart().StartsWith("select", StringComparison.OrdinalIgnoreCase))
			{
				command.CommandText += " " + AdjustmentMarker;
			}

			base.AdjustCommand(command);
		}
	}
}
