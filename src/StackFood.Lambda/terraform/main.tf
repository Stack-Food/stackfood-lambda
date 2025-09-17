#######################
# Main Terraform Configuration
#######################

provider "aws" {
  region = var.aws_region
}

# VPC Module
module "vpc" {
  source = "../modules/vpc/"

  # Configurações gerais
  vpc_name         = var.vpc_name
  environment      = var.environment
  tags             = var.tags

  # Configurações da VPC
  vpc_cidr_blocks          = var.vpc_cidr_blocks
  vpc_enable_dns_support   = true
  vpc_enable_dns_hostnames = true
  subnets_private          = var.private_subnets
  subnets_public           = var.public_subnets
}

# Lambda Functions
module "lambda" {
  source = "../modules/lambda/"
  for_each = var.lambda_functions

  # Configurações gerais
  function_name = each.key
  description   = each.value.description
  environment   = var.environment
  tags          = var.tags

  # Configurações da função
  package_type     = each.value.package_type
  runtime          = try(each.value.runtime, ".NET 8")
  handler          = try(each.value.handler, null)
  filename         = try(each.value.filename, null)
  source_code_hash = try(each.value.source_code_hash, null)
  image_uri        = each.value.image_uri

  # Configurações de rede
  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnet_ids

  # Configurações do IAM
  lambda_role_name = var.lambda_role_name
}