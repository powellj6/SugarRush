Sugar Rush Console App
=======================================

### Description ###

This is a glorified text find-and-replace for quickly updating nuget package references in large .NET projects.
It will find and update `csproj`, `refresh`, and `packages.config` files. Build your project after running to 
actually download and update the packages.

### Note ###
In it's current state, it will only update the text for the package and it's direct assembly references. It will not update
or resolve dependencies and/or their sub-dependencies. If you know you only need to update some text for your package
and it's direct assemblies, use this.

### Run ###

`cd` into the folder containing `SugarRush.exe`, update `SugarRush.exe.config`, and then run the exe.

SugarRush.exe.config example:
```
<appSettings>
	<add key="folderPath" value="C:\code\MyProject"/>
	<add key="exclusionPaths" value="C:\code\MyProject\ExcludeThisFolder,C:\code\MyProject\AlsoExcludeThisFolder"/>
	<add key="packageID" value="Domain.ServiceConfiguration"/>
	<add key="packageVersion" value="1.0.352-ConfigUpdate"/>
	<add key="nugetRepoUrl" value="https://api.nuget.org/v3/index.json"/>
	<add key="logFilePath" value="C:\logs\SugarRush"/>
	<add key="packageDownloadFolder" value="C:\Temp\SugarRush\NugetPackages\"/>
</appSettings>
```