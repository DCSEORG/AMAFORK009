![Header image](https://github.com/DougChisholm/App-Mod-Assist/blob/main/repo-header.png)

# Modern Expense Management System

A modernized cloud-native expense management application built with ASP.NET Core, deployed to Azure with AI-powered chat assistance.

## 🌟 Features

- **Modern Web UI**: Clean, responsive interface for managing expenses
- **RESTful APIs**: Full CRUD operations with Swagger documentation
- **Azure Integration**: Deployed to Azure App Service with Managed Identity
- **Secure Database**: Connects to Azure SQL using passwordless authentication
- **AI Chat Assistant**: Optional GenAI-powered chat for natural language interaction
- **Manager Approvals**: Workflow for expense submission and approval

## 🚀 Quick Start

### Prerequisites

- Azure CLI installed and configured
- Azure subscription
- Access to an Azure SQL Database (or use the provided sample database)

### Deployment

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd AMAFORK009
   ```

2. **Login to Azure**
   ```bash
   az login
   az account set --subscription <your-subscription-id>
   ```

3. **Configure deployment** (optional)
   
   Edit `deploy.sh` to customize:
   - `LOCATION`: Azure region (default: uksouth)
   - `BASE_NAME`: Resource naming prefix (default: expensemgmt)
   - `ENVIRONMENT`: Environment suffix (default: dev)
   - `INCLUDE_CHAT_UI`: Enable AI chat features (default: false)

4. **Deploy everything**
   ```bash
   ./deploy.sh
   ```

   This single script will:
   - Deploy all Azure infrastructure (App Service, Managed Identity, GenAI resources)
   - Configure database permissions
   - Deploy the application code
   - Provide you with the application URL

5. **Access your application**
   
   Navigate to: `https://<your-app-url>/Index`
   
   ⚠️ **Important**: Don't forget the `/Index` path!

## 📋 Deployment Options

### Without Chat UI (Default)
```bash
# In deploy.sh, ensure:
INCLUDE_CHAT_UI="false"

# Then run:
./deploy.sh
```

This deploys only the core application with APIs and web UI.

### With AI Chat UI
```bash
# In deploy.sh, set:
INCLUDE_CHAT_UI="true"

# Then run:
./deploy.sh
```

This additionally deploys:
- Azure OpenAI (GPT-4o model in Sweden)
- Azure AI Search (for RAG pattern)
- Configured chat interface

## 🏗️ Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for the complete Azure services diagram and detailed component descriptions.

**Core Components:**
- App Service (Basic B1 tier, UK South)
- User-Assigned Managed Identity
- Azure SQL Database (external, managed identity auth)

**Optional Components:**
- Azure OpenAI (S0, Sweden Central)
- Azure AI Search (Basic, UK South)

## 📚 Documentation

- **API Documentation**: Available at `https://<your-app-url>/swagger`
- **Chat UI**: Available at `https://<your-app-url>/chatui/chat.html`
- **Architecture Diagram**: See [ARCHITECTURE.md](ARCHITECTURE.md)
- **Database Schema**: See [Database-Schema/database_schema.sql](Database-Schema/database_schema.sql)

## 🔧 Manual Setup

If you prefer to deploy components individually:

### 1. Deploy Infrastructure
```bash
az deployment sub create \
  --name expensemgmt-deploy \
  --location uksouth \
  --template-file ./Infrastructure/main.bicep \
  --parameters location=uksouth includeChatUI=false
```

### 2. Configure Database Permissions
```bash
pip3 install pyodbc azure-identity
python3 run-sql.py
```

### 3. Deploy Application
```bash
az webapp deploy \
  --resource-group rg-expensemgmt-dev \
  --name <your-app-name> \
  --src-path ./app.zip
```

## 💡 Usage

### Web Interface

1. **View Expenses**: See all expenses with filtering
2. **Add Expense**: Create new expense entries with amount, date, category, and description
3. **Approve Expenses**: Managers can review and approve pending expenses

### API Endpoints

- `GET /api/expenses` - List all expenses
- `POST /api/expenses` - Create new expense
- `PUT /api/expenses/{id}` - Update expense
- `POST /api/expenses/{id}/submit` - Submit for approval
- `POST /api/expenses/{id}/approve` - Approve expense
- `GET /api/categories` - Get expense categories
- `GET /api/statuses` - Get expense statuses

### Chat Assistant

Navigate to `/chatui/chat.html` and ask questions like:
- "Show me my pending expenses"
- "Create a new travel expense for £50"
- "What's the expense policy for meals?"

## 🔐 Security

- **Managed Identity**: Passwordless authentication to Azure SQL
- **HTTPS Only**: All traffic encrypted
- **Azure AD Authentication**: Secure database access
- **Error Handling**: Fallback to dummy data if database unavailable

## 🧪 Testing

The application includes dummy data fallback, so you can test the UI even without database connectivity.

To test with real data:
1. Ensure the Azure SQL Database exists
2. Run `python3 run-sql.py` to grant managed identity permissions
3. Restart the App Service

## 📝 Development

### Building Locally

```bash
cd app
dotnet restore
dotnet build
dotnet run
```

Access at: `https://localhost:5001/Index`

### Publishing

```bash
cd app
dotnet publish -c Release -o ./publish
cd publish && zip -r ../../app.zip . && cd ../..
```

## 🐛 Troubleshooting

**Issue**: Application shows dummy data
- **Solution**: Check database connection and managed identity permissions

**Issue**: Chat UI not working
- **Solution**: Ensure `INCLUDE_CHAT_UI="true"` in deploy.sh and redeploy

**Issue**: Cannot access application
- **Solution**: Remember to navigate to `/Index`, not just the root URL

**Issue**: Database permission errors
- **Solution**: Run `python3 run-sql.py` to grant managed identity access

## 📄 License

See [LICENSE](LICENSE) file for details.

## 🤝 Contributing

This is a demonstration project showing Azure app modernization patterns. Feel free to fork and adapt for your needs.

---

**Note**: This application follows Azure best practices for:
- Infrastructure as Code (Bicep)
- Managed Identity for authentication
- Low-cost development SKUs
- Secure, cloud-native architecture
