# Expense Management Chat UI

This folder contains the AI-powered chat interface for the Expense Management System.

## Features

- **Natural Language Interaction**: Ask questions about your expenses in plain English
- **Expense Management**: Create, view, and manage expenses through conversation
- **Policy Assistance**: Get answers to expense policy questions
- **RAG Integration**: Uses Retrieval-Augmented Generation for context-aware responses

## How to Use

1. Navigate to `/chatui/chat.html` in your browser
2. Type your questions or requests in the chat input
3. The AI assistant will help you with expense-related tasks

## Example Queries

- "Show me my pending expenses"
- "Create a new expense for travel"
- "What's the policy for meal expenses?"
- "Check the status of my last expense"

## Technical Details

The chat UI integrates with:
- **Azure OpenAI**: GPT-4o model for natural language understanding
- **Azure AI Search**: For RAG-based context retrieval
- **Expense Management APIs**: For performing actual operations

## Configuration

Settings are stored in `GenAISettings.json`:
- `EnableChatUI`: Toggle Chat UI functionality
- `AzureOpenAI.Endpoint`: Your Azure OpenAI endpoint
- `AzureSearch.Endpoint`: Your Azure AI Search endpoint

## Note

In the current implementation, the chat UI uses rule-based responses for demonstration. 
To enable full Azure OpenAI integration, ensure the GenAI resources are deployed 
by setting `INCLUDE_CHAT_UI="true"` in the `deploy.sh` script.
