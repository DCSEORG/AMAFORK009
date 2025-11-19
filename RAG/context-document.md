# Expense Management System - Context Document

## System Overview

The Expense Management System is a modern web application for tracking and managing business expenses. 
It supports employee expense submission and manager approval workflows.

## Key Features

1. **Expense Submission**: Employees can create and submit expense claims
2. **Approval Workflow**: Managers can review and approve/reject expenses
3. **Categories**: Travel, Meals, Supplies, Accommodation, Other
4. **Status Tracking**: Draft, Submitted, Approved, Rejected

## Database Schema

### Tables
- **Users**: Employee and manager information
- **Expenses**: Expense records with amounts, dates, and descriptions
- **ExpenseCategories**: Expense classification categories
- **ExpenseStatus**: Status lookup (Draft, Submitted, Approved, Rejected)
- **Roles**: Employee and Manager roles

### Key Fields
- **AmountMinor**: Amounts stored in pence (£12.34 = 1234)
- **Currency**: GBP (British Pounds)
- **ExpenseDate**: Date the expense was incurred
- **SubmittedAt**: When the expense was submitted for approval
- **ReviewedBy**: Manager who approved/rejected the expense

## Expense Policies

### Submission Requirements
1. All expenses must have a description
2. Expenses should include receipts when available
3. Submit expenses within 30 days of incurrence
4. Use appropriate categories for each expense

### Approval Guidelines
1. Managers review expenses from their direct reports
2. Check for appropriate categorization
3. Verify expense amounts match receipts
4. Ensure compliance with company policies

### Category Guidelines

**Travel**
- Taxi/rideshare services
- Train/bus tickets
- Parking fees
- Mileage reimbursement

**Meals**
- Client entertainment
- Business meals
- Team lunches (with approval)

**Supplies**
- Office supplies
- Equipment under £500
- Software subscriptions

**Accommodation**
- Hotel stays for business travel
- Conference accommodation

**Other**
- Miscellaneous business expenses
- Must include detailed description

## API Endpoints

- `GET /api/expenses` - List all expenses
- `GET /api/expenses/{id}` - Get specific expense
- `POST /api/expenses` - Create new expense
- `PUT /api/expenses/{id}` - Update expense
- `POST /api/expenses/{id}/submit` - Submit for approval
- `POST /api/expenses/{id}/approve` - Approve expense
- `GET /api/categories` - Get expense categories
- `GET /api/statuses` - Get expense statuses

## Common Questions

**Q: How do I submit an expense?**
A: Go to the "Add Expense" tab, fill in the amount, date, category, and description, then click Submit.

**Q: What statuses can an expense have?**
A: Draft, Submitted, Approved, or Rejected.

**Q: Who can approve my expenses?**
A: Your direct manager can approve expenses you submit.

**Q: What information is required for an expense?**
A: Amount, date, category, and optionally a description and receipt.

**Q: How do I filter expenses?**
A: Use the filter box in the "View Expenses" tab to search by description, category, or user.
