# Get path to default task
$scriptPath  = split-path $MyInvocation.InvocationName
$defaultTask = join-path $scriptPath "build\default.ps1"
$psakeModule = join-path $scriptPath "build\psake.psm1"
# Launch default build task
import-module $psakeModule
invoke-psake $defaultTask -framework '4.0' $ARGS[0]