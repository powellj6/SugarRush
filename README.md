Sugar Rush Console App
=======================================

### Description ###

Quickly update nuget package references in large .NET projects.

### Run ###

Currently set up to only be run after building, either in debug or release mode. `cd` into the folder containing the `SugarRush.exe`, update App.config, and then run the exe from the command line.

App.config example:
```
<appSettings>
	<add key="folderPath" value="C:\code\MyProject"/>
	<add key="exclusionPaths" value="C:\code\MyProject\ExcludeThisFolder"/>
	<add key="packageID" value="Domain.ServiceConfiguration"/>
	<add key="packageVersion" value="1.0.352-ConfigUpdate"/>
	<add key="nugetRepoUrl" value="https://api.nuget.org/v3/index.json"/>
	<add key="logFilePath" value="C:\logs\SugarRush"/>
	<add key="packageDownloadFolder" value="C:\Temp\SugarRush\NugetPackages\"/>
</appSettings>
```