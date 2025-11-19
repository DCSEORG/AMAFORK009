# Azure Services Architecture Diagram

## Expense Management System - Azure Infrastructure

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         AZURE SUBSCRIPTION                              │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────┐    │
│  │         Resource Group: rg-expensemgmt-dev                    │    │
│  │                                                               │    │
│  │  ┌──────────────────────────────────────────────────────┐    │    │
│  │  │  User-Assigned Managed Identity                      │    │    │
│  │  │  mid-AppModAssist-[timestamp]                        │    │    │
│  │  │                                                       │    │    │
│  │  │  • Authenticates to Azure SQL                        │    │    │
│  │  │  • Accesses Azure OpenAI (if enabled)                │    │    │
│  │  └───────────────┬──────────────────────────────────────┘    │    │
│  │                  │                                            │    │
│  │                  │ Identity                                   │    │
│  │                  ▼                                            │    │
│  │  ┌──────────────────────────────────────────────────────┐    │    │
│  │  │  App Service Plan (Basic B1)                         │    │    │
│  │  │  asp-expensemgmt-dev                                 │    │    │
│  │  │                                                       │    │    │
│  │  │  Location: UK South                                  │    │    │
│  │  │  OS: Linux                                           │    │    │
│  │  │  Runtime: .NET 8.0                                   │    │    │
│  │  └───────────────┬──────────────────────────────────────┘    │    │
│  │                  │                                            │    │
│  │                  │ Hosts                                      │    │
│  │                  ▼                                            │    │
│  │  ┌──────────────────────────────────────────────────────┐    │    │
│  │  │  App Service                                         │    │    │
│  │  │  app-expensemgmt-dev-[timestamp]                     │    │    │
│  │  │                                                       │    │    │
│  │  │  • ASP.NET Core Razor Pages                          │    │    │
│  │  │  • REST APIs                                         │    │    │
│  │  │  • Swagger Documentation                             │    │    │
│  │  │  • Chat UI (if enabled)                              │    │    │
│  │  └───────────────┬──────────────────────────────────────┘    │    │
│  │                  │                                            │    │
│  │                  │ Connects via                               │    │
│  │                  │ Managed Identity                           │    │
│  │                  ▼                                            │    │
│  │  ┌──────────────────────────────────────────────────────┐    │    │
│  │  │  Azure SQL Database (External)                       │    │    │
│  │  │  Server: sql-expense-mgmt-xyz                        │    │    │
│  │  │  Database: ExpenseManagementDB                       │    │    │
│  │  │                                                       │    │    │
│  │  │  • Expenses table                                    │    │    │
│  │  │  • Users table                                       │    │    │
│  │  │  • Categories and Status tables                      │    │    │
│  │  │  • Managed Identity authentication                   │    │    │
│  │  └──────────────────────────────────────────────────────┘    │    │
│  │                                                               │    │
│  │  ┌─ OPTIONAL: When includeChatUI = true ─────────────┐      │    │
│  │  │                                                     │      │    │
│  │  │  ┌───────────────────────────────────────────┐     │      │    │
│  │  │  │  Azure OpenAI (S0 SKU)                   │     │      │    │
│  │  │  │  aoai-expensemgmt-dev                    │     │      │    │
│  │  │  │                                           │     │      │    │
│  │  │  │  Location: Sweden Central                │     │      │    │
│  │  │  │  Model: GPT-4o                           │     │      │    │
│  │  │  │  • Natural language understanding        │     │      │    │
│  │  │  │  • Function calling for APIs             │     │      │    │
│  │  │  └───────────────────────────────────────────┘     │      │    │
│  │  │                                                     │      │    │
│  │  │  ┌───────────────────────────────────────────┐     │      │    │
│  │  │  │  Azure AI Search (Basic)                 │     │      │    │
│  │  │  │  srch-expensemgmt-dev                    │     │      │    │
│  │  │  │                                           │     │      │    │
│  │  │  │  Location: UK South                      │     │      │    │
│  │  │  │  • RAG pattern implementation            │     │      │    │
│  │  │  │  • Contextual document search            │     │      │    │
│  │  │  └───────────────────────────────────────────┘     │      │    │
│  │  │                                                     │      │    │
│  │  └─────────────────────────────────────────────────────┘      │    │
│  │                                                               │    │
│  └───────────────────────────────────────────────────────────────┘    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

## Component Details

### Core Components (Always Deployed)

1. **User-Assigned Managed Identity**
   - Purpose: Secure authentication to Azure services
   - Used by: App Service
   - Grants access to: Azure SQL Database, Azure OpenAI (if enabled)

2. **App Service Plan (Basic B1)**
   - Tier: Basic
   - Cost: ~£13/month (UK South)
   - Suitable for: Development and testing

3. **App Service**
   - Runtime: .NET 8.0 on Linux
   - Features: Razor Pages UI, REST APIs, Swagger docs
   - HTTPS: Enforced
   - Configuration: Managed Identity client ID

4. **Azure SQL Database**
   - Connection: Via Managed Identity
   - Authentication: Azure AD
   - Fallback: Dummy data if connection fails

### Optional Components (When includeChatUI = true)

5. **Azure OpenAI**
   - Location: Sweden Central (GPT-4o availability)
   - SKU: S0 (Standard)
   - Model: GPT-4o
   - Cost: Pay per token
   - Purpose: AI chat assistant

6. **Azure AI Search**
   - Tier: Basic
   - Cost: ~£70/month
   - Purpose: RAG pattern for contextual chat responses

## Data Flow

1. **User Request** → App Service receives HTTP request
2. **Authentication** → App Service uses Managed Identity
3. **Database Query** → Connects to Azure SQL with MI credentials
4. **Response** → Data returned to user via Razor Pages or API
5. **Chat Query** (if enabled) → OpenAI processes with RAG context
6. **Function Calling** → OpenAI calls APIs to perform actions

## Security Features

- Managed Identity for passwordless authentication
- HTTPS enforced on App Service
- Azure SQL with firewall rules
- Private endpoints possible (not configured in basic setup)
- Role-based access control (RBAC)

## Cost Optimization

- Basic tier for App Service Plan (lowest cost with custom domains)
- S0 tier for Azure OpenAI (pay-as-you-go)
- Basic tier for AI Search (lowest search tier)
- Linux-based App Service (cheaper than Windows)
- Optional GenAI resources (can be disabled)

## Deployment

All resources are deployed using:
- **Infrastructure as Code**: Bicep templates
- **Orchestration**: deploy.sh bash script
- **One-command deployment**: `./deploy.sh`
