<#
### Git Helpers

This module provides helper functions for pulling information
about the git repo in the current working directory.
#>

function git-shorthash { <#
  Gets the short hash of the HEAD commit, i.e. b75d6ba
  #>
  $gitLog = git log --oneline -1
  return $gitLog.Split(' ')[0]
}

function git-revision([string]$version) { <#
  Gets the number of commits since the provided version. If no version is
  provided, it returns the total number of commits in the repository
  #>
  if (isEmpty($version)) { return (git rev-list HEAD --count) -as [int] }
  else                   { return (git rev-list $version..HEAD) -as [int] }
}

function git-describe { <#
  Gets a friendly description of the HEAD commit, i.e. 1.0.0-5-b75d6ba
  The parts are {tag}-{number of commits since tag}-{shorthash of HEAD}.
  Returns the description as an object:
    description.Version  - The version number, i.e. 1.0.0
    description.Revision - The number of commits since the version was tagged
    description.Commit   - The short hash of the HEAD commit
  #>
  $description = git describe --tags
  # If git describe returns an error, there have been no versions tagged
  if (!$?) {
    # Default to version 1.0.0 for untagged versions
    $name = "1.0.0"
    $revs = git-revision
    $hash = git-shorthash
    return make @{"Version" = $name; "Revision" = $revs; "Commit" = $hash}
  } else {
    # $description is in the format {version}-{rev}-{commit hash}
    $parts = $description.Split('-')
    $name  = $parts[0]
    $revs  = $parts[1] -as [int]
    $hash  = $parts[2]
    return make @{"Version" = $name; "Revision" = $revs; "Commit" = $hash}
  }
}