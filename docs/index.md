# BeyondTrust Secret Safe Provider

Terraform provider for **BeyondTrust Secret Safe** using **.NET Native AOT**, **Slim Builder**, and **Terraform Plugin Protocol v5.2**.

## Overview

The BeyondTrust Secret Safe provider allows you to manage and retrieve secrets from BeyondTrust Secret Safe in your Terraform configurations.

## Supported Data Sources

- `secretsafe_credential_data` — Retrieve username/password credentials from a secret
- `secretsafe_download_file_data` — Download file content from a secret as base64

## Example Usage

```hcl
terraform {
  required_providers {
    secretsafe = {
      source = "albertomonteiro/beyondtrust-secretsafe"
    }
  }
}

provider "secretsafe" {
  # Configure your Secret Safe connection
}

# Retrieve credential data
data "secretsafe_credential_data" "example" {
  secret_id = "your-secret-id"
}

output "username" {
  value = data.secretsafe_credential_data.example.username
}

output "password" {
  value     = data.secretsafe_credential_data.example.password
  sensitive = true
}

# Download file content
data "secretsafe_download_file_data" "example" {
  secret_id = "your-file-secret-id"
}

output "file_content" {
  value     = data.secretsafe_download_file_data.example.file_content_base64
  sensitive = true
}
```
