variable "aws_region" {
  description = "The AWS region to deploy the resources."
  default     = "us-east-1"
}

variable "vpc_name" {
  description = "The name of the VPC."
}

variable "environment" {
  description = "The environment for the deployment."
}

variable "tags" {
  description = "Tags to assign to the resources."
  type        = map(string)
}

variable "vpc_cidr_blocks" {
  description = "CIDR block for the VPC."
  type        = string
}

variable "private_subnets" {
  description = "List of private subnet CIDR blocks."
  type        = list(string)
}

variable "lambda_functions" {
  description = "Map of Lambda function configurations."
  type        = map(object({
    description   = string
    package_type  = string
    runtime       = string
    handler       = string
    filename      = string
    source_code_hash = string
    image_uri     = string
  }))
}

variable "lambda_role_name" {
  description = "The name of the IAM role for Lambda."
}
