# Change Log
All notable changes to OpenRasta will be documented in this file.
OpenRasta adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added
- A new pipeline with "double tap" semantics, which makes most of the old crazy
  pipeline code redundant
- Obsoleting `Abort`, you should instead throw an exception from the contributor: aborting
   was meant only for exceptional circumstances, and surely that's what 
   exceptions are for.
  `Finished` was always a bit of a lie. You reached the end of the line or you didn't, and that's all that ever really mattered.

### Changed
 - We used to not be the old guard, but cool kids have moved on, so we have to 
   follow. .net 4.5 is now a minimum.
 - The AppVeyor build is now under the OpenRasta organisation. We were not
   really organised before, so we (well, @holytshirt) sorted it out, and we feel
   much cleaner.
 - `IPipeline` used to have a read-only `IList` Contributors property. This was
   really as useless as a web framework with no web, so it's now an
   `IEnumerable`.

### Fixed

### Removed
