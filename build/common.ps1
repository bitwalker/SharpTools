# Gets the short hash of the most recent commit
function get-last-commit-hash {
  $gitLog = git log --oneline -1
  return $gitLog.Split(' ')[0]
}

# Gets the current git tag
function get-git-tag {
  $tag = git describe --tags --abbrev=0
  return $tag
}

# Gets the number of commits since the provided tag
function get-commits-since-tag {
  param([string]$tag = "HEAD")
  $revlist = git rev-list $tag..HEAD
  if ($revlist -eq $null) {
    return 0
  }
  return $revlist.Split('\n').Length
}

# Generates AssemblyInfo.cs
function generate-assembly-info {
  param(
    [string]$clsCompliant = "true",
    [string]$title,
    [string]$description,
    [string]$company,
    [string]$product,
    [string]$copyright,
    [string]$version,
    [string]$file = $(throw "file is a required parameter.")
  )
  $commit = get-last-commit-hash
  $asmInfo = @"
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("$title")]
[assembly: AssemblyDescription("$description")]
[assembly: AssemblyVersion("$version")]
[assembly: AssemblyInformationalVersion("$version / $commit")]
[assembly: AssemblyFileVersion("$version")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("$company")]
[assembly: AssemblyProduct("$product")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyDelaySign(false)]
[assembly: CLSCompliant($clsCompliant)]
[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("SharpTools.Test")]
"@

  $dir = [System.IO.Path]::GetDirectoryName($file)
  if ([System.IO.Directory]::Exists($dir) -eq $false) {
    write-host "Creating directory $dir"
    [System.IO.Directory]::CreateDirectory($dir)
  }
  write-host   "Generating assembly info file: $file"
  write-output $asmInfo > $file
}
