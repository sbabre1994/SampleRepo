import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

class ApiService {
  // Planned Release endpoints
  async getPlannedRelease() {
    try {
      const response = await apiClient.get('/upgrade/planned-release');
      return response.data;
    } catch (error) {
      if (error.response?.status === 404) {
        return null; // No active planned release
      }
      throw this.handleApiError(error);
    }
  }

  async createPlannedRelease(releaseData) {
    try {
      const response = await apiClient.post('/upgrade/planned-release', releaseData);
      return response.data;
    } catch (error) {
      throw this.handleApiError(error);
    }
  }

  // User Preference endpoints
  async getUserPreference(userId, plannedReleaseId) {
    try {
      const response = await apiClient.get(`/upgrade/preference/${userId}/${plannedReleaseId}`);
      return response.data;
    } catch (error) {
      if (error.response?.status === 404) {
        return null; // No preference found
      }
      throw this.handleApiError(error);
    }
  }

  async setUserPreference(preferenceData) {
    try {
      const response = await apiClient.post('/upgrade/preference', preferenceData);
      return response.data;
    } catch (error) {
      throw this.handleApiError(error);
    }
  }

  // Upgrade History endpoints
  async getUpgradeHistory(filters = {}) {
    try {
      const params = new URLSearchParams();
      
      if (filters.plannedReleaseId) {
        params.append('plannedReleaseId', filters.plannedReleaseId);
      }
      if (filters.userId) {
        params.append('userId', filters.userId);
      }
      if (filters.page) {
        params.append('page', filters.page);
      }
      if (filters.pageSize) {
        params.append('pageSize', filters.pageSize);
      }

      const response = await apiClient.get(`/upgrade/history?${params.toString()}`);
      return response.data;
    } catch (error) {
      throw this.handleApiError(error);
    }
  }

  // Error handling
  handleApiError(error) {
    if (error.response) {
      // Server responded with error status
      const message = error.response.data?.message || 'An error occurred';
      return new Error(`API Error: ${message} (Status: ${error.response.status})`);
    } else if (error.request) {
      // Request was made but no response received
      return new Error('Network Error: Unable to connect to the server');
    } else {
      // Something else happened
      return new Error(`Request Error: ${error.message}`);
    }
  }
}

const apiService = new ApiService();
export default apiService;