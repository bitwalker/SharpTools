Properties {
  # Project Info
  $solutionName   = "SharpTools"
  $nugetProject   = "SharpTools"
  $configuration  = "Release"
  $currentGitTag  = get-git-tag
  $revisionNumber = get-commits-since-tag $currentGitTag
  $version        = "${currentGitTag}.${revisionNumber}"
  # Project Paths
  $scriptPath     = split-path -parent $PSCommandPath
  $buildDir       = $scriptPath
  $net45Dir       = join-path $buildDir "net45"
  $sln            = join-path $scriptPath "..\$solutionName.sln"
  $nuget          = join-path $scriptPath "nuget.exe"
  $packageSpec    = join-path $scriptPath "$nugetProject.nuspec"
  $projectPath    = join-path $scriptPath "..\$nugetProject"
  $releaseDir     = join-path $projectPath "bin\$configuration"
  # Build Configuration
  $verbosityLevel = "normal"
  $runTests       = $true
}

# Ensure .NET 4+ is used
Framework "4.0"
# Customize task name output
FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))
# Include helper functions
(ls "$scriptPath\*.ps1" -exclude "default.ps1") | foreach {
  include (join-path $scriptPath $_)
}

task Default -depends Rebuild
task Rebuild -depends Build

task Clean {
  write-host "Cleaning package artifacts..." -ForegroundColor Green
  remove-item -force -recurse $net45Dir -ErrorAction SilentlyContinue

  write-host "Cleaning SharpTools solution..." -ForegroundColor Green
  exec { msbuild "$sln" /t:Clean /p:Configuration=$configuration /v:quiet }
}

task Prepare -depends Clean {
  $assemblyInfo = join-path $projectPath "Properties\AssemblyInfo.cs"
  generate-assembly-info `
    -file $assemblyInfo `
    -title "SharpTools $version" `
    -description "A collection of practical C# code to build upon" `
    -company "Paul Schoenfelder" `
    -product "SharpTools $version" `
    -version $version `
    -copyright "Paul Schoenfelder 2014"

  new-item $net45Dir -itemType directory
}

task Build -depends Prepare {
  write-host "Building SharpTools solution..." -ForegroundColor Green
  msbuild "$sln" `
    /t:Build /p:Configuration=$configuration `
    /v:quiet `
    /p:OutDir="$net45Dir\x86" `
    /p:Platform=x86 /p:TargetFrameworkVersion="V4.5"
  msbuild "$sln" `
    /t:Rebuild /p:Configuration=$configuration `
    /v:quiet `
    /p:OutDir="$net45Dir\x64" `
    /p:Platform=x64 /p:TargetFrameworkVersion="V4.5"
}

task TestBuild {
  write-host "Cleaning test project..." -ForegroundColor Green
  exec { msbuild "$sln" /t:Clean /p:Configuration=Debug /v:quiet }
  write-host "Building test project..." -ForegroundColor Green
  msbuild "$sln" `
    /t:Build /p:Configuration=Debug `
    /v:quiet `
    /p:Platform="Any CPU" /p:TargetFrameworkVersion="V4.5"
}

task Test -depends TestBuild -precondition { return $runTests } {
  $testProj     = join-path $scriptPath "..\SharpTools.Test"
  $testArtifact = join-path $testProj "bin\Debug\SharpTools.Test.dll"
  $msTestDir    = join-path ($env:programfiles + " (x86)") "Microsoft Visual Studio 12.0\Common7\IDE"
  $mstest       = join-path $msTestDir "mstest.exe"
  # Run tests
  & $runner /testcontainer:$testArtifact /noresults
}

task Package -depends Build, Test {
  write-host "Generating nuget package..." -ForegroundColor Green
  & $nuget pack $packageSpec `
    -o $buildDir `
    -Version $version `
    -Symbols `
    -Verbose `
    -Verbosity "$verbosityLevel" `
    -Configuration "$packageConfiguration"
}