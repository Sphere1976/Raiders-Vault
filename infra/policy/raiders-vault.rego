package raidersvault.deploy

deny[msg] {
  input.kind == "Deployment"
  container := input.spec.template.spec.containers[_]
  not container.resources.requests.cpu
  msg := sprintf("container %s must define cpu requests", [container.name])
}

deny[msg] {
  input.kind == "Deployment"
  container := input.spec.template.spec.containers[_]
  not container.resources.limits.memory
  msg := sprintf("container %s must define memory limits", [container.name])
}

deny[msg] {
  input.kind == "Service"
  input.spec.type == "LoadBalancer"
  msg := "services must not expose LoadBalancer directly; use managed ingress"
}

deny[msg] {
  input.kind == "Ingress"
  not input.spec.rules
  msg := "ingress must define host rules"
}
