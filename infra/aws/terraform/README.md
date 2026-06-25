# AWS Terraform Blueprint

This folder demonstrates hands-on AWS and Infrastructure as Code experience for Raiders Vault.
It provisions a simple production-style ECS Fargate deployment:

- VPC with public subnets across two availability zones
- Application Load Balancer
- ECS Fargate cluster, task definition, and service
- CloudWatch log group
- Security groups scoped from the ALB to the service

## Validate

```bash
terraform fmt -check
terraform init
terraform validate
```

## Deploy

```bash
terraform apply \
  -var="container_image=<account>.dkr.ecr.us-east-1.amazonaws.com/raiders-vault:latest" \
  -var="container_port=8080"
```

This blueprint intentionally keeps state management, Route 53, ACM, WAF, and private NAT topology out
of the default portfolio path. Those would be natural next production hardening steps.
