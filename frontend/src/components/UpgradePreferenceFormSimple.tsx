import React, { useState, useEffect } from 'react';
import { UpgradeNotificationDto, SetPreferenceRequest } from '../types';
import { ApiService } from '../services/apiService';
import '../styles/upgrade-form.css';

interface UpgradePreferenceFormProps {
  upgradeInfo: UpgradeNotificationDto;
  userId: string;
  userEmail: string;
  onPreferenceSet: () => void;
}

const UpgradePreferenceFormSimple: React.FC<UpgradePreferenceFormProps> = ({
  upgradeInfo,
  userId,
  userEmail,
  onPreferenceSet,
}) => {
  const [preferredDateTime, setPreferredDateTime] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [useDefaultTime, setUseDefaultTime] = useState(false);
  const [message, setMessage] = useState<{text: string, type: 'success' | 'error'} | null>(null);

  useEffect(() => {
    if (upgradeInfo.userPreferredDateTime) {
      // Convert ISO string to datetime-local format
      const date = new Date(upgradeInfo.userPreferredDateTime);
      const localDateTime = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);
      setPreferredDateTime(localDateTime);
    }
  }, [upgradeInfo.userPreferredDateTime]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    setIsSubmitting(true);
    setMessage(null);
    
    try {
      const request: SetPreferenceRequest = {
        plannedUpgradeId: upgradeInfo.plannedUpgradeId,
        userId,
        userEmail,
        preferredDateTime: useDefaultTime ? undefined : new Date(preferredDateTime).toISOString(),
      };

      await ApiService.setUserPreference(request);
      onPreferenceSet();
      
      setMessage({
        text: useDefaultTime 
          ? 'You will receive the upgrade at the default planned time.'
          : 'Your preferred upgrade time has been saved.',
        type: 'success'
      });
    } catch (error) {
      console.error('Error setting preference:', error);
      setMessage({
        text: 'Failed to save your preference. Please try again.',
        type: 'error'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUseDefaultTime = () => {
    setUseDefaultTime(true);
    setPreferredDateTime('');
  };

  const handleCustomTime = () => {
    setUseDefaultTime(false);
    if (!preferredDateTime) {
      // Set a default time 1 hour after the planned time
      const plannedDate = new Date(upgradeInfo.plannedDateTime);
      plannedDate.setHours(plannedDate.getHours() + 1);
      const localDateTime = new Date(plannedDate.getTime() - plannedDate.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);
      setPreferredDateTime(localDateTime);
    }
  };

  const getMinDateTime = () => {
    const now = new Date();
    return new Date(now.getTime() - now.getTimezoneOffset() * 60000)
      .toISOString()
      .slice(0, 16);
  };

  return (
    <div className="preference-form">
      <div className="divider"></div>
      
      <div className="form-container">
        <div className="form-header">
          <h3>Set Your Upgrade Preference</h3>
          <p>
            Choose when you'd like to receive the upgrade. If you don't set a preference,
            the upgrade will proceed at the planned time.
          </p>
        </div>

        {upgradeInfo.hasUserPreference && (
          <div className="alert info">
            <strong>Current Preference:</strong>{' '}
            {upgradeInfo.userPreferredDateTime
              ? `Custom time: ${new Date(upgradeInfo.userPreferredDateTime).toLocaleString()}`
              : 'Default planned time'
            }
          </div>
        )}

        {message && (
          <div className={`alert ${message.type}`}>
            {message.text}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="button-group">
            <button
              type="button"
              className={`btn ${useDefaultTime ? 'btn-primary' : 'btn-outline'}`}
              onClick={handleUseDefaultTime}
            >
              Use Default Time
            </button>
            <button
              type="button"
              className={`btn ${!useDefaultTime ? 'btn-success' : 'btn-outline'}`}
              onClick={handleCustomTime}
            >
              Choose Custom Time
            </button>
          </div>

          {useDefaultTime && (
            <div className="alert info">
              <div>
                <strong>Default Planned Time</strong>
                <br />
                {new Date(upgradeInfo.plannedDateTime).toLocaleString()}
                <br />
                <span className="badge recommended">Recommended</span>
              </div>
            </div>
          )}

          {!useDefaultTime && (
            <div className="form-group">
              <label htmlFor="preferredDateTime">Preferred Date & Time</label>
              <input
                type="datetime-local"
                id="preferredDateTime"
                value={preferredDateTime}
                onChange={(e) => setPreferredDateTime(e.target.value)}
                min={getMinDateTime()}
                required
              />
              <small>Select your preferred upgrade time (must be in the future)</small>
            </div>
          )}

          <div className="form-actions">
            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSubmitting || (!useDefaultTime && !preferredDateTime)}
            >
              {isSubmitting ? 'Saving...' : 'Save Preference'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default UpgradePreferenceFormSimple;