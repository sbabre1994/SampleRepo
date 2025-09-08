import React from 'react';
import NotificationItem from './NotificationItem';

/**
 * Notification category group component
 * Groups and displays notifications by category
 */
const NotificationCategory = ({ categoryName, notifications }) => {
  if (!notifications || notifications.length === 0) {
    return null;
  }

  // Sort notifications by timestamp (newest first)
  const sortedNotifications = [...notifications].sort(
    (a, b) => b.timestamp - a.timestamp
  );

  return (
    <div className="notification-category">
      <div className="category-header">
        <h3 className="category-title">{categoryName}</h3>
        <span className="category-count">({notifications.length})</span>
      </div>
      <div className="category-notifications">
        {sortedNotifications.map((notification) => (
          <NotificationItem key={notification.id} notification={notification} />
        ))}
      </div>
    </div>
  );
};

export default NotificationCategory;