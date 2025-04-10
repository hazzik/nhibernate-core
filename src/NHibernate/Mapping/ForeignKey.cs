using System.Collections.Generic;
using System.Text;
using NHibernate.Util;
using System;
using System.Linq;

namespace NHibernate.Mapping
{
	/// <summary>
	/// A Foreign Key constraint in the database.
	/// </summary>
	[Serializable]
	public class ForeignKey : Constraint
	{
		private Table referencedTable;
		private string referencedEntityName;
		private bool cascadeDeleteEnabled;
		private List<Column> referencedColumns;

		/// <summary>
		/// Generates the SQL string to create the named Foreign Key Constraint in the database.
		/// </summary>
		/// <param name="d">The <see cref="Dialect.Dialect"/> to use for SQL rules.</param>
		/// <param name="constraintName">The name to use as the identifier of the constraint in the database.</param>
		/// <param name="defaultSchema"></param>
		/// <param name="defaultCatalog"></param>
		/// <returns>
		/// A string that contains the SQL to create the named Foreign Key Constraint.
		/// </returns>
		public override string SqlConstraintString(
			Dialect.Dialect d,
			string constraintName,
			string defaultCatalog,
			string defaultSchema)
		{
			var refiter = IsReferenceToPrimaryKey
				? referencedTable.PrimaryKey.Columns
				: referencedColumns;

			var cols = Columns.ToArray(column => column.GetQuotedName(d));
			var refcols = refiter.ToArray(column => column.GetQuotedName(d));

			string result = d.GetAddForeignKeyConstraintString(
				constraintName,
				cols,
				referencedTable.GetQualifiedName(d, defaultCatalog, defaultSchema),
				refcols,
				IsReferenceToPrimaryKey);

			return cascadeDeleteEnabled && d.SupportsCascadeDelete ? result + " on delete cascade" : result;
		}

		/// <summary>
		/// Gets or sets the <see cref="Table"/> that the Foreign Key is referencing.
		/// </summary>
		/// <value>The <see cref="Table"/> the Foreign Key is referencing.</value>
		/// <exception cref="MappingException">
		/// Thrown when the number of columns in this Foreign Key is not the same
		/// amount of columns as the Primary Key in the ReferencedTable.
		/// </exception>
		public Table ReferencedTable
		{
			get { return referencedTable; }
			set { referencedTable = value; }
		}

		public bool CascadeDeleteEnabled
		{
			get { return cascadeDeleteEnabled; }
			set { cascadeDeleteEnabled = value; }
		}

		#region IRelationalModel Memebers

		/// <summary>
		/// Get the SQL string to drop this Constraint in the database.
		/// </summary>
		/// <param name="dialect">The <see cref="Dialect.Dialect"/> to use for SQL rules.</param>
		/// <param name="defaultSchema"></param>
		/// <param name="defaultCatalog"></param>
		/// <returns>
		/// A string that contains the SQL to drop this Constraint.
		/// </returns>
		public override string SqlDropString(Dialect.Dialect dialect, string defaultCatalog, string defaultSchema)
		{
			var catalog = Table.GetQuotedCatalog(dialect, defaultCatalog);
			var schema = Table.GetQuotedSchema(dialect, defaultSchema);
			var quotedName = Table.GetQuotedName(dialect);

			return new StringBuilder()
				.AppendLine(dialect.GetIfExistsDropConstraint(catalog, schema, quotedName, Name))
				.AppendFormat("alter table ")
				.Append(Table.GetQualifiedName(dialect, defaultCatalog, defaultSchema))
				.Append(" ")
				.AppendLine(dialect.GetDropForeignKeyConstraintString(Name))
				.Append(dialect.GetIfExistsDropConstraintEnd(catalog, schema, quotedName, Name))
				.ToString();
		}

		#endregion

		/// <summary> 
		/// Validates that columnspan of the foreignkey and the primarykey is the same.
		///  Furthermore it aligns the length of the underlying tables columns.
		/// </summary>
		public void AlignColumns()
		{
			if (IsReferenceToPrimaryKey)
				AlignColumns(referencedTable);
		}

		private void AlignColumns(Table referencedTable)
		{
			if (referencedTable.PrimaryKey.ColumnSpan != ColumnSpan)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Foreign key (")
					.Append(Name + ":")
					.Append(Table.Name)
					.Append(" [");
				AppendColumns(sb, ColumnIterator);
				sb.Append("])")
					.Append(") must have same number of columns as the referenced primary key (")
					.Append(referencedTable.Name).Append(" [");
				AppendColumns(sb, referencedTable.PrimaryKey.ColumnIterator);
				sb.Append("])");
				throw new FKUnmatchingColumnsException(sb.ToString());
			}

			AlignColumns(ColumnIterator, referencedTable.PrimaryKey.ColumnIterator);
		}

		internal static void AlignColumns(IEnumerable<Column> fk, IEnumerable<Column> pk)
		{
			using (var fkCols = fk.GetEnumerator())
			using (var pkCols = pk.GetEnumerator())
			{
				while (fkCols.MoveNext() && pkCols.MoveNext())
				{
					if (pkCols.Current.IsLengthDefined() || fkCols.Current.IsLengthDefined())
						fkCols.Current.Length = pkCols.Current.Length;
					if (pkCols.Current.IsPrecisionDefined() || fkCols.Current.IsPrecisionDefined())
						fkCols.Current.Precision = pkCols.Current.Precision;
					if (pkCols.Current.IsScaleDefined() || fkCols.Current.IsScaleDefined())
						fkCols.Current.Scale = pkCols.Current.Scale;
				}
			}
		}

		private static void AppendColumns(StringBuilder buf, IEnumerable<Column> columns)
		{
			bool commaNeeded = false;
			foreach (Column column in columns)
			{
				if (commaNeeded)
					buf.Append(StringHelper.CommaSpace);
				commaNeeded = true;
				buf.Append(column.Name);
			}
		}

		public virtual void AddReferencedColumns(IEnumerable<Column> referencedColumnsIterator)
		{
			foreach (Column col in referencedColumnsIterator)
			{
				if (!col.IsFormula)
					AddReferencedColumn(col);
			}
		}

		private void AddReferencedColumn(Column column)
		{
			referencedColumns ??= new List<Column>(1);
			if (!referencedColumns.Contains(column))
				referencedColumns.Add(column);
		}

		internal void AddReferencedTable(PersistentClass referencedClass)
		{
			referencedTable = IsReferenceToPrimaryKey ? referencedClass.Table : referencedColumns[0].Value.Table;
		}

		public override string ToString()
		{
			if (IsReferenceToPrimaryKey)
				return base.ToString();

			var columns = string.Join(", ", Columns);
			var refColumns = string.Join(", ", referencedColumns);
			return $"{GetType().FullName}({Table.Name}{columns} ref-columns:({refColumns}) as {Name}";
		}

		public bool HasPhysicalConstraint
		{
			get
			{
				return referencedTable.IsPhysicalTable && Table.IsPhysicalTable && !referencedTable.HasDenormalizedTables;
			}
		}

		public IList<Column> ReferencedColumns
		{
			get
			{	
				referencedColumns ??= new List<Column>(1);
				return referencedColumns;
			}
		}

		public string ReferencedEntityName
		{
			get { return referencedEntityName; }
			set { referencedEntityName = value; }
		}

		/// <summary>Does this foreignkey reference the primary key of the reference table </summary>
		public bool IsReferenceToPrimaryKey => referencedColumns == null || referencedColumns.Count == 0;

		public string GeneratedConstraintNamePrefix => "FK_";

		public override bool IsGenerated(Dialect.Dialect dialect)
		{
			if (!HasPhysicalConstraint)
				return false;
			if (dialect.SupportsNullInUnique || IsReferenceToPrimaryKey)
				return true;

			return referencedColumns.All(column => !column.IsNullable);
		}
	}
}
