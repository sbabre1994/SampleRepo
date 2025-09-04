import React, { useState, useEffect } from 'react';
import { UpgradeNotificationDto } from '../types';
import { ApiService } from '../services/apiService';
import { useSignalR } from '../hooks/useSignalR';
import UpgradePreferenceFormSimple from './UpgradePreferenceFormSimple';
import '../styles/upgrade-app.css';

// Demo user ID - in a real app, this would come from authentication
const DEMO_USER_ID = 'user123';
const DEMO_USER_EMAIL = 'user@example.com';

const UpgradeNotificationApp: React.FC = () => {
  const [upgradeInfo, setUpgradeInfo] = useState<UpgradeNotificationDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [notifications, setNotifications] = useState<string[]>([]);
  
  const { signalRService, isConnected, connectionError } = useSignalR(DEMO_USER_ID);

  useEffect(() => {
    loadUpgradeInfo();
  }, []);

  useEffect(() => {
    if (signalRService && isConnected) {
      // Listen for new upgrade planned notifications
      signalRService.onUpgradePlanned((upgrade) => {
        addNotification(`New Upgrade Planned! 🚀 Version ${upgrade.Version} is scheduled for ${new Date(upgrade.PlannedDateTime).toLocaleString()}`);
        
        // Reload upgrade info
        loadUpgradeInfo();
      });

      // Listen for upgrade scheduled confirmations
      signalRService.onUpgradeScheduled((payload) => {
        addNotification(`Upgrade Scheduled! ✅ Your upgrade to version ${payload.version} is scheduled for ${new Date(payload.scheduledDateTime).toLocaleString()}`);
      });
    }

    return () => {
      if (signalRService) {
        signalRService.off('UpgradePlanned');
        signalRService.off('UpgradeScheduled');
      }
    };
  }, [signalRService, isConnected]);

  const addNotification = (message: string) => {
    setNotifications(prev => [message, ...prev].slice(0, 5)); // Keep only last 5 notifications
    setTimeout(() => {
      setNotifications(prev => prev.filter(n => n !== message));
    }, 10000); // Remove after 10 seconds
  };

  const loadUpgradeInfo = async () => {
    try {
      setLoading(true);
      const info = await ApiService.getUserUpgradeInfo(DEMO_USER_ID);
      setUpgradeInfo(info);
      setError(null);
    } catch (err) {
      setError('Failed to load upgrade information');
      console.error('Error loading upgrade info:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePreferenceSet = () => {
    addNotification('Preference Saved! ✅ Your upgrade preference has been saved successfully.');
    
    // Reload upgrade info to get updated preference
    loadUpgradeInfo();
  };

  if (loading) {
    return (
      <div className="container">
        <p>Loading upgrade information...</p>
      </div>
    );
  }

  return (
    <div className="container">
      <div className="notifications">
        {notifications.map((notification, index) => (
          <div key={index} className="notification success">
            {notification}
          </div>
        ))}
      </div>

      <div className="header">
        <h1>System Upgrade Notification</h1>
        <p>Stay informed about planned system upgrades and set your preferences</p>
      </div>

      {connectionError && (
        <div className="alert warning">
          <strong>Real-time notifications unavailable:</strong> {connectionError}
        </div>
      )}

      {isConnected && (
        <div className="alert success">
          <strong>Connected to real-time notifications</strong> - You'll receive instant notifications about new upgrades
        </div>
      )}

      {error && (
        <div className="alert error">
          <strong>Error:</strong> {error}
        </div>
      )}

      {!upgradeInfo ? (
        <div className="alert info">
          <strong>No planned upgrades</strong> - There are currently no planned system upgrades.
        </div>
      ) : (
        <div>
          <div className="upgrade-info">
            <div className="upgrade-header">
              <h2>Planned Upgrade: Version {upgradeInfo.version}</h2>
              <span className="badge active">Active</span>
            </div>
            
            <p>
              <strong>Planned Date & Time:</strong>{' '}
              {new Date(upgradeInfo.plannedDateTime).toLocaleString()}
            </p>
            
            {upgradeInfo.description && (
              <p>
                <strong>Description:</strong> {upgradeInfo.description}
              </p>
            )}

            {upgradeInfo.hasUserPreference && upgradeInfo.userPreferredDateTime && (
              <p className="preference">
                <strong>Your Preferred Time:</strong>{' '}
                {new Date(upgradeInfo.userPreferredDateTime).toLocaleString()}
              </p>
            )}
          </div>

          <UpgradePreferenceFormSimple
            upgradeInfo={upgradeInfo}
            userId={DEMO_USER_ID}
            userEmail={DEMO_USER_EMAIL}
            onPreferenceSet={handlePreferenceSet}
          />
        </div>
      )}
    </div>
  );
};

export default UpgradeNotificationApp;