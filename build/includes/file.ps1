<#
### File Helpers

This module includes functions for working with files and
directories, manipulating paths, etc.

#>

function join-paths { <#
  Mimics the behavior of Path.Combine

  Example:
    join-paths "C:", "foo", "bar.txt"
  #>
  param(
    [parameter(
      ValueFromPipeline=$true,
      ValueFromPipelineByPropertyName=$true
    )]
    [string[]]$parts = @()
  )

  $result = ""
  foreach ($part in $parts) {
    if (isEmpty($result)) { $result = $part }
    else { $result = join-path $result $part }
  }

  return $result
}

function mkdirp($directory = ${ throw "mkdirp requires a valid path! "}) { <#
  Mimics the behavior of mkdir -p.

  Silently creates a directory if it doesn't exist,
  creating paths along the way as necessary. If it does
  exist, it still behaves as though it did.
  #>
  mkdir $directory -ErrorAction -SilentlyContinue | out-null
}

function delete { <#
  An alternate version of remove-item which more closely emulates
  POSIX rm.

  Examples:
    rm C:\foo\bar\baz.txt
    rm -r C:\foo\bar
    rm -rf C:\foo
  #>
  param(
    [parameter(
      ValueFromPipeline=$true,
      ValueFromPipelineByPropertyName=$true
    )]
    [string[]]$paths = @(),
    [alias('f')][switch]$force,
    [alias('r')][switch]$recurse,
    [alias('rf')][switch]$forceRecurse
  )

  $args = ""
  $errAction = "-ErrorAction SilentlyContinue"

  if ($recurse.IsPresent)      { $args = "-recurse $args" }
  if ($force.IsPresent)        { $args = "-force $args" }
  if ($forceRecurse.IsPresent) { $args = "-force -recurse $args" }

  foreach ($path in $paths) {
    if (isEmpty($path)) { continue }
    else { invoke-expression "remove-item $path $args $errAction" | out-null }
  }
}
