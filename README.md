# BeyondTrust Secret Safe Terraform Provider

A Terraform provider for [BeyondTrust Secret Safe](https://www.beyondtrust.com/secrets-safe), implemented in **C# (.NET 10)** using **Native AOT** compilation.

## Features

- 🔐 Retrieve secrets from BeyondTrust Secret Safe
- 📦 Native AOT compiled for minimal dependencies (~15 MB)
- 🔄 Full support for Terraform Plugin Protocol v5.2
- 🛡️ mTLS with self-signed certificates
- ⚡ Fast, lightweight binary

## Requirements

- Terraform >= 0.12
- BeyondTrust Secret Safe instance with API access

## Installation

```hcl
terraform {
  required_providers {
    secretsafe = {
      source = "albertomonteiro/beyondtrust-secretsafe"
    }
  }
}
```

## Provider Configuration

The provider requires the following arguments:

- `runas` - (Required) User to authenticate in BeyondTrust Secret Safe
- `key` - (Required, Sensitive) The API key of BeyondTrust Secret Safe
- `baseUrl` - (Required) Base URL of the BeyondTrust Secret Safe instance

```hcl
provider "secretsafe" {
  runas   = "terraform-user"
  key     = var.secret_safe_api_key
  baseUrl = "https://secretsafe.example.com"
}
```

## Usage

### Retrieve Credentials

```hcl
data "secretsafe_credential_data" "example" {
  secret_id = "2e22e1b1-d5c2-4a17-bc90-1234567890ab"
}

output "username" {
  value = data.secretsafe_credential_data.example.username
}

output "password" {
  value     = data.secretsafe_credential_data.example.password
  sensitive = true
}
```

### Download File

```hcl
data "secretsafe_download_file_data" "example" {
  secret_id = "87654321-4321-4321-4321-abcdefgh1234"
}

output "file_name" {
  value = data.secretsafe_download_file_data.example.file_name
}

output "file_content" {
  value     = base64decode(data.secretsafe_download_file_data.example.file_content_base64)
  sensitive = true
}
```

## Data Sources

### `secretsafe_credential_data`

Retrieves username and password from a Secret Safe secret.

**Arguments:**
- `secret_id` (Required) - UUID of the secret in BeyondTrust Secret Safe

**Attributes:**
- `username` - Username from the secret
- `password` - Password from the secret (sensitive)

### `secretsafe_download_file_data`

Downloads file content from a Secret Safe secret.

**Arguments:**
- `secret_id` (Required) - UUID of the secret in BeyondTrust Secret Safe

**Attributes:**
- `file_name` - Name of the file stored in the secret
- `file_content_base64` - File content encoded in base64 (use `base64decode()` to get the actual content)

## Development

See [CLAUDE.md](./CLAUDE.md) for development instructions.

## License

Mozilla Public License 2.0 - see [LICENSE](./LICENSE)
