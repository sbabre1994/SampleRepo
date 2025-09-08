# Upgrade Notification System

A comprehensive upgrade notification system built with ASP.NET Core and React that allows administrators to schedule system upgrades and enables users to set their preferred upgrade times.

## Features

### Backend (ASP.NET Core)
- **PostgreSQL Database**: Stores planned releases, user preferences, and upgrade history
- **SignalR Hub**: Real-time notifications for upgrade events
- **Email Notifications**: HTML email templates for upgrade notifications
- **Background Jobs**: Hangfire integration for scheduled upgrade execution
- **S3 Integration**: Automatic data backup to AWS S3 (with mock service for development)
- **RESTful API**: Complete API for managing upgrades and preferences

### Frontend (React + Chakra UI)
- **Real-time Notifications**: Toast notifications via SignalR
- **Upgrade Preferences**: User-friendly form to set upgrade times
- **Upgrade History**: Timeline view of all upgrade events
- **Admin Panel**: Interface for scheduling new upgrades

## Prerequisites

- .NET 8.0 SDK
- Node.js 16+
- PostgreSQL (optional - uses in-memory database for development)
- AWS S3 (optional - uses mock service for development)

## Quick Start

### 1. Backend Setup

```bash
# Navigate to project root
cd UpgradeNotificationSystem

# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the backend
dotnet run
```

The backend will start on `https://localhost:7000` (or `http://localhost:5000`)

### 2. Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will start on `http://localhost:5173`

## Configuration

### Database Configuration

Update `appsettings.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=upgrade_notification;Username=postgres;Password=yourpassword;"
  }
}
```

### AWS S3 Configuration

For production, configure AWS S3 in `appsettings.json`:

```json
{
  "AWS": {
    "S3": {
      "BucketName": "your-upgrade-notification-backup-bucket"
    }
  }
}
```

## API Endpoints

### Upgrade Management
- `GET /api/upgrade/planned-release` - Get current planned release
- `POST /api/upgrade/planned-release` - Create new planned release (Admin)
- `GET /api/upgrade/preference/{userId}/{plannedReleaseId}` - Get user preference
- `POST /api/upgrade/preference` - Set/update user preference
- `GET /api/upgrade/history` - Get upgrade history (supports filtering)

### Real-time Communication
- `/upgradeHub` - SignalR hub for real-time notifications

### Background Jobs
- `/hangfire` - Hangfire dashboard for monitoring background jobs

## Database Schema

### PlannedRelease
- Version, planned date/time, description, and status
- Relationships to user preferences and history

### UserPreference
- User ID, email, preferred upgrade time, and default time flag
- Links to planned release

### UpgradeHistory
- Event tracking for all system activities
- Includes event type, description, timestamps, and additional data

## Background Jobs

The system uses Hangfire to schedule upgrade jobs:
- **Individual Jobs**: Scheduled for users with custom preferences
- **Default Jobs**: Scheduled for users without preferences
- **Job Monitoring**: Available through Hangfire dashboard

## Email Templates

HTML email templates are included for:
- New upgrade notifications
- Preference confirmations
- Upgrade execution notifications

## Frontend Components

### Key Components
- **UpgradePreferenceForm**: Main interface for setting preferences
- **UpgradeHistoryView**: Timeline of upgrade events
- **AdminPanel**: Admin interface for scheduling upgrades
- **SignalR Integration**: Real-time notification handling

### Services
- **ApiService**: HTTP client for backend communication
- **SignalRService**: Real-time communication service

## Development Features

- **Mock Services**: S3 sync service has mock implementation for development
- **In-Memory Database**: Entity Framework can use in-memory database for testing
- **Hot Reload**: Both backend and frontend support hot reload
- **Comprehensive Logging**: Detailed logging for debugging

## Deployment

### Backend Deployment
1. Configure production database and S3
2. Set environment variables
3. Deploy to your hosting platform (Azure, AWS, etc.)

### Frontend Deployment
1. Build the frontend: `npm run build`
2. Deploy the `dist` folder to your static hosting service
3. Update API URLs for production

## Security Considerations

- Implement proper authentication and authorization
- Secure SignalR connections with authentication
- Validate all inputs on both client and server
- Use HTTPS in production
- Secure database connections
- Implement proper CORS policies

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.