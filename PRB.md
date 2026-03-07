Construção de um Terraform Provider utilizando **.NET Native AOT**, **Slim Builder** e o **Protocolo V5**.

---

## 🏗️ 1. Arquitetura do Projeto

O objetivo é criar um executável nativo único que implemente a interface gRPC do Terraform.

### Configuração do `.csproj`

Focada em **linkagem estática** e compatibilidade com **musl** (Alpine).

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <PublishAot>true</PublishAot>
  <StaticExecutable>true</StaticExecutable>
  <Nullable>enable</Nullable>
  <InvariantGlobalization>true</InvariantGlobalization>
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>full</TrimMode>
  <StackTraceSupport>false</StackTraceSupport>
  <OptimizationPreference>Size</OptimizationPreference>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Grpc.AspNetCore" Version="2.*" />
</ItemGroup>

```

### Inicialização com Slim Builder (`Program.cs`)

O Terraform V5 espera que o provider inicie um servidor gRPC e imprima uma string de handshake no `STDOUT`.

```csharp
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateSlimBuilder(args);

// Configuração manual do Kestrel para gRPC local (h2c)
builder.WebHost.ConfigureKestrel(options => {
    options.ListenLocalhost(50051, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();
var app = builder.Build();

app.MapGrpcService<Terraform5ProviderService>();

// Handshake Protocol V5: versão_proto|versão_core|rede|endereço|tipo
Console.WriteLine("1|5|tcp|127.0.0.1:50051|grpc");

app.Run();

```

---

## 🛠️ 2. Ambiente de Build (Docker)

Para que o provider rode na imagem oficial do Terraform (Alpine), o build **precisa** ser feito no Alpine para vincular à biblioteca `musl`.

**Dockerfile de Referência:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
# Instalando toolchain nativa necessária para AOT
RUN apk add --no-cache clang gcc build-base zlib-dev musl-dev

WORKDIR /src
COPY . .

# Publish gerando binário estático para o Terraform
RUN dotnet publish -c Release -r linux-musl-x64 -o /app/publish \
    -p:PublishAot=true -p:StaticExecutable=true

```

---

## 📦 3. Empacotamento para o Registry

O Terraform Registry exige uma estrutura de arquivos e nomenclatura rígida para o `terraform init` funcionar.

### Convenção de Nomes

* **Binário:** `terraform-provider-{nome}_{versao}_{os}_{arch}`
* **Zip:** `terraform-provider-{nome}_{versao}_{os}_{arch}.zip`

### Script de Automação (Bash)

Este script gera os hashes necessários para a release:

```bash
#!/bin/bash
NAME="meu-provedor"
VERSION="1.0.0"

# 1. Zipar os binários
zip "terraform-provider-${NAME}_${VERSION}_linux_amd64.zip" terraform-provider-${NAME}

# 2. Gerar SHA256SUMS
sha256sum *.zip > "terraform-provider-${NAME}_${VERSION}_SHA256SUMS"

# 3. Assinar com GPG (Exige que você tenha uma chave configurada)
gpg --detach-sign "terraform-provider-${NAME}_${VERSION}_SHA256SUMS"

```

---

## ✅ 4. Checklist de Recuperação

Ao retomar este projeto, verifique:

1. **Handshake:** A string `1|5|tcp|...` está sendo a primeira coisa impressa no console?
2. **Arquivos Proto:** Você importou os `.proto` da V5 (tf680) e os compilou com o `Grpc.Tools`?
3. **Linkagem:** Rodou `ldd` no binário final para garantir que ele é `statically linked`?
4. **GPG:** Sua chave pública está cadastrada no Terraform Registry?

---

**Status do Handoff:** Pronto para arquivamento.
Deseja que eu escreva um exemplo de implementação para o método `GetSchema` do Protocolo V5 antes de encerrarmos?