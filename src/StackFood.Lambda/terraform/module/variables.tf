variable "function_name" {
  description = "The name of the Lambda function."
}

variable "description" {
  description = "Description of the Lambda function."
}

variable "runtime" {
  description = "The runtime for the Lambda function."
}

variable "handler" {
  description = "The handler for the Lambda function."
}

variable "package_type" {
  description = "The package type for the Lambda function (zip or image)."
}

variable "filename" {
  description = "The path to the deployment package."
}

variable "source_code_hash" {
  description = "A base64-encoded representation of the contents of the deployment package."
}

variable "image_uri" {
  description = "The URI of the Docker image if the package type is image."
}

variable "vpc_id" {
  description = "The VPC ID where the Lambda function will be deployed."
}

variable "subnet_ids" {
  description = "List of subnet IDs for the Lambda function."
  type        = list(string)
}

variable "lambda_role_name" {
  description = "The name of the IAM role for Lambda."
}
