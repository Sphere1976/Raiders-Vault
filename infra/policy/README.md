# Policy as Code

This folder contains Open Policy Agent/Rego examples for reviewing Raiders Vault Kubernetes manifests.

Example validation flow:

```bash
kubectl kustomize infra/kubernetes/base > /tmp/raiders-vault.yaml
conftest test /tmp/raiders-vault.yaml --policy infra/policy
```

Policies enforce:

- CPU requests for deployments
- Memory limits for deployments
- Ingress-based exposure instead of direct LoadBalancer services
- Host rules on ingress resources
