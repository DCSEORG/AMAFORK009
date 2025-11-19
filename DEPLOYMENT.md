# Deployment Guide - Expense Management System

## Overview

This guide walks you through deploying the modernized Expense Management System to Azure.

## Prerequisites

✅ **Required:**
- Azure CLI installed and configured
- Active Azure subscription
- Appropriate permissions to create resources

✅ **Optional (for Chat UI):**
- Access to Azure OpenAI service
- Azure AI Search quota

## Quick Deployment (5 minutes)

### Step 1: Login to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### Step 2: Run Deployment Script

```bash
./deploy.sh
```

That's it! The script will:
1. ✅ Create all Azure resources
2. ✅ Configure managed identity
3. ✅ Deploy the application
4. ✅ Provide you with the application URL

### Step 3: Access Your Application

Navigate to: `https://<your-app-url>/Index`

**⚠️ Important**: Don't forget the `/Index` path!

## Configuration Options

### Deploy Without Chat UI (Default - Faster & Cheaper)

```bash
# In deploy.sh, ensure:
INCLUDE_CHAT_UI="false"
```

**Resources deployed:**
- App Service (Basic B1) - ~£13/month
- Managed Identity - Free
- Database connectivity - Free (uses existing SQL)

**Total cost**: ~£13/month

### Deploy With AI Chat Assistant

```bash
# In deploy.sh, set:
INCLUDE_CHAT_UI="true"
```

**Additional resources deployed:**
- Azure OpenAI (S0, pay-per-use) - Variable cost
- Azure AI Search (Basic) - ~£70/month

**Total cost**: ~£83/month + OpenAI usage

## Customization

### Change Azure Region

Edit `deploy.sh`:
```bash
LOCATION="westeurope"  # or your preferred region
```

### Change Resource Names

Edit `deploy.sh`:
```bash
BASE_NAME="myexpenses"  # customize resource prefix
ENVIRONMENT="prod"      # or test, staging, etc.
```

### Use Custom Database

Update `run-sql.py`:
```python
SERVER = "your-sql-server.database.windows.net"
DATABASE = "YourDatabaseName"
```

## Post-Deployment Steps

### 1. Configure Database Access

The deployment script will prompt you to run the SQL permission setup:

```bash
python3 run-sql.py
```

This grants the managed identity access to your database.

### 2. Verify Application

Visit: `https://<your-app-url>/Index`

You should see:
- ✅ Modern purple gradient interface
- ✅ Three tabs: View Expenses, Add Expense, Approve Expenses
- ✅ Sample expenses loaded (from dummy data if DB not connected)

### 3. Test API Endpoints

Visit: `https://<your-app-url>/swagger`

You can test all API endpoints directly from the Swagger UI.

### 4. Test Chat UI (if enabled)

Visit: `https://<your-app-url>/chatui/chat.html`

Try asking:
- "Show me my expenses"
- "What's the expense policy?"
- "Create a new travel expense"

## Troubleshooting

### Issue: Application shows dummy data

**Cause**: Database connection not established or managed identity lacks permissions

**Solution**:
```bash
# Run the SQL permission script
python3 run-sql.py

# Restart the App Service
az webapp restart --name <your-app-name> --resource-group <your-rg-name>
```

### Issue: Cannot access application

**Cause**: Incorrect URL or app not started

**Solution**:
1. Verify you're using `/Index` in the URL
2. Wait 2-3 minutes for app to start
3. Check App Service logs:
   ```bash
   az webapp log tail --name <your-app-name> --resource-group <your-rg-name>
   ```

### Issue: Chat UI not responding

**Cause**: GenAI resources not deployed

**Solution**:
1. Set `INCLUDE_CHAT_UI="true"` in deploy.sh
2. Re-run deployment: `./deploy.sh`

### Issue: Deployment fails with "Resource not found"

**Cause**: Azure subscription lacks required resource providers

**Solution**:
```bash
# Register required providers
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.ManagedIdentity
az provider register --namespace Microsoft.CognitiveServices
az provider register --namespace Microsoft.Search
```

## Architecture Summary

```
User → App Service → Managed Identity → Azure SQL Database
                  ↓
            (if enabled)
                  ↓
         Azure OpenAI + AI Search
```

**Key Features:**
- 🔐 Passwordless authentication with Managed Identity
- 🚀 One-command deployment
- 💰 Cost-optimized with low-tier SKUs
- 🌍 Modern, responsive UI
- 📚 Full API documentation
- 🤖 Optional AI chat assistant

## Next Steps

### For Development
1. Clone the repository locally
2. Run `dotnet restore` in the `app` folder
3. Update `appsettings.Development.json` with your database details
4. Run `dotnet run` to test locally

### For Production

**⚠️ Important**: This is a **development/POC setup**. For production:

1. **Upgrade App Service Plan** to Standard or Premium
2. **Enable Private Endpoints** for database and other services
3. **Configure Custom Domains** and SSL certificates
4. **Set up Application Insights** for monitoring
5. **Enable Azure AD Authentication** for user login
6. **Configure Backup and Disaster Recovery**
7. **Implement CI/CD Pipeline** with GitHub Actions or Azure DevOps
8. **Add Application Gateway** or Azure Front Door
9. **Enable Network Security Groups** and firewall rules
10. **Review and implement GDPR/compliance** requirements

## Cost Estimation

### Without Chat UI (Default)
- App Service (Basic B1): ~£13/month
- Existing database: £0 (assuming already provisioned)
- **Total**: ~£13/month

### With Chat UI
- App Service (Basic B1): ~£13/month
- Azure OpenAI (S0): Pay-per-use (~£5-20/month for moderate use)
- Azure AI Search (Basic): ~£70/month
- **Total**: ~£88-103/month

### Cost Optimization Tips
1. Use **Free tier** App Service for learning/testing
2. **Stop** App Service when not in use
3. Use **consumption-based** OpenAI pricing
4. Consider **serverless SQL** for database if applicable
5. Delete resources when not needed

## Support & Resources

- **Architecture Diagram**: See `ARCHITECTURE.md`
- **Database Schema**: See `Database-Schema/database_schema.sql`
- **API Documentation**: Available at `/swagger` endpoint
- **Azure Documentation**: https://learn.microsoft.com/azure/

## Security Summary

✅ **Security Features Implemented:**
- Managed Identity for passwordless authentication
- HTTPS enforcement on App Service
- Azure AD authentication for database
- Secure secrets management (no hardcoded credentials)
- Network isolation ready (can add private endpoints)

✅ **Dependencies Scanned:**
- No known vulnerabilities in NuGet packages
- No known vulnerabilities in Python packages
- All packages use recent stable versions

⚠️ **Production Considerations:**
- Enable Azure AD authentication for user login
- Implement proper RBAC controls
- Add Azure Key Vault for additional secrets
- Enable Azure Security Center
- Configure Azure Policy for compliance
- Set up Azure DDoS Protection

## License & Contributing

This is a demonstration project for Azure app modernization. Feel free to:
- Fork and modify for your needs
- Use as a template for similar projects
- Contribute improvements back to the repository

## Feedback

For issues or questions, please refer to the main README.md or create an issue in the repository.

---

**Happy Deploying! 🚀**
