import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.XmlReport
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildFeatures.xmlReport
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetNugetPush
import jetbrains.buildServer.configs.kotlin.buildSteps.nant
import jetbrains.buildServer.configs.kotlin.buildSteps.nuGetPublish
import jetbrains.buildServer.configs.kotlin.triggers.VcsTrigger
import jetbrains.buildServer.configs.kotlin.triggers.finishBuildTrigger
import jetbrains.buildServer.configs.kotlin.triggers.schedule
import jetbrains.buildServer.configs.kotlin.triggers.vcs

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2023.05"

project {

    buildType(SapSqlAnywhere)
    buildType(DevPackage)
    buildType(PostgreSQL)
    buildType(Oracle32)
    buildType(SqlServer2012)
    buildType(OracleManaged32)
    buildType(ReleasePackage)
    buildType(SQLite32)
    buildType(SqlServerMicrosoftDataSqlClient)
    buildType(MySql)
    buildType(NHibernate)
    buildType(Firebird32)
    buildType(SqlServerOdbc)
    buildType(SqlServerCe32)

    template(Template_1)
    buildTypesOrder = arrayListOf(NHibernate, DevPackage, ReleasePackage, SQLite32, Firebird32, MySql, Oracle32, OracleManaged32, PostgreSQL, SapSqlAnywhere, SqlServerMicrosoftDataSqlClient, SqlServer2012, SqlServerCe32, SqlServerOdbc)
}

object DevPackage : BuildType({
    name = "NHibernate (Publish Dev NuGet Package)"

    allowExternalStatus = true
    type = BuildTypeSettings.Type.DEPLOYMENT
    buildNumberPattern = "%build.counter% (Git: {build.vcs.number.1})"
    publishArtifacts = PublishMode.SUCCESSFUL

    vcs {
        root(DslContext.settingsRoot)

        checkoutMode = CheckoutMode.MANUAL
        cleanCheckout = true
        branchFilter = """
            +:<default>
            +:5.*.x
        """.trimIndent()
    }

    steps {
        dotnetNugetPush {
            name = "Publish Github Package Registry"
            enabled = false
            packages = "./NHibernate*/nuget_gallery/*-dev*.nupkg"
            serverUrl = "GPR"
            apiKey = "credentialsJSON:0564b001-7323-48a9-9ed7-69ee9e330720"
            param("dotNetCoverage.dotCover.home.path", "%teamcity.tool.JetBrains.dotCover.CommandLineTools.DEFAULT%")
        }
        dotnetNugetPush {
            name = "Publish MyGet"
            enabled = false
            packages = "./NHibernate*/nuget_gallery/*-dev*.nupkg"
            serverUrl = "https://www.myget.org/F/nhibernate/api/v3/index.json"
            apiKey = "credentialsJSON:20887698-e0a5-405b-90e8-afe1f5938582"
            param("dotNetCoverage.dotCover.home.path", "%teamcity.tool.JetBrains.dotCover.CommandLineTools.DEFAULT%")
        }
        dotnetNugetPush {
            name = "Publish nuget to CloudSmith"
            packages = "./NHibernate*/nuget_gallery/*-dev*.nupkg"
            serverUrl = "https://nuget.cloudsmith.io/nhibernate/nhibernate-core/v3/index.json"
            apiKey = "credentialsJSON:95a4dfa0-c4d5-4547-973b-a02d0071e010"
            param("dotNetCoverage.dotCover.home.path", "%teamcity.tool.JetBrains.dotCover.CommandLineTools.DEFAULT%")
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${ReleasePackage.id}"
            successfulOnly = true
            branchFilter = """
                +:<default>
                +:5.*.x
            """.trimIndent()
        }
    }

    failureConditions {
        executionTimeoutMin = 60
        testFailure = false
        nonZeroExitCode = false
        javaCrash = false
    }

    features {
        commitStatusPublisher {
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = personalToken {
                    token = "credentialsJSON:a531a6d1-0daf-4cda-bfa6-084825ef8e52"
                }
            }
            param("github_oauth_user", "hazzik")
        }
    }

    dependencies {
        artifacts(ReleasePackage) {
            buildRule = lastSuccessful("""
                +:<default>
                +:5.*.x
            """.trimIndent())
            cleanDestination = true
            artifactRules = "NHibernate*/nuget_gallery/*.*pkg"
        }
    }

    requirements {
        contains("system.agent.name", "nhibernate", "RQ_282")
    }

    cleanup {
        baseRule {
            artifacts(builds = 10, artifactPatterns = "+:**/*")
        }
    }
    
    disableSettings("RQ_282")
})

