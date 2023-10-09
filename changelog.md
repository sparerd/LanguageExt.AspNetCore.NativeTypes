# Changelog
All notable changes to LanguageExt.AspNetCore.NativeTypes will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

Change entries are optionally tagged with their corresponding work item or pull request. The tags are:
- [PR-#]: A pull request number which can be viewed at https://github.com/sparerd/LanguageExt.AspNetCore.NativeTypes/pulls

## [0.2.0] - 2023-10-09
### Added
- Added support for configuring this library from IMvcCoreBuild instances

### Changed
- Improved API for configuring features of this library

## [0.1.0] - 2023-06-28
### Added
- Added support for returning Eff/Aff from controller methods
- Added support for returning Option from controller methods
- Added configurable JSON serialization for Option types
- Added model binding support for Option in controllers
- Added model binding support for Seq and Lst in controllers
