# ☸️ .NET Kubernetes Provisioner

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Test](https://github.com/devantler-tech/dotnet-kubernetes-provisioner/actions/workflows/test.yaml/badge.svg)](https://github.com/devantler-tech/dotnet-kubernetes-provisioner/actions/workflows/test.yaml)
[![codecov](https://codecov.io/gh/devantler-tech/dotnet-kubernetes-provisioner/graph/badge.svg?token=RhQPb4fE7z)](https://codecov.io/gh/devantler-tech/dotnet-kubernetes-provisioner)

Simple provisioners that can provision Kubernetes and Kubernetes resources.

<details>
  <summary>Show/hide folder structure</summary>

<!-- readme-tree start -->
```
.
├── .github
│   └── workflows
├── src
│   ├── Devantler.KubernetesProvisioner.CNI.Cilium
│   ├── Devantler.KubernetesProvisioner.CNI.Core
│   ├── Devantler.KubernetesProvisioner.Cluster.Core
│   ├── Devantler.KubernetesProvisioner.Cluster.K3d
│   ├── Devantler.KubernetesProvisioner.Cluster.Kind
│   ├── Devantler.KubernetesProvisioner.GitOps.Core
│   ├── Devantler.KubernetesProvisioner.GitOps.Flux
│   └── Devantler.KubernetesProvisioner.Resources.Native
└── tests
    ├── Devantler.KubernetesProvisioner.CNI.Cilium.Tests
    │   ├── CiliumProvisionerTests
    │   └── assets
    ├── Devantler.KubernetesProvisioner.CNI.Core.Tests
    ├── Devantler.KubernetesProvisioner.Cluster.Core.Tests
    ├── Devantler.KubernetesProvisioner.Cluster.K3d.Tests
    │   ├── K3dProvisionerTests
    │   └── assets
    ├── Devantler.KubernetesProvisioner.Cluster.Kind.Tests
    │   ├── KindProvisionerTests
    │   └── assets
    ├── Devantler.KubernetesProvisioner.GitOps.Core.Tests
    ├── Devantler.KubernetesProvisioner.GitOps.Flux.Tests
    │   ├── FluxProvisionerTests
    │   └── assets
    │       └── k8s
    │           ├── apps
    │           ├── clusters
    │           │   └── test-flux-cluster
    │           │       └── flux-system
    │           └── infrastructure
    │               └── controllers
    └── Devantler.KubernetesProvisioner.Resources.Native.Tests
        ├── KubernetesResourceProvisionerTests
        └── assets

38 directories
```
<!-- readme-tree end -->

</details>

## Prerequisites

- [.NET](https://dotnet.microsoft.com/en-us/)

## 🚀 Getting Started

To get started, you can install the packages from NuGet.

```bash
# For provisioning a K3d cluster
dotnet add package Devantler.KubernetesProvisioner.Cluster.K3d

# For provisioning a Kind cluster
dotnet add package Devantler.KubernetesProvisioner.Cluster.Kind

# For provisioning Cilium CNI
dotnet add package Devantler.KubernetesProvisioner.CNI.Cilium

# For provisioning Flux GitOps tooling
dotnet add package Devantler.KubernetesProvisioner.GitOps.Flux

# For provisioning native Kubernetes resources
dotnet add package Devantler.KubernetesProvisioner.Resources.Native
```

## 📝 Usage

To use the provisioners, all you need to do is to create and use a new instance of the provisioner.

```csharp
using Devantler.KubernetesProvisioner.Cluster.K3d;

var provisioner = new K3dProvisioner();

await provisioner.ProvisionAsync("my-cluster", "path/to/config.yaml", CancellationToken.None);
```
