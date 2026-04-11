terraform {
  required_version = ">= 1.6.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  backend "s3" {
    bucket = "aipromo-terraform-state"
    key    = "dev/terraform.tfstate"
    region = "ap-southeast-1"
  }
}

provider "aws" {
  region = var.aws_region
}

module "vpc" {
  source = "../../modules/vpc"

  project     = var.project
  environment = var.environment
  vpc_cidr    = var.vpc_cidr
}

module "ec2" {
  source = "../../modules/ec2"

  project          = var.project
  environment      = var.environment
  vpc_id           = module.vpc.vpc_id
  public_subnet_id = module.vpc.public_subnet_ids[0]
  instance_type    = var.ec2_instance_type
  key_name         = var.ec2_key_name
  allowed_cidr     = var.allowed_cidr
}

module "rds" {
  source = "../../modules/rds"

  project              = var.project
  environment          = var.environment
  vpc_id               = module.vpc.vpc_id
  private_subnet_ids   = module.vpc.private_subnet_ids
  ec2_security_group_id = module.ec2.security_group_id
  db_name              = var.db_name
  db_username          = var.db_username
  db_password          = var.db_password
  instance_class       = var.rds_instance_class
}

module "s3" {
  source = "../../modules/s3"

  project     = var.project
  environment = var.environment
}