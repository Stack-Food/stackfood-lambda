output "lambda_function_names" {
  description = "List of Lambda function names."
  value       = [for fn in module.lambda : fn.function_name]
}