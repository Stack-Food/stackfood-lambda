aws_region  = "us-east-1"
environment = "prod"

tags = {
  Project    = "StackFood"
  Team       = "DevOps"
  CostCenter = "IT"
}

domain_name               = "stackfood.com.br"
subject_alternative_names = ["*.stackfood.com.br"]
lambda_role_name      = "LabRole"

######################
# Lambda Configuration #
######################
lambda_functions = {
  "stackfood-auth" = {
    description  = "Lambda for CPF authentication and JWT validation"
    package_type = "Image"
    # Imagem base oficial AWS Lambda para .NET 8 - RUNTIME
    image_uri   = "public.ecr.aws/lambda/dotnet:9.2025.09.15.12"
    memory_size = 256
    runtime     = null # Não usado para package_type = "Image"
    timeout     = 30
    vpc_access  = false
    handler     = null # Não usado para package_type = "Image"
    filename    = null
    environment_variables = {
      USER_POOL_ID           = ""
      CLIENT_ID              = ""
      LOG_LEVEL              = "info"
      ASPNETCORE_ENVIRONMENT = "Production"
    }
  }
}

##########################
# Cognito Configuration #
##########################
cognito_user_pools = {
  "stackfood-users" = {
    name                                          = "stackfood-prod-users"
    alias_attributes                              = ["preferred_username"] # CPF será usado via preferred_username
    auto_verified_attributes                      = []                     # Sem verificação automática (apenas CPF)
    attributes_require_verification_before_update = []                     # Nenhum atributo requer verificação antes de atualizar
    # Para autenticação por CPF customizada, usar alias_attributes em vez de username_attributes

    # Password Policy - Desabilitada para autenticação sem senha
    password_minimum_length          = 8 # Mantido para compatibilidade, mas não será usado
    password_require_lowercase       = false
    password_require_numbers         = false
    password_require_symbols         = false
    password_require_uppercase       = false
    temporary_password_validity_days = 1

    # Security Settings - Configurado para autenticação customizada
    advanced_security_mode       = "AUDIT" # Mudado para AUDIT para permitir auth customizada
    allow_admin_create_user_only = true    # Apenas admin pode criar (via Lambda)

    # Email Configuration - Opcional para este fluxo
    email_configuration = {
      email_sending_account = "COGNITO_DEFAULT"
    }

    # Lambda Triggers para autenticação customizada com CPF
    lambda_config = {
      create_auth_challenge          = null # Lambda para criar desafio personalizado (CPF)
      define_auth_challenge          = null # Lambda para definir fluxo de autenticação
      verify_auth_challenge_response = null # Lambda para verificar CPF
      pre_sign_up                    = null # Lambda para pré-processamento de registro
      post_confirmation              = null # Lambda para pós-confirmação
      post_authentication            = null # Lambda para pós-autenticação
    }

    # Domain for hosted UI (opcional para POC)
    domain = "stackfood-prod"

    # Client Applications - Configurado para autenticação sem senha
    clients = {
      "cpf-auth-app" = {
        name                         = "stackfood-cpf-auth"
        generate_secret              = false # Frontend não precisa de secret
        refresh_token_validity       = 30
        access_token_validity        = 60
        id_token_validity            = 60
        access_token_validity_units  = "minutes"
        id_token_validity_units      = "minutes"
        refresh_token_validity_units = "days"

        # OAuth flows para SPA com autenticação customizada
        allowed_oauth_flows                  = ["implicit"]
        allowed_oauth_flows_user_pool_client = true
        allowed_oauth_scopes                 = ["openid", "profile", "aws.cognito.signin.user.admin"]
        callback_urls                        = ["http://localhost:3000/callback", "https://stackfood-prod.com/callback"]
        logout_urls                          = ["http://localhost:3000/logout", "https://stackfood-prod.com/logout"]

        # Autenticação customizada para CPF sem senha
        explicit_auth_flows           = ["ALLOW_CUSTOM_AUTH", "ALLOW_REFRESH_TOKEN_AUTH"]
        supported_identity_providers  = ["COGNITO"]
        prevent_user_existence_errors = "ENABLED"
        enable_token_revocation       = true

        read_attributes  = []
        write_attributes = []
      }

      "api-backend" = {
        name                         = "stackfood-api-backend"
        generate_secret              = true # Backend precisa de secret
        refresh_token_validity       = 30
        access_token_validity        = 120 # Maior validade para API
        id_token_validity            = 60
        access_token_validity_units  = "minutes"
        id_token_validity_units      = "minutes"
        refresh_token_validity_units = "days"

        # Client credentials para serviços backend
        allowed_oauth_flows                  = ["client_credentials"]
        allowed_oauth_flows_user_pool_client = true
        allowed_oauth_scopes                 = ["aws.cognito.signin.user.admin"]

        # Permite autenticação administrativa para criação de usuários
        explicit_auth_flows           = ["ALLOW_ADMIN_USER_PASSWORD_AUTH", "ALLOW_CUSTOM_AUTH", "ALLOW_REFRESH_TOKEN_AUTH"]
        supported_identity_providers  = ["COGNITO"]
        prevent_user_existence_errors = "ENABLED"
        enable_token_revocation       = true

        read_attributes  = []
        write_attributes = []
      }
    }

    # Identity Pool para acesso AWS (opcional)
    create_identity_pool             = true
    allow_unauthenticated_identities = false
    default_client_key               = "cpf-auth-app"

    # Custom Attributes para StackFood - Foco em CPF
    schemas = [
      {
        attribute_data_type      = "String"
        name                     = "custom:cpf"
        required                 = false # Custom attributes não podem ser required
        mutable                  = false # CPF não pode ser alterado
        developer_only_attribute = false
        string_attribute_constraints = {
          min_length = "11"
          max_length = "14"
        }
      }
    ]
  }
}
