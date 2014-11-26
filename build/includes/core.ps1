<#
### Core

These are core functions used by everything else. This
script must be included before any other scripts (that rely
on these functions anyway).
#>

function make([hashtable]$properties) { <#
  Creates a new anonymous object from a hashtable.

  Example:
    > $person = make @{"first" = "PauL"; "last" = "Schoenfelder"}
    > $person.first
    Paul
  #>
  return new-object PSObject -property $properties
}

function isEmpty([string]$str = "") { <#
  Returns true if the provided string is null or empty
  #>
  return [System.String]::IsNullOrWhitespace($str)
}

<#
### Logging

Contains aliases writing colored messages to the console

#>
$colors = make @{
  "debug"   = "white";
  "info"    = "blue";
  "success" = "green"
  "warn"    = "yellow";
  "error"   = "red"
}

function log($message, $color = "white") {
  write-host $message -foregroundcolor $color
}

function debug($message) {
  log "DEBUG: $message" $colors.debug
}

function info($message) {
  log $message $colors.info
}

function success($message) {
  log $message $colors.success
}

function warn($message) {
  log $message $colors.warn
}

function error($message) {
  log $message $colors.error
}

function inspect([string]$message, $obj) {
  write-host
  info $message
  info "=========="
  write-output $obj
  write-host
}