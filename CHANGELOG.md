# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.9.2] - 2025-07-03

### Fixed
- Fixed code formatting and indentation in FluxProvisioner.cs

### Technical
- Small indentation fix for proper code formatting consistency

## [1.9.1] - 2025-07-02

### Fixed
- **deps**: Update dependency duende.identitymodel to 7.1.0 (#311)
- Update PushAsync method to use UTC time for epoch conversion
- Remove Duende.IdentityModel package reference from Flux project

### Changed
- Replaced `DateTime.Now.ToEpochTime()` with `DateTimeOffset.UtcNow.ToUnixTimeSeconds()` for better UTC time handling
- Removed unused Duende.IdentityModel dependency from FluxProvisioner

## [1.9.0] - Previous Release

For earlier changes, see the commit history on GitHub.