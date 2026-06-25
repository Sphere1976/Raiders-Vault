output "application_url" {
  description = "Public HTTP URL for the Raiders Vault application load balancer."
  value       = "http://${aws_lb.app.dns_name}"
}

output "ecs_cluster_name" {
  description = "ECS cluster name."
  value       = aws_ecs_cluster.main.name
}

output "cloudwatch_log_group" {
  description = "Application log group."
  value       = aws_cloudwatch_log_group.app.name
}
