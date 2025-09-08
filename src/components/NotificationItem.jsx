import React from 'react';

/**
 * Individual notification item component
 * Displays a single notification with type, description, and timestamp
 */
const NotificationItem = ({ notification }) => {
  const formatTimestamp = (timestamp) => {
    return timestamp.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getTypeIcon = (type) => {
    const icons = {
      info: 'ℹ️',
      success: '✅',
      warning: '⚠️',
      error: '❌',
    };
    return icons[type] || 'ℹ️';
  };

  return (
    <div className={`notification-item notification-${notification.type}`}>
      <div className="notification-header">
        <span className="notification-icon">{getTypeIcon(notification.type)}</span>
        <span className="notification-type">{notification.type.toUpperCase()}</span>
        <span className="notification-timestamp">
          {formatTimestamp(notification.timestamp)}
        </span>
      </div>
      <div className="notification-description">
        {notification.description}
      </div>
    </div>
  );
};

export default NotificationItem;