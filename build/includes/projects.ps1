<#
### Project Helpers

Contains functions for project introspection.

The primary function of this module is to provide the ability to query
information about projects dynamically, instead of hardcoding those
details in the build script, which require extra work to maintain when
configuration changes are made.

#>

function get-elements([xml]$doc, [string]$xpath) { <#
  Gets an element from a .csproj file using the given XPath expression
  #>
  if ($doc -eq $null)  { throw "get-elements requires a valid xml document!" }
  if (isEmpty($xpath)) { throw "get-elements requires a valid xpath expression!" }

  $nsmgr = new-object System.Xml.XmlNamespaceManager -ArgumentList $doc.NameTable
  $nsmgr.AddNamespace('a', 'http://schemas.microsoft.com/developer/msbuild/2003')
  $nodes = $doc.SelectNodes($xpath, $nsmgr)
  return $nodes
}

function get-containsXpath([string]$attr, [string]$val) { <#
  Builds a `contains` XPath expression for the given attribute and value
  #>
  return "contains(@$attr, `"$val`")"
}

function get-configurations([xml]$csproj) { <#
  Loads build configurations from the provided .csproj XML document
  #>
  if ($csproj -eq $null) {
    throw "get-configurations requires a valid xml document!"
  }

  $configurations    = @{}
  $configNamePattern = ".*== '((\w+)\|(\w+))'"
  $containsConfig    = get-containsxpath 'Condition' '$(Configuration)|$(Platform)'
  $xml               = get-elements $csproj "//a:PropertyGroup[$containsConfig]"

  foreach ($configuration in $xml) {
    $match    = [regex]::match($configuration.Condition, $configNamePattern)
    $name     = $match.Groups[1].Value
    $type     = $match.Groups[2].Value
    $platform = $match.Groups[3].Value
    $config = make @{
      "Configuration" = $type
      "Platform"      = $platform
      "OutputPath"    = $configuration.OutputPath.InnerText;
    }
    $configurations.Add($name, $config)
  }
  return $configurations
}

function get-csprojs([string]$baseDirectory) { <#
  Gets a list of all .csproj files located somewhere in
  $baseDirectory's tree. Paths are absolute, not relative.
  #>
  if (isEmpty($baseDirectory))     { throw "get-csprojs requires a valid base directory path!" }
  if (!(test-path $baseDirectory)) { throw "$baseDirectory does not exist!" }

  return ls (join-path $baseDirectory "*.csproj") -recurse | foreach { $_.FullName }
}

function get-projects([string]$baseDirectory) { <#
  Discovers and loads relevant project information for all projects in
  the given base directory. Recursively searches the entire tree.
  #>
  if (isEmpty($baseDirectory))     { throw "get-projects requires a valid base directory path!" }
  if (!(test-path $baseDirectory)) { throw "$baseDirectory does not exist!" }

  $paths    = get-csprojs $baseDirectory
  $projects = @{}
  foreach ($path in $paths) {
    # Load csproj as xml document
    $csproj = [xml](get-content $path)
    # Fetch nodes of interest
    $projectInfo    = get-elements $csproj "//a:PropertyGroup[1]"
    $projectName    = $projectInfo.AssemblyName
    $projectType    = $projectInfo.OutputType
    if ($projectInfo.TestProjectType -ne $null) {
      $projectType = $projectInfo.TestProjectType
    }
    $projectPath    = [System.IO.Path]::GetDirectoryName($path)
    $projectFile    = [System.IO.Path]::GetFileName($path)
    $configurations = get-configurations $csproj
    # Compose hash table of project information
    $project = @{
      "ProjectType"    = $projectType;
      "ProjectPath"    = $projectPath;
      "ProjectFile"    = $projectFile;
      "Configurations" = $configurations
    }
    $projects.Add($projectName, $project)
  }
  return $projects;
}

function generate-assembly-info { <#
  Generates AssemblyInfo.cs for a given project
  #>
  param(
    [string]$clsCompliant = "true",
    [string]$title,
    [string]$description,
    [string]$company,
    [string]$product,
    [string]$copyright,
    [string]$file = $(throw "file is a required parameter.")
  )
  $versionInfo = git-describe
  $version = "${versionInfo.Version}.${versionInfo.Revision}"
  $asmInfo = @"
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("$title")]
[assembly: AssemblyDescription("$description")]
[assembly: AssemblyVersion("$version")]
[assembly: AssemblyFileVersion("$version")]
[assembly: AssemblyInformationalVersion("$version / ${versionInfo.Commit}")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("$company")]
[assembly: AssemblyProduct("$product")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyDelaySign(false)]
[assembly: CLSCompliant($clsCompliant)]
[assembly: ComVisible(false)]
"@

  $dir = [System.IO.Path]::GetDirectoryName($file)
  if ([System.IO.Directory]::Exists($dir) -eq $false) {
    write-host "Creating directory $dir"
    [System.IO.Directory]::CreateDirectory($dir)
  }
  write-host   "Generating assembly info file: $file"
  write-output $asmInfo > $file
}