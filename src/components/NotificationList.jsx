import React, { useMemo } from 'react';
import NotificationCategory from './NotificationCategory';
import './NotificationList.css';

/**
 * Main notification list component
 * Renders notifications dynamically from a data array, grouped by category
 * Provides a scalable structure for future notification types and features
 */
const NotificationList = ({ notifications = [] }) => {
  // Group notifications by category
  const groupedNotifications = useMemo(() => {
    const groups = {};
    
    notifications.forEach((notification) => {
      const category = notification.category || 'Uncategorized';
      if (!groups[category]) {
        groups[category] = [];
      }
      groups[category].push(notification);
    });
    
    return groups;
  }, [notifications]);

  // Get sorted category names for consistent ordering
  const sortedCategories = useMemo(() => {
    const categories = Object.keys(groupedNotifications);
    
    // Define preferred order for categories
    const categoryOrder = ['General', 'Upgrade History', 'Security'];
    
    // Sort categories: predefined order first, then alphabetically
    return categories.sort((a, b) => {
      const aIndex = categoryOrder.indexOf(a);
      const bIndex = categoryOrder.indexOf(b);
      
      if (aIndex !== -1 && bIndex !== -1) {
        return aIndex - bIndex;
      }
      if (aIndex !== -1) return -1;
      if (bIndex !== -1) return 1;
      return a.localeCompare(b);
    });
  }, [groupedNotifications]);

  const getTotalNotificationCount = () => {
    return notifications.length;
  };

  if (notifications.length === 0) {
    return (
      <div className="notification-list">
        <div className="notification-header">
          <h2>Notifications</h2>
        </div>
        <div className="empty-state">
          <p>No notifications available</p>
        </div>
      </div>
    );
  }

  return (
    <div className="notification-list">
      <div className="notification-header">
        <h2>Notifications</h2>
        <span className="total-count">
          {getTotalNotificationCount()} total
        </span>
      </div>
      
      <div className="notification-categories">
        {sortedCategories.map((categoryName) => (
          <NotificationCategory
            key={categoryName}
            categoryName={categoryName}
            notifications={groupedNotifications[categoryName]}
          />
        ))}
      </div>
    </div>
  );
};

export default NotificationList;