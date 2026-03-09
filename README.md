# BeyondTrust Secret Safe Terraform Provider

A Terraform provider for [BeyondTrust Secret Safe](https://www.beyondtrust.com/secrets-safe), implemented in **C# (.NET 10)** with **Native AOT** compilation for minimal resource footprint.

## Features

- 🔐 **Retrieve & Manage Secrets** - Query credentials and file secrets from BeyondTrust Secret Safe
- 📦 **Native AOT Compiled** - Minimal binary size (~15 MB) with no runtime dependencies
- 🔄 **Full TPPv5.2 Support** - Implements Terraform Plugin Protocol v5.2 over gRPC
- 🛡️ **mTLS Security** - Self-signed certificates for encrypted provider-Terraform communication
- 📁 **Folder Management** - Create, read, and manage folders in Secret Safe
- ⚡ **Fast & Lightweight** - Optimized for performance and quick startup

## Requirements

- **Terraform** >= 0.12
- **BeyondTrust Secret Safe** instance with API access
- **Linux/Alpine** for production (Native AOT targets linux-musl-x64)

## Installation

### From Terraform Registry

```hcl
terraform {
  required_providers {
    secretsafe = {
      source  = "albertomonteiro/beyondtrust-secretsafe"
      version = "~> 0.0"
    }
  }
}
```

### Manual Installation (Development)

See [Development](#development) section below.

## Provider Configuration

The provider requires authentication credentials:

```hcl
provider "secretsafe" {
  runas    = "terraform-user"              # User to authenticate as
  key      = var.secret_safe_api_key       # API key (sensitive)
  baseUrl  = "https://secretsafe.example.com"  # Base URL of Secret Safe instance
  pwd      = var.secret_safe_password      # Optional: domain password for auth
}
```

### Configuration Arguments

| Argument  | Type    | Required | Sensitive | Description |
|-----------|---------|----------|-----------|-------------|
| `runas`   | String  | Yes      | No        | User account to authenticate in BeyondTrust Secret Safe |
| `key`     | String  | Yes      | Yes       | API key for BeyondTrust Secret Safe authentication |
| `baseUrl` | String  | Yes      | No        | Base URL of the BeyondTrust Secret Safe instance |
| `pwd`     | String  | No       | Yes       | Optional domain password for authentication (if using domain credentials) |

## Data Sources

### `secretsafe_credential_data`

Retrieves username and password from a Secret Safe credential secret.

**Example:**

```hcl
data "secretsafe_credential_data" "db_creds" {
  secret_id = "2e22e1b1-d5c2-4a17-bc90-1234567890ab"
}

output "username" {
  value = data.secretsafe_credential_data.db_creds.username
}

output "password" {
  value     = data.secretsafe_credential_data.db_creds.password
  sensitive = true
}
```

**Arguments:**
- `secret_id` (Required, String) - UUID of the credential secret in BeyondTrust Secret Safe

**Attributes:**
- `secret_id` (String) - The secret ID
- `username` (String) - Username stored in the secret
- `password` (String, Sensitive) - Password stored in the secret

---

### `secretsafe_download_file_data`

Downloads and retrieves file content from a Secret Safe file secret.

**Example:**

```hcl
data "secretsafe_download_file_data" "certificate" {
  secret_id = "87654321-4321-4321-4321-abcdefgh1234"
}

output "file_name" {
  value = data.secretsafe_download_file_data.certificate.file_name
}

output "file_content" {
  value     = base64decode(data.secretsafe_download_file_data.certificate.file_content_base64)
  sensitive = true
}

# Write file to disk
resource "local_file" "certificate" {
  filename            = "/etc/ssl/certs/${data.secretsafe_download_file_data.certificate.file_name}"
  content             = base64decode(data.secretsafe_download_file_data.certificate.file_content_base64)
  file_permission     = "0600"
  sensitive_content   = true
}
```

**Arguments:**
- `secret_id` (Required, String) - UUID of the file secret in BeyondTrust Secret Safe

**Attributes:**
- `secret_id` (String) - The secret ID
- `file_name` (String) - Name of the file stored in the secret
- `file_content_base64` (String, Sensitive) - File content encoded as base64

## Resources

### `secretsafe_folder`

Manages a folder in BeyondTrust Secret Safe.

**Example:**

```hcl
resource "secretsafe_folder" "terraform" {
  name             = "Terraform Managed"
  description      = "Secrets managed by Terraform"
  parent_id        = null
  user_group_id    = 1
}
```

**Arguments:**
- `name` (Required, String) - Name of the folder
- `description` (Optional, String) - Folder description
- `parent_id` (Optional, String) - UUID of the parent folder
- `user_group_id` (Optional, Number) - User group ID for folder access

**Attributes:**
- `id` (String) - UUID of the created folder
- `name` (String) - Folder name
- `description` (String) - Folder description
- `parent_id` (String) - Parent folder UUID
- `user_group_id` (Number) - User group ID

---

### `secretsafe_folder_credential`

Manages a credential secret in a folder.

**Example:**

```hcl
resource "secretsafe_folder_credential" "app_db" {
  folder_id   = secretsafe_folder.terraform.id
  title       = "Application Database"
  description = "Credentials for app database"
  username    = "appuser"
  password    = var.db_password
}

output "db_secret_id" {
  value = secretsafe_folder_credential.app_db.id
}
```

**Arguments:**
- `folder_id` (Required, String) - UUID of the parent folder
- `title` (Required, String) - Secret title
- `description` (Optional, String) - Secret description
- `username` (Required, String) - Username
- `password` (Required, String, Sensitive) - Password

**Attributes:**
- `id` (String) - UUID of the created secret
- `folder_id` (String) - Parent folder UUID
- `title` (String) - Secret title
- `description` (String) - Secret description
- `username` (String) - Username
- `password` (String, Sensitive) - Password
- `owner_id` (Number) - ID of the secret owner (auto-populated from authenticated user)

---

### `secretsafe_folder_file`

Manages a file secret in a folder.

**Example:**

```hcl
resource "secretsafe_folder_file" "tls_cert" {
  folder_id            = secretsafe_folder.terraform.id
  title                = "TLS Certificate"
  description          = "Production TLS certificate"
  file_name            = "cert.pem"
  file_content_base64  = base64encode(file("${path.module}/cert.pem"))
}
```

**Arguments:**
- `folder_id` (Required, String) - UUID of the parent folder
- `title` (Required, String) - Secret title
- `description` (Optional, String) - Secret description
- `file_name` (Required, String) - Name of the file
- `file_content_base64` (Required, String, Sensitive) - File content as base64

**Attributes:**
- `id` (String) - UUID of the created secret
- `folder_id` (String) - Parent folder UUID
- `title` (String) - Secret title
- `description` (String) - Secret description
- `file_name` (String) - File name
- `file_content_base64` (String, Sensitive) - File content as base64
- `owner_id` (Number) - ID of the secret owner (auto-populated from authenticated user)

## Development

### Prerequisites

- **.NET 10 SDK** or later
- **Docker** (for testing and building Native AOT binaries)
- **Git**

### Building

```bash
# Restore dependencies
dotnet restore BeyondTrust.SecretSafeProvider/BeyondTrust.SecretSafeProvider.csproj

# Build (debug)
dotnet build BeyondTrust.SecretSafeProvider/BeyondTrust.SecretSafeProvider.csproj

# Run locally (debug mode with full WebApplication builder)
dotnet run --project BeyondTrust.SecretSafeProvider/BeyondTrust.SecretSafeProvider.csproj
```

### Testing

```bash
# Run all tests (required command for .NET 10)
dotnet run --project BeyondTrust.SecretSafeProvider.Tests/BeyondTrust.SecretSafeProvider.Tests.csproj \
  --disable-logo --fail-fast --no-progress --no-ansi
```

**Note:** Do NOT use `dotnet test` — it is not supported by Microsoft.Testing.Platform on .NET 10 SDK.

### Native AOT Build (Production)

Build a self-contained, trimmed binary for Linux/Alpine:

```bash
# Inside Alpine Docker container with musl toolchain
dotnet publish -c Release -r linux-musl-x64 -o /app/publish \
  -p:PublishAot=true -p:StaticExecutable=true
```

The resulting binary requires no .NET runtime and is suitable for containerized deployment.

### Project Structure

```
.
├── BeyondTrust.SecretSafeProvider/           # Main provider implementation
│   ├── Program.cs                            # gRPC server setup & handshake
│   ├── Services/
│   │   ├── Terraform5ProviderService.cs      # TPPv5.2 RPC implementations
│   │   ├── IBeyondTrustSecretSafe.cs         # Refit API client interface
│   │   ├── DataSources/                      # Data source handlers
│   │   └── Resources/                        # Resource handlers
│   ├── Models/                               # Domain models & configuration
│   └── Protos/                               # gRPC proto definitions
├── BeyondTrust.SecretSafeProvider.AppHost/   # Aspire orchestration (tests)
│   └── __admin/mappings/                     # WireMock API mocks
├── BeyondTrust.SecretSafeProvider.Tests/     # Integration & unit tests
└── CLAUDE.md                                 # Development guidelines
```

### Architecture

**Terraform Plugin Protocol Flow:**

1. Terraform spawns the provider binary
2. Provider writes handshake line to stdout: `1|5|tcp|{host}:{port}|grpc|{cert_base64}`
3. Provider starts gRPC server with mTLS (self-signed certificates)
4. Terraform connects via gRPC and presents its own client certificate
5. Provider implements all TPPv5.2 RPCs: `GetSchema`, `Configure`, `ReadDataSource`, `ApplyResourceChange`, etc.

**Authentication Flow:**

1. User provides `runas`, `key`, and optional `pwd` in provider configuration
2. Each operation calls `SignAppin(KeyAndRunAs)` to authenticate with BeyondTrust API
3. Returns session ID (`SID`) which is used for subsequent API calls
4. Calls `Signout()` to terminate the session

### Testing with WireMock

Tests use **Aspire** to orchestrate:
- **WireMock container** - Mocks the BeyondTrust Secret Safe HTTP API
- **Provider server** - Runs the actual provider for integration testing
- **Test client** - Calls provider gRPC methods and validates responses

WireMock mappings are located in `BeyondTrust.SecretSafeProvider.AppHost/__admin/mappings/` and define mock responses for:
- `/public/v3/Auth/SignAppin` - Authentication endpoint
- `/public/v3/Secrets-Safe/Secrets/{id}` - Get secret
- `/public/v3/Secrets-Safe/Secrets/{id}/file/download` - Download file
- `/public/v3/Secrets-Safe/Folders/*` - Folder operations

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes and ensure all tests pass
4. Commit with clear messages
5. Push to your fork and open a Pull Request

## License

Mozilla Public License 2.0 — see [LICENSE](./LICENSE) for details.

## Support

For issues, questions, or feature requests, please open an [issue on GitHub](https://github.com/AlbertoMonteiro/terraform-provider-beyondtrust-secretsafe/issues).

For BeyondTrust Secret Safe support, visit [BeyondTrust Support](https://www.beyondtrust.com/support).
