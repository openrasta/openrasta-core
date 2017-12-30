# Change Log
All notable changes to OpenRasta's asp.net hosting will be documented in this file.
OpenRasta adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### ADDED
### CHANGED
### FIXED
### Known issues

## 2.6.0-preview.1 [2017-12-30]
### ADDED
 - As there's an async OpenRasta, so the asp.net hosting one evolves. It can
   now run your code... Eventually.

### CHANGED
 - We now target .net 4.6.1, as apaprently 4.6.2 was a naughty child and no
   one likes them.

### FIXED
 - The uninstall wasn't really uninstalling because it wasn't being shipped
   in the nuget package. The uninstall script has now been reprimanded and
   will not do it again. It now only removes the web.config section as it is
   no longer needed. Death by a 1000 angle brackets!
 
### Known issues
 - The integrated pipeline mode is a bit hairy, so there may be some compatibility
   issues. For example, we don't currently capture and restore thread identity
   as we move through the asp.net pipeline stages. We may fix it eventually though.

## 2.5.2000 - [2017-12-12]

Sync'ing the version between hosting and core, it's tidier...

### FIXED
 - Under some unknown conditions, the logger would be null, this may have now been fixed.

## 2.5.25 - [2015-01-27]
### ADDED
 - Native handlers are pretty slow to enumerate, they're now cached so we
   can run as quick as the 🌬. [#16]

### CHANGED
 - We used not to handle `/` by default, we do now. The setting in web.config
   is still there if you'd rather not.

## 2.5.22 - [2014-10-08]
This build has no code changes

## 2.5.20 - [2014-10-08]
### FIXED
 - The HTTP module calling OpenRasta tries to be a good neighbour and does
   not take over requests that other http modules are already handling.
   We used to match on path and query instead of only on the path, which was
   a bit rude. [#15]

## 2.5.17 - [2014-07-01]

Initial NuGet version of the 2.5 release.

### ADDED
 - Adding the package now configures your web.config correctly, because no
   one likes messing with XML!

﻿