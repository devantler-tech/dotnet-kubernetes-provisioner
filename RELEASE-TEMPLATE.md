# Release Template

Use this template for documenting future releases of the .NET Kubernetes Provisioner.

## Release Process
1. **Code Changes**: Implement and test changes
2. **Commit Messages**: Follow conventional commits format for semantic release
3. **Tag Creation**: Semantic release will create tags automatically
4. **Documentation**: Update CHANGELOG.md and create release notes
5. **Publishing**: GitHub Actions will handle package publishing

## Release Documentation Checklist
- [ ] Update CHANGELOG.md with new version
- [ ] Create release notes file (RELEASE-vX.X.X.md)
- [ ] Verify all packages are updated
- [ ] Check documentation for version references
- [ ] Validate GitHub Actions workflows completion
- [ ] Confirm NuGet package publishing

## Version Types
- **Major** (X.0.0): Breaking changes
- **Minor** (0.X.0): New features, backward compatible
- **Patch** (0.0.X): Bug fixes, backward compatible

## Commit Message Format
```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

## Package Structure
All packages follow the naming convention:
- `DevantlerTech.KubernetesProvisioner.{Component}.{Implementation}`
- Core interfaces: `DevantlerTech.KubernetesProvisioner.{Component}.Core`