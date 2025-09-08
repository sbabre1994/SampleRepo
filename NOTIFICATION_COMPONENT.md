# Notification List Component

A scalable JSX-based notification list component that renders notifications dynamically from a data array with categorization and clean styling.

![Notification List Component](https://github.com/user-attachments/assets/9d9e89aa-478c-4590-9133-c5c1b622dcb5)

## Features

✅ **Dynamic Rendering**: Renders notifications from a data array  
✅ **Categorization**: Groups notifications by category (General, Upgrade History, Security, etc.)  
✅ **Rich Display**: Shows notification type, description, and timestamp  
✅ **Scalable Structure**: Easy to add new notification types and features  
✅ **Responsive Design**: Mobile-friendly responsive layout  
✅ **Type-based Styling**: Visual indicators for different notification types (info, success, warning, error)  
✅ **Sorting**: Notifications sorted by timestamp (newest first) within each category  

## Project Structure

```
src/
├── components/
│   ├── NotificationList.jsx        # Main notification list component
│   ├── NotificationList.css        # Styling for the notification list
│   ├── NotificationCategory.jsx    # Category grouping component
│   └── NotificationItem.jsx        # Individual notification item
├── data/
│   └── notificationData.js         # Sample notification data
├── App.jsx                         # Main app component
├── App.css                         # Global app styles
└── index.js                        # React entry point
```

## Getting Started

### Prerequisites
- Node.js (version 14 or higher)
- npm or yarn

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

3. Build for production:
```bash
npm run build
```

## Usage

### Basic Usage

```jsx
import React from 'react';
import NotificationList from './components/NotificationList';
import { notificationData } from './data/notificationData';

function App() {
  return (
    <div className="App">
      <NotificationList notifications={notificationData} />
    </div>
  );
}
```

### Notification Data Structure

Each notification should follow this structure:

```javascript
{
  id: 1,                              // Unique identifier
  type: 'info',                       // 'info', 'success', 'warning', 'error'
  category: 'General',                // Category name for grouping
  description: 'Notification text',   // Main notification content
  timestamp: new Date(),              // JavaScript Date object
}
```

### Adding New Categories

Simply add notifications with new category names to your data array. The component will automatically:
- Create new category sections
- Sort categories (predefined order first, then alphabetically)
- Display category counts

### Adding New Notification Types

1. Add new type styling in `NotificationList.css`:
```css
.notification-newtype {
  border-left-color: #your-color;
  background-color: #your-bg-color;
}

.notification-newtype .notification-type {
  color: #your-text-color;
}
```

2. Add icon mapping in `NotificationItem.jsx`:
```javascript
const getTypeIcon = (type) => {
  const icons = {
    info: 'ℹ️',
    success: '✅',
    warning: '⚠️',
    error: '❌',
    newtype: '🆕',  // Add your new type
  };
  return icons[type] || 'ℹ️';
};
```

## Component Props

### NotificationList

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `notifications` | Array | `[]` | Array of notification objects |

### NotificationCategory

| Prop | Type | Description |
|------|------|-------------|
| `categoryName` | String | Name of the category |
| `notifications` | Array | Array of notifications for this category |

### NotificationItem

| Prop | Type | Description |
|------|------|-------------|
| `notification` | Object | Single notification object |

## Styling

The component uses CSS modules with responsive design:
- Mobile-first approach
- Flexible color scheme for different notification types
- Hover effects and smooth transitions
- Clean, modern design with proper spacing

## Browser Support

- Modern browsers (Chrome, Firefox, Safari, Edge)
- Mobile browsers (iOS Safari, Chrome Mobile)
- Responsive design for all screen sizes

## Future Enhancements

The scalable structure makes it easy to add:
- Notification actions (dismiss, mark as read)
- Filtering and search functionality
- Real-time updates via WebSocket
- Pagination for large notification lists
- Custom notification templates
- Notification persistence
- Push notification integration