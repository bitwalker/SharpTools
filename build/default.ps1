# Powershell preferences
$VerobsePreference     = 'SilentlyContinue'
$WarningPreference     = 'Continue'
$ErrorActionPreference = 'Stop'

# Load core functions
. ".\core.ps1"

trap {
  $location      = $error[0].InvocationInfo.PositionMessage
  $exception     = $error[0].Exception
  $exceptionType = $exception.GetType().Name
  $message       = $exception.Message

  error "$exceptionType: $message"
  error $location
}

# Load submodules
$submodules = ls .\include\*.ps1 -exclude @("core.ps1")
foreach ($mod in $submodules) {
  info "Loading extensions from $mod..."
  . $mod
}

properties { # Solution Info
  $scripts_dir   = resolve-path $PSScriptRoot
  $root_dir      = split-path $scripts_dir
  $solution_name = "SharpTools"
  $solution_file = join-path $root_dir "$solution_name.sln"
}

properties { # Nuget Packaging
  $nuget        = join-path $scripts_dir "nuget.exe"
  $package_name = $solution_name
  $package_spec = join-path $scripts_dir "$package_name.nuspec"
  $package_dir  = join-path $scripts_dir "package"
  $net45_dir    = join-path $package_dir "net45"
}

properties { # Project Info
  $projects      = get-projects $root_dir
  $configuration = "Release"  # The build configuration to use
  $platform      = "x64"      # The platform to build for
  $verbosity     = "minimal"  # quiet, minimal, normal, detailed
  $clsCompliant  = "true"     # produce more stringent build warnings
  $runTests      = $true      # If true, will run tests prior to release
}

properties { # Environment Configuration
  $environment = "dev"
}

properties { # Versioning
  $currentGitTag  = get-git-tag
  $revisionNumber = get-commits-since-tag $currentGitTag
  $version        = "${currentGitTag}.${revisionNumber}"
}

task default -depends rebuild
task rebuild -depends build

task clean {
  info "Cleaning solution..."
  exec { msbuild "$solution_file" /t:Clean /p:Configuration=$configuration /v:$verbosity }
}

task precompile {
  # Generate AssemblyInfo.cs for all projects
  foreach ($project in $projects.GetEnumerator()) {
    $projectName  = $project.Name
    $projectInfo  = $project.Value
    $assemblyInfo = join-path $projectInfo.ProjectPath "Properties\AssemblyInfo.cs"

    generate-assembly-info `
      -file $assemblyInfo `
      -clsCompliant $clsCompliant `
      -title "$projectName $version" `
      -description "A collection of practical C# code to build upon" `
      -company "Paul Schoenfelder" `
      -product "$projectName $version" `
      -version $version `
      -copyright "Paul Schoenfelder 2014"
  }

  mkdirp $net45_dir
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
  mstest $testProject
}

task Package -depends Build, Test {
  info "Cleaning package artifacts..."
  remove -rf $net45Dir

  rm -recurse
  write-host "Generating nuget package..." -ForegroundColor Green
  & $nuget pack $packageSpec `
    -o $buildDir `
    -Version $version `
    -Symbols `
    -Verbose `
    -Verbosity "$verbosityLevel" `
    -Configuration "$packageConfiguration"
}