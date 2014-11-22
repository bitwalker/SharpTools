Properties {
  $scriptPath = split-path -parent $PSCommandPath
  $sln   = join-path $scriptPath "..\SharpTools.sln"
  $nuget = join-path $scriptPath ".\NuGet.exe"
  $packagePath = join-path $scriptPath "..\SharpTools"
  $packageSpec = join-path $packagePath "SharpTools.csproj.nuspec"
  $verbosePacking = true
  $verbosityLevel = "normal"
  $packSymbols = true
  $packageConfiguration = "Release"
}

FormatTaskName (("-"*25) + "[{0}]" + ("-"*25))

task Default -depends Rebuild
task Rebuild -depends Clean, Build

task Package {
  write-host "Generating nuget package..." -ForegroundColor Green
  exec {
    & $nuget pack `
      $packageSpec #-Verbose -Symbols -Build `
      #Verbosity="$verbosityLevel" `
      #Configuration="$packageConfiguration"
  }
}

task Build -depends Clean {
  write-host "Building SharpTools solution..." -ForegroundColor Green
  exec { msbuild "$sln" /t:Build /p:Configuration=Release /v:quiet }
}

task Clean {
  write-host "Cleaning SharpTools solution..." -ForegroundColor Green
  exec { msbuild "$sln" /t:Clean /p:Configuration=Release /v:quiet }
}