object Firebird32 : BuildType({
    templates(Template_1)
    name = "NHibernate Firebird 32"

    params {
        param("database", "firebird32")
    }

    requirements {
    }
})

object MySql : BuildType({
    templates(Template_1)
    name = "NHibernate MySql"

    params {
        param("database", "mysql")
    }

    features {
        commitStatusPublisher {
            id = "BUILD_EXT_150"
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = personalToken {
                    token = "credentialsJSON:a531a6d1-0daf-4cda-bfa6-084825ef8e52"
                }
            }
            param("github_oauth_user", "hazzik")
        }
    }
})

object NHibernate : BuildType({
    name = "NHibernate"

    allowExternalStatus = true
    artifactRules = "./build/**/*.zip"
    buildNumberPattern = "%build.counter% (Git: {build.vcs.number.1})"
    maxRunningBuilds = 1

    vcs {
        root(DslContext.settingsRoot)

        cleanCheckout = true
    }

    steps {
        nant {
            mode = nantFile {
                path = "teamcity.build"
            }
            nantHome = """Tools\nant\bin"""
            args = "-D:skip.manual=true -D:skip.db-service=false"
        }
    }

    triggers {
        vcs {
            quietPeriodMode = VcsTrigger.QuietPeriodMode.USE_DEFAULT
            branchFilter = ""
        }
    }

    failureConditions {
        executionTimeoutMin = 60
    }

    features {
        xmlReport {
            reportType = XmlReport.XmlReportType.NUNIT
            rules = "./build/**/*dll-results.xml"
        }
        commitStatusPublisher {
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = personalToken {
                    token = "credentialsJSON:a531a6d1-0daf-4cda-bfa6-084825ef8e52"
                }
            }
            param("github_oauth_user", "hazzik")
        }
    }

    requirements {
        contains("system.agent.name", "nhibernate")
    }

    cleanup {
        baseRule {
            artifacts(builds = 10, artifactPatterns = "+:**/*")
        }
    }
})

object Oracle32 : BuildType({
    templates(Template_1)
    name = "NHibernate Oracle 32"
    paused = true

    params {
        param("database", "oracle32")
    }
})

object OracleManaged32 : BuildType({
    templates(Template_1)
    name = "NHibernate Oracle Managed 32"

    params {
        param("database", "oracle-managed32")
    }
})

object PostgreSQL : BuildType({
    templates(Template_1)
    name = "NHibernate PostgreSQL"

    params {
        param("database", "postgresql")
    }
})

object ReleasePackage : BuildType({
    name = "NHibernate (Release Package)"

    allowExternalStatus = true
    artifactRules = """
        ./build/**/*.zip
        ./build/**/nuget_gallery/*.*
    """.trimIndent()
    buildNumberPattern = "%build.counter% (Git: {build.vcs.number.1})"
    maxRunningBuilds = 1

    params {
        param("system.VersionSuffix", "${DslContext.settingsRoot.paramRefs.buildVcsBranch}-%build.number%")
        param("system.build.branchName", "%teamcity.build.branch%")
        param("system.build.counter", "%build.counter%")
        param("system.nuget.branchNameRegex", """(^refs/heads/master)|(^\d\.\d\.x${'$'})|(\<default\>)""")
    }

    vcs {
        root(DslContext.settingsRoot)

        checkoutMode = CheckoutMode.ON_AGENT
        cleanCheckout = true
    }

    steps {
        nant {
            mode = nantFile {
                path = "teamcity.build"
            }
            nantHome = """Tools\nant\bin"""
            args = "-D:skip.db-service=false -D:project.config=release -D:CCNetLabel=${DslContext.settingsRoot.paramRefs.buildVcsBranch}-%build.counter% cleanall init copy-teamcity-configuration start-db-service package nugetpushbat stop-db-service"
        }
        nuGetPublish {
            name = "Publish Github Package Registry"
            enabled = false
            toolPath = "./Tools/nuget.exe"
            packages = "./build/**/nuget_gallery/*.*"
            serverUrl = "GPR"
        }
        nuGetPublish {
            name = "Publish MyGet"
            enabled = false
            toolPath = "./Tools/nuget.exe"
            packages = "./build/**/nuget_gallery/*.*"
            serverUrl = "https://www.myget.org/F/nhibernate/api/v2/package"
            apiKey = "credentialsJSON:20887698-e0a5-405b-90e8-afe1f5938582"
        }
    }

    triggers {
        schedule {
            schedulingPolicy = daily {
                hour = 0
                timezone = "Etc/Universal"
            }
            triggerBuild = always()
            param("revisionRuleBuildBranch", "<default>")
        }
    }

    failureConditions {
        executionTimeoutMin = 60
    }

    features {
        xmlReport {
            reportType = XmlReport.XmlReportType.NUNIT
            rules = "./build/**/*dll-results.xml"
        }
        commitStatusPublisher {
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = personalToken {
                    token = "credentialsJSON:dd9ba246-dafb-4e4f-a0df-72ba30f27c13"
                }
            }
            param("secure:github_password", "credentialsJSON:ac865c0a-3d56-4e88-b3f3-01cf10181272")
            param("github_username", "nhibernate-bot")
        }
    }

    requirements {
        contains("system.agent.name", "nhibernate")
    }

    cleanup {
        baseRule {
            artifacts(builds = 10, artifactPatterns = "+:**/*")
        }
    }
})

object SQLite32 : BuildType({
    templates(Template_1)
    name = "NHibernate SQLite 32"

    params {
        param("database", "sqlite32")
    }
})

object SapSqlAnywhere : BuildType({
    templates(Template_1)
    name = "NHibernate SAP SQL Anywhere"

    params {
        param("database", "sqlanywhere")
    }

    requirements {
    }
})

object SqlServer2012 : BuildType({
    templates(Template_1)
    name = "NHibernate SqlServer 2012"

    params {
        param("database", "sqlServer2012")
    }
})

object SqlServerCe32 : BuildType({
    templates(Template_1)
    name = "NHibernate SqlServerCe 32"

    params {
        param("database", "sqlServerCe32")
    }
})

object SqlServerMicrosoftDataSqlClient : BuildType({
    templates(Template_1)
    name = "NHibernate SqlServer (Microsoft.Data.SqlClient)"
    paused = true

    params {
        param("database", "sqlServer-MicrosoftDataSqlClientDriver")
    }
})

object SqlServerOdbc : BuildType({
    templates(Template_1)
    name = "NHibernate SQL Server ODBC"

    params {
        param("database", "sqlServerOdbc")
    }
})

object Template_1 : Template({
    id("Template")
    name = "NHibernate DB Template"

    allowExternalStatus = true
    artifactRules = """
        ./build/**/NHibernate.Test.last-results.xml
        ./build/**/NHibernate.Test.current-results.xml
        ./build/**/Comparison.txt
    """.trimIndent()
    buildNumberPattern = "%build.counter% (Git: {build.vcs.number.1})"

    params {
        param("database", "")
    }

    vcs {
        root(DslContext.settingsRoot)

        cleanCheckout = true
    }

    steps {
        nant {
            id = "RUNNER_NAnt_159"
            mode = nantFile {
                path = "teamcity.build"
            }
            nantHome = """Tools\nant\bin"""
            args = "-D:skip.manual=true -D:config.teamcity=%database% -D:skip.db-service=false"
        }
    }

    triggers {
        vcs {
            id = "vcsTrigger"
            triggerRules = """
                +:.
                -:./doc/**
            """.trimIndent()
            branchFilter = ""
            watchChangesInDependencies = true
        }
    }

    failureConditions {
        executionTimeoutMin = 60
        testFailure = false
    }

    features {
        xmlReport {
            id = "BUILD_EXT_8"
            reportType = XmlReport.XmlReportType.NUNIT
            rules = "./build/**/*dll-results.xml"
        }
        commitStatusPublisher {
            id = "BUILD_EXT_150"
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = personalToken {
                    token = "credentialsJSON:a531a6d1-0daf-4cda-bfa6-084825ef8e52"
                }
            }
            param("github_oauth_user", "hazzik")
        }
    }

    dependencies {
        snapshot(NHibernate) {
            onDependencyFailure = FailureAction.FAIL_TO_START
        }
    }

    requirements {
        contains("system.agent.name", "nhibernate", "RQ_283")
    }
})
