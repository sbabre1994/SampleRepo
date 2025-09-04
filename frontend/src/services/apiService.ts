import { UpgradeNotificationDto, SetPreferenceRequest } from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7030/api';

export class ApiService {
  static async getPlannedUpgrade(): Promise<UpgradeNotificationDto | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/upgrade/planned`);
      if (response.status === 404) {
        return null;
      }
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Error fetching planned upgrade:', error);
      throw error;
    }
  }

  static async getUserUpgradeInfo(userId: string): Promise<UpgradeNotificationDto | null> {
    try {
      const response = await fetch(`${API_BASE_URL}/upgrade/user/${userId}`);
      if (response.status === 404) {
        return null;
      }
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Error fetching user upgrade info:', error);
      throw error;
    }
  }

  static async setUserPreference(request: SetPreferenceRequest): Promise<any> {
    try {
      const response = await fetch(`${API_BASE_URL}/upgrade/preference`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error('Error setting user preference:', error);
      throw error;
    }
  }
}