using System;
using System.Reflection;
using log4net.Config;
using NUnit.Framework;

[assembly: CLSCompliant(false)]
[assembly: AssemblyDelaySign(false)]

[assembly: LevelOfParallelism(0)]

[assembly: XmlConfigurator(ConfigFile = "log4net.config")]
