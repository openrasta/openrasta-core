# Change Log
All notable changes to OpenRasta will be documented in this file.
OpenRasta adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### ADDED
 - As there's an async OpenRasta, so the asp.net hosting one evolves. It can
   now run both sync and async pipelines transparently, in theory.

### CHANGED
 - Now targets .net 4.5

### FIXED
 - The uninstall wasn't really uninstalling because it wasn't being shipped
   in the nuget package. The uninstall script has now been reprimanded and
   will not do it again.

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