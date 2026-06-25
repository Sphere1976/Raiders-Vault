variable "aws_region" {
  description = "AWS region for the Raiders Vault stack."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Name prefix for provisioned resources."
  type        = string
  default     = "raiders-vault"
}

variable "container_image" {
  description = "Container image to deploy to ECS Fargate."
  type        = string
  default     = "public.ecr.aws/docker/library/nginx:stable-alpine"
}

variable "container_port" {
  description = "Port exposed by the application container."
  type        = number
  default     = 8080
}
