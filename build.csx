#!/usr/bin/env dotnet-script
#r "nuget: Bullseye, 4.0.0"
#r "nuget: SimpleExec, 11.0.0"
#r "nuget: Mono.Options, 6.12.0.148"

using Mono.Options;
using System.Xml.Linq;
using System.Xml.XPath;

using static Bullseye.Targets;
using static SimpleExec.Command;
using static System.IO.Directory;

var database = "SqlServer2012";
var configuration = "Release";
var connectionString = "";
var configDir = "./current-test-configuration";
var testAssemblies = new [] 
{ 
    "NHibernate.TestDatabaseSetup",
    "NHibernate.Test",
    "NHibernate.Test.VisualBasic"
};

var options = new OptionSet 
{
    { "configuration=", c => configuration = c },
    { "database=", c => database = c },
    { "connectionString=|connection-string=", c => connectionString = c}
};

Dictionary<string, Dictionary<string, string>> allSettings = new (StringComparer.OrdinalIgnoreCase) {
    {
        "Firebird", 
        new ()
        {
            ["connection.connection_string"] = @"DataSource=localhost;Database=nhibernate;User ID=SYSDBA;Password=masterkey;MaxPoolSize=200;",
            ["connection.driver_class"] = "NHibernate.Driver.FirebirdClientDriver",
            ["dialect"] = "NHibernate.Dialect.FirebirdDialect"
        }
    },
    {
        "MySQL", 
        new ()
        {
            ["connection.connection_string"] = @"Server=127.0.0.1;Uid=root;Pwd=Password12!;Database=nhibernate;Old Guids=True;SslMode=none;",
            ["connection.driver_class"] = "NHibernate.Driver.MySqlDataDriver",
            ["dialect"] = "NHibernate.Dialect.MySQL5Dialect"
        }
    },
    {
        "Odbc", 
        // The OdbcDriver inherits SupportsMultipleOpenReaders=true from DriverBase, which requires Mars_Connection=yes for SQL Server.
        new ()
        {
            ["connection.connection_string"] = @"Server=(local)\SQL2017;Uid=sa;Pwd=Password12!;Database=nhibernateOdbc;Driver={SQL Server Native Client 11.0};Mars_Connection=yes;",
            ["connection.driver_class"] = "NHibernate.Driver.OdbcDriver",
            ["odbc.explicit_datetime_scale"] = "3",
            /*
            We need to use a dialect that avoids mapping DbType.Time to TIME on MSSQL. On modern SQL Server
            this becomes TIME(7). Later, such values cannot be read back over ODBC. The
            error we get is "System.ArgumentException : Unknown SQL type - SS_TIME_EX.". I don't know for certain
            why this occurs, but MS docs do say that for a client "compiled using a version of SQL Server Native
            Client prior to SQL Server 2008", TIME(7) cannot be converted back to the client. Assuming that .Net's
            OdbcDriver would be considered a "client compiled with a previous version", it would make sense. Anyway,
            using the MsSql2005Dialect avoids these test failures.
            */
            ["dialect"] = "NHibernate.Dialect.MsSql2005Dialect"
        }
    },
    {
        "PostgreSQL", 
        new ()
        {
            ["connection.connection_string"] = @"Host=localhost;Port=5432;Username=postgres;Password=Password12!;Database=nhibernate;Enlist=true",
            ["connection.driver_class"] = "NHibernate.Driver.NpgsqlDriver",
            ["dialect"] = "NHibernate.Dialect.PostgreSQL83Dialect"
        }
    },
    {
        "SQLite", 
        new ()
        {
            /*
            DateTimeFormatString allows to prevent storing the fact that written date was having kind UTC,
            which dodges the undesirable time conversion to local done on reads by System.Data.SQLite.
            See https://system.data.sqlite.org/index.html/tktview/44a0955ea344a777ffdbcc077831e1adc8b77a36
            and https://github.com/nhibernate/nhibernate-core/issues/1362
            */
            // Please note the connection string is formated for putting the db file in configDir.
            ["connection.connection_string"] = $@"Data Source={configDir}/NHibernate.db;DateTimeFormatString=yyyy-MM-dd HH:mm:ss.FFFFFFF;",
            ["connection.driver_class"] = "NHibernate.Driver.SQLite20Driver",
            ["dialect"] = "NHibernate.Dialect.SQLiteDialect"
        }
    },
    {
        "SqlServerCe", 
        new ()
        {
            // Please note the connection string is formated for putting the db file in configDir.
            ["connection.connection_string"] = $@"Data Source={configDir}/NHibernate.sdf;",
            ["connection.driver_class"] = "NHibernate.Driver.SQLite20Driver",
            ["dialect"] = "NHibernate.Dialect.SQLiteDialect"
        }
    },
    {
        "SqlServer2008", 
        new ()
        {
            ["connection.connection_string"] = @"Server=(local)\SQL2017;User ID=sa;Password=Password12!;initial catalog=nhibernate;",
        }
    },
    {
        "SqlServer2012", 
        new ()
        {
            ["connection.connection_string"] = @"Server=(local)\SQL2017;User ID=sa;Password=Password12!;initial catalog=nhibernate;",
            ["dialect"] = "NHibernate.Dialect.MsSql2012Dialect"
        }
    },
    {
        "Oracle", 
        new ()
        {
            ["connection.connection_string"] = @"User ID=nhibernate;Password=nhibernate;Metadata Pooling=false;Self Tuning=false;Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = XEPDB1)))",
            ["connection.driver_class"] = "NHibernate.Driver.OracleManagedDataClientDriver",
            ["dialect"] = "NHibernate.Dialect.Oracle10gDialect"
        }
    },
};

void SetConfiguration(Dictionary<string, string> values)
{
    if (!string.IsNullOrWhiteSpace(connectionString))
        values["connection.connection_string"] = connectionString;

    XNamespace ns = "urn:nhibernate-configuration-2.2";

    var config = XDocument.Load("./build-common/teamcity-hibernate.cfg.xml");
    var sessionFactory = config.Root.Element(ns + "session-factory");
    var properties = sessionFactory.Elements(ns + "property");
    foreach (var (name, value) in values)
    {
        var property = properties.SingleOrDefault(p => p.Attribute("name").Value == name);
        if (property is null)
        {
            sessionFactory.Add(new XElement(ns + "property", new XAttribute("name", name), value));
        }
        else
        {
            property.Value = value;
        }
    }

    var dir = CreateDirectory("current-test-configuration");
    config.Save($"{configDir}/hibernate.cfg.xml");
}

Target("default", 
    DependsOn("test"));

Target("set-configuration", 
    () => SetConfiguration(allSettings[database]));

Target("build", 
    DependsOn("set-configuration"),
    () => RunAsync("dotnet", $"build ./src/NHibernate.sln -m:1 -c {configuration}"));

Target("test", 
    DependsOn("build"), 
    testAssemblies, 
    assembly => RunAsync("dotnet", $"./src/{assembly}/bin/{configuration}/net6.0/{assembly}.dll"));

await RunTargetsAndExitAsync(options.Parse(Args));
