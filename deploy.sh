#!/bin/bash

# Deployment script for Expense Management System
# This script deploys all Azure infrastructure and application code

set -e  # Exit on error

echo "========================================="
echo " Expense Management System Deployment"
echo "========================================="
echo ""

# Configuration
LOCATION="uksouth"
BASE_NAME="expensemgmt"
ENVIRONMENT="dev"
RG_NAME="rg-${BASE_NAME}-${ENVIRONMENT}"
INCLUDE_CHAT_UI="false"  # Set to "true" to deploy GenAI resources for Chat UI

echo "Configuration:"
echo "  Location: $LOCATION"
echo "  Base Name: $BASE_NAME"
echo "  Environment: $ENVIRONMENT"
echo "  Resource Group: $RG_NAME"
echo "  Include Chat UI: $INCLUDE_CHAT_UI"
echo ""

# Check Azure CLI is installed and logged in
echo "Checking Azure CLI..."
if ! command -v az &> /dev/null; then
    echo "Error: Azure CLI is not installed. Please install it first."
    exit 1
fi

if ! az account show &> /dev/null; then
    echo "Error: Not logged in to Azure. Please run 'az login' first."
    exit 1
fi

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
echo "Using subscription: $SUBSCRIPTION_ID"
echo ""

# Deploy infrastructure
echo "========================================="
echo " Deploying Infrastructure"
echo "========================================="
echo ""

DEPLOYMENT_NAME="expensemgmt-$(date +%Y%m%d%H%M%S)"

echo "Creating deployment: $DEPLOYMENT_NAME"
DEPLOYMENT_OUTPUT=$(az deployment sub create \
  --name "$DEPLOYMENT_NAME" \
  --location "$LOCATION" \
  --template-file "./Infrastructure/main.bicep" \
  --parameters \
    location="$LOCATION" \
    baseName="$BASE_NAME" \
    environment="$ENVIRONMENT" \
    resourceGroupName="$RG_NAME" \
    includeChatUI="$INCLUDE_CHAT_UI" \
  --output json)

echo "✓ Infrastructure deployed successfully"
echo ""

# Extract outputs
echo "Extracting deployment outputs..."
APP_SERVICE_NAME=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.properties.outputs.appServiceName.value')
APP_SERVICE_URL=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.properties.outputs.appServiceUrl.value')
MANAGED_IDENTITY_NAME=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.properties.outputs.managedIdentityName.value')
MANAGED_IDENTITY_CLIENT_ID=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.properties.outputs.managedIdentityClientId.value')

echo "  App Service Name: $APP_SERVICE_NAME"
echo "  App Service URL: $APP_SERVICE_URL"
echo "  Managed Identity: $MANAGED_IDENTITY_NAME"
echo "  MI Client ID: $MANAGED_IDENTITY_CLIENT_ID"
echo ""

# Setup SQL Database Permissions
echo "========================================="
echo " Setting Up Database Permissions"
echo "========================================="
echo ""

echo "Installing required Python packages..."
pip3 install --quiet pyodbc azure-identity

echo "Running SQL permission setup script..."
echo "NOTE: This script grants the managed identity access to the database."
echo "If the database doesn't exist or you don't have access, this step may fail."
echo "You can skip this and manually grant permissions later."
echo ""

read -p "Do you want to run the SQL permission setup? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    # Update script.sql with the actual managed identity name
    sed -i "s/mid-AppModAssist/${MANAGED_IDENTITY_NAME}/g" script.sql
    
    if python3 run-sql.py; then
        echo "✓ Database permissions configured successfully"
    else
        echo "⚠ Warning: Database permission setup failed. You may need to configure manually."
        echo "  Run: python3 run-sql.py"
    fi
else
    echo "⚠ Skipping database permission setup"
    echo "  Remember to run: python3 run-sql.py"
fi
echo ""

# Deploy Application
echo "========================================="
echo " Deploying Application Code"
echo "========================================="
echo ""

if [ ! -f "app.zip" ]; then
    echo "Error: app.zip not found. Please build the application first."
    exit 1
fi

echo "Deploying application to $APP_SERVICE_NAME..."
az webapp deploy \
  --resource-group "$RG_NAME" \
  --name "$APP_SERVICE_NAME" \
  --src-path ./app.zip \
  --type zip

echo "✓ Application deployed successfully"
echo ""

# Final Instructions
echo "========================================="
echo " Deployment Complete! "
echo "========================================="
echo ""
echo "Your Expense Management System is now deployed!"
echo ""
echo "📱 Application URL: ${APP_SERVICE_URL}/Index"
echo "📚 API Documentation: ${APP_SERVICE_URL}/swagger"
echo ""
echo "IMPORTANT: Navigate to ${APP_SERVICE_URL}/Index to view the application"
echo "(Don't just go to the root URL)"
echo ""

if [ "$INCLUDE_CHAT_UI" = "true" ]; then
    echo "✨ Chat UI is ENABLED"
    echo "   GenAI resources have been deployed"
else
    echo "💡 Chat UI is DISABLED"
    echo "   To enable it, set INCLUDE_CHAT_UI='true' at the top of this script and re-run"
fi
echo ""
echo "Next steps:"
echo "1. Wait 2-3 minutes for the app to start up"
echo "2. Navigate to ${APP_SERVICE_URL}/Index"
echo "3. Test the application with the dummy data"
echo "4. If database connection fails, verify:"
echo "   - The SQL server and database exist"
echo "   - The managed identity has been granted access"
echo "   - Run: python3 run-sql.py to grant permissions"
echo ""
echo "========================================="
