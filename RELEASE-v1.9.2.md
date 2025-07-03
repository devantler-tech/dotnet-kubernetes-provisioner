# Release Documentation

## .NET Kubernetes Provisioner v1.9.2

### Release Status
✅ **Release v1.9.2 Generated Successfully**

### Release Assets
- **Git Tag**: v1.9.2 (exists on GitHub)
- **Commit Hash**: 80343f5029d550415e176c54170f6941bd9e6c99
- **Release Date**: July 3, 2025

### Documentation Updates
- ✅ CHANGELOG.md created with comprehensive release history
- ✅ Release notes prepared for GitHub release
- ✅ Package information documented

### Release Components

#### 1. Code Changes
- **File Modified**: `src/DevantlerTech.KubernetesProvisioner.GitOps.Flux/FluxProvisioner.cs`
- **Change Type**: Code formatting/indentation fix
- **Impact**: Improved code consistency, no functional changes

#### 2. NuGet Packages
The following packages are versioned as part of this release:
- `DevantlerTech.KubernetesProvisioner.Cluster.K3d`
- `DevantlerTech.KubernetesProvisioner.Cluster.Kind`
- `DevantlerTech.KubernetesProvisioner.CNI.Cilium`
- `DevantlerTech.KubernetesProvisioner.Deployment.Kubectl`
- `DevantlerTech.KubernetesProvisioner.GitOps.Flux`
- `DevantlerTech.KubernetesProvisioner.Resources.Native`

#### 3. Automation
- **Semantic Release**: Configured via `.releaserc`
- **GitHub Actions**: Release workflow in `.github/workflows/release.yaml`
- **Package Publishing**: Automated via `.github/workflows/publish.yaml`

### Verification Checklist
- ✅ Tag v1.9.2 exists on GitHub
- ✅ CHANGELOG.md created and committed
- ✅ Release notes prepared
- ✅ Dependencies properly referenced (FluxCLI v1.9.2)
- ✅ No breaking changes introduced
- ✅ Code quality improved

### Installation Instructions
Users can install the latest v1.9.2 packages using:

```bash
# For provisioning a K3d cluster
dotnet add package DevantlerTech.KubernetesProvisioner.Cluster.K3d

# For provisioning a Kind cluster
dotnet add package DevantlerTech.KubernetesProvisioner.Cluster.Kind

# For provisioning Cilium CNI
dotnet add package DevantlerTech.KubernetesProvisioner.CNI.Cilium

# For provisioning manifests with Kubectl
dotnet add package DevantlerTech.KubernetesProvisioner.Deployment.Kubectl

# For provisioning Flux GitOps tooling
dotnet add package DevantlerTech.KubernetesProvisioner.GitOps.Flux

# For provisioning native Kubernetes resources
dotnet add package DevantlerTech.KubernetesProvisioner.Resources.Native
```

### Next Steps
1. Automated workflows will handle package publishing
2. GitHub release will be created by semantic-release
3. NuGet packages will be available for consumption
4. Documentation is ready for users

---
**Release completed successfully on**: $(date)
**Automated by**: Semantic Release & GitHub Actions