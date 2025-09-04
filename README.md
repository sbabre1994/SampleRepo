# Upgrade Notification System

A comprehensive ASP.NET Core web application with React frontend for managing system upgrade notifications.

## Features

### Backend Features
- **ASP.NET Core Web API** with Entity Framework Core
- **SignalR** for real-time notifications
- **Hangfire** for background job scheduling
- **Email notifications** using MailKit
- **Admin API** for creating planned upgrades
- **User preference management** via REST API

### Frontend Features
- **React with TypeScript** for type safety
- **Real-time notifications** via SignalR integration
- **Responsive design** with custom CSS
- **DateTime picker** for upgrade preferences
- **Toast notifications** for user feedback

## Project Structure

```
/
├── Controllers/           # API Controllers
│   ├── AdminController.cs      # Admin functionality
│   └── UpgradeController.cs    # User upgrade preferences
├── Data/                 # Entity Framework
│   └── UpgradeNotificationContext.cs
├── Hubs/                 # SignalR Hubs
│   └── UpgradeNotificationHub.cs
├── Models/               # Data models and DTOs
│   ├── PlannedUpgrade.cs
│   ├── UserUpgradePreference.cs
│   └── Dtos.cs
├── Services/             # Business logic
│   ├── EmailService.cs
│   ├── UpgradeJobService.cs
│   └── UpgradeNotificationService.cs
└── frontend/             # React frontend
    ├── src/
    │   ├── components/         # React components
    │   ├── services/          # API and SignalR services
    │   ├── hooks/             # Custom React hooks
    │   ├── types/             # TypeScript types
    │   └── styles/            # CSS styles
    └── public/
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 14+ and npm
- SQL Server or SQL Server LocalDB

### Backend Setup

1. **Configure Database Connection**
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UpgradeNotificationDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

2. **Configure Email Settings**
   Update email settings in `appsettings.json`:
   ```json
   {
     "Email": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": "587",
       "UseSsl": "true",
       "FromEmail": "noreply@yourcompany.com",
       "Username": "your-email@gmail.com",
       "Password": "your-app-password"
     }
   }
   ```

3. **Run the Backend**
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:7030`

### Frontend Setup

1. **Install Dependencies**
   ```bash
   cd frontend
   npm install
   ```

2. **Configure API URL**
   Create a `.env` file in the `frontend` directory:
   ```
   REACT_APP_API_URL=https://localhost:7030/api
   REACT_APP_HUB_URL=https://localhost:7030/upgrade-hub
   ```

3. **Run the Frontend**
   ```bash
   npm start
   ```
   The React app will be available at `http://localhost:3000`

## API Endpoints

### Admin Endpoints
- `POST /api/admin/upgrade/plan` - Create a new planned upgrade
- `GET /api/admin/upgrade/{id}` - Get a specific planned upgrade
- `GET /api/admin/upgrades` - Get all planned upgrades
- `GET /api/admin/upgrade/{id}/preferences` - Get user preferences for an upgrade
- `POST /api/admin/upgrade/{id}/cancel` - Cancel a planned upgrade

### User Endpoints
- `GET /api/upgrade/planned` - Get the current active planned upgrade
- `GET /api/upgrade/user/{userId}` - Get user upgrade info with preferences
- `GET /api/upgrade/preference/{userId}/{plannedUpgradeId}` - Get specific user preference
- `POST /api/upgrade/preference` - Set or update user preference

### Example Requests

**Create a Planned Upgrade:**
```bash
curl -X POST https://localhost:7030/api/admin/upgrade/plan \
  -H "Content-Type: application/json" \
  -d '{
    "version": "2.1.0",
    "plannedDateTime": "2024-01-15T02:00:00Z",
    "description": "Major security updates and new features"
  }'
```

**Set User Preference:**
```bash
curl -X POST https://localhost:7030/api/upgrade/preference \
  -H "Content-Type: application/json" \
  -d '{
    "plannedUpgradeId": 1,
    "userId": "user123",
    "userEmail": "user@example.com",
    "preferredDateTime": "2024-01-15T03:00:00Z"
  }'
```

## Real-time Features

### SignalR Hub Events
- **UpgradePlanned** - Fired when a new upgrade is planned
- **UpgradeScheduled** - Fired when a user's upgrade is scheduled

### Background Jobs
The system uses Hangfire to schedule upgrade jobs based on user preferences:
- Jobs are scheduled when users set their preferences
- Default jobs use the planned upgrade time if no preference is set
- Jobs execute the upgrade process and send confirmation emails

## Development Tools

### Swagger UI
API documentation is available at `https://localhost:7030/swagger`

### Hangfire Dashboard
Background job monitoring is available at `https://localhost:7030/hangfire`

## Email Templates

The system includes HTML and plain text email templates for:
- Upgrade notifications (when a new upgrade is planned)
- Upgrade confirmations (when an upgrade is scheduled)

Templates are customizable in the `EmailService.cs` file.

## Testing

### Manual Testing Workflow

1. **Start both backend and frontend**
2. **Create a planned upgrade** using the admin API
3. **Check that real-time notifications appear** in the React frontend
4. **Set user preferences** using the frontend form
5. **Verify email notifications** are sent (check email service configuration)
6. **Monitor background jobs** in the Hangfire dashboard

### Sample Test Data

```json
{
  "version": "1.5.0",
  "plannedDateTime": "2024-12-31T23:59:00Z",
  "description": "Year-end maintenance upgrade with performance improvements"
}
```

## Architecture Notes

### Design Patterns
- **Repository Pattern** via Entity Framework DbContext
- **Service Layer** for business logic separation
- **Dependency Injection** for loose coupling
- **SignalR** for real-time communication
- **Background Services** for scheduled tasks

### Security Considerations
- **Input validation** on all API endpoints
- **SQL injection protection** via Entity Framework
- **CORS configuration** for frontend integration
- **Error handling** with appropriate HTTP status codes

## Deployment

### Production Configuration
1. Update connection strings for production database
2. Configure email service with production SMTP settings
3. Set up SSL certificates for HTTPS
4. Configure reverse proxy (nginx/IIS) for frontend
5. Set up monitoring and logging

### Environment Variables
Consider using environment variables for sensitive configuration:
- Database connection strings
- Email service credentials
- SignalR connection settings

## Troubleshooting

### Common Issues
1. **SignalR connection fails** - Check CORS configuration and hub URL
2. **Email notifications not sent** - Verify SMTP settings and credentials
3. **Database connection errors** - Check connection string and SQL Server availability
4. **Frontend build errors** - Ensure all npm dependencies are installed

### Logs
Check application logs for detailed error information:
- ASP.NET Core logs for backend issues
- Browser console for frontend issues
- Hangfire dashboard for background job issues