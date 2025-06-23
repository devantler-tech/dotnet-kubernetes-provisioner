# ☸️ .NET Kubernetes Provisioner

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Test](https://github.com/devantler-tech/dotnet-kubernetes-provisioner/actions/workflows/test.yaml/badge.svg)](https://github.com/devantler-tech/dotnet-kubernetes-provisioner/actions/workflows/test.yaml)
[![codecov](https://codecov.io/gh/devantler-tech/dotnet-kubernetes-provisioner/graph/badge.svg?token=RhQPb4fE7z)](https://codecov.io/gh/devantler-tech/dotnet-kubernetes-provisioner)

Simple provisioners that can provision Kubernetes and Kubernetes resources.

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

# For provisioning manifests with Kubectl
dotnet add package Devantler.KubernetesProvisioner.Deployment.Kubectl

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
