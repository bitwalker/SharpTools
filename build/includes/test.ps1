function mstest {
  param([string]$projectName)
  # Build the path to mstest
  $mstest = join-paths @(
    ($env:programfiles + " (x86)"),                   # Program files
    "Microsoft Visual Studio 12.0", "Common7", "IDE", # Visual Studio program files
    "mstest.exe"                                      # MSTest executable
  )
  # Get project artifact
  $projectDll = join-paths @($scriptPath, "..", $projectName, "bin", "Debug", "$projectName.dll")
  # Run tests
  & $mstest /testcontainer:$projectDll /noresults
}