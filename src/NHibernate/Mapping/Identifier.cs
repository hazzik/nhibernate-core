using System;

namespace NHibernate.Mapping
{
	public class QualifiedName
	{
		public QualifiedName(Identifier catalogName, Identifier schemaName, Identifier objectName)
		{
			CatalogName = catalogName;
			SchemaName = schemaName;
			ObjectName = objectName;
		}

		public Identifier CatalogName { get; }
		public Identifier SchemaName { get; }
		public Identifier ObjectName { get; }
	}
	
	public class Identifier : IEquatable<Identifier>
	{
		public Identifier(string text, bool quoted)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentException("Identifier text cannot be null", nameof(text));
			if (IsQuoted(text))
				throw new ArgumentException("Identifier text should not contain quote markers (` or \")", nameof(text));
			Text = text;
			Quoted = quoted;
		}

		public string Text { get; }

		public bool Quoted { get; }

		public string CanonicalName => Quoted ? Text : Text.ToLowerInvariant();

		public bool Equals(Identifier other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;
			return string.Equals(CanonicalName, other.CanonicalName, StringComparison.Ordinal);
		}

		public static Identifier ToIdentifier(string text)
		{
			return ToIdentifier(text, false);
		}

		public static Identifier ToIdentifier(string text, bool quote)
		{
			if (string.IsNullOrEmpty(text))
				return null;
			var trimmedText = text.Trim();
			if (IsQuoted(trimmedText))
			{
				var bareName = trimmedText.Substring(1, trimmedText.Length - 2);
				return new Identifier(bareName, true);
			}
			return new Identifier(trimmedText, quote);
		}

		public static bool IsQuoted(string name)
		{
			if (string.IsNullOrEmpty(name)) return false;
			var begin = name[0];
			var end = name[name.Length - 1];
			return begin == '`' && end == '`'
			       || begin == '[' && end == ']'
			       || begin == '"' && end == '"';
		}

		public string Render(Dialect.Dialect dialect)
		{
			return Quoted
				? dialect.OpenQuote + Text + dialect.CloseQuote
				: Text;
		}

		public string Render()
		{
			return Quoted
				? "`" + Text + "`"
				: Text;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Identifier);
		}

		public override int GetHashCode()
		{
			return CanonicalName.GetHashCode();
		}

		public static Identifier Quote(Identifier identifier)
		{
			return identifier.Quoted
				? identifier
				: new Identifier(identifier.Text, true);
		}

		public override string ToString()
		{
			return Render();
		}
	}
}
