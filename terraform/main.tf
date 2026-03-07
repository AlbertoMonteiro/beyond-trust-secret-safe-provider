terraform {
  required_providers {
    beyondtrust = {
      source = "beyondtrust/secretsafe"
    }
  }
}

provider "beyondtrust" {}

data "beyondtrust_hello" "example" {}

output "content" {
  value = data.beyondtrust_hello.example.content
}
