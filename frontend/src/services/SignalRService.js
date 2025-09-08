import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.callbacks = {
      newUpgradeScheduled: [],
      preferenceUpdated: [],
      upgradeExecuted: []
    };
  }

  async start() {
    try {
      this.connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5000/upgradeHub', {
          withCredentials: false
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      this.connection.on('NewUpgradeScheduled', (data) => {
        console.log('New upgrade scheduled:', data);
        this.callbacks.newUpgradeScheduled.forEach(callback => callback(data));
      });

      this.connection.on('PreferenceUpdated', (data) => {
        console.log('Preference updated:', data);
        this.callbacks.preferenceUpdated.forEach(callback => callback(data));
      });

      this.connection.on('UpgradeExecuted', (data) => {
        console.log('Upgrade executed:', data);
        this.callbacks.upgradeExecuted.forEach(callback => callback(data));
      });

      await this.connection.start();
      console.log('SignalR connection started successfully');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
    }
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop();
      console.log('SignalR connection stopped');
    }
  }

  async joinUserGroup(userId) {
    if (this.connection && this.connection.state === 'Connected') {
      try {
        await this.connection.invoke('JoinUserGroup', userId);
        console.log(`Joined user group: ${userId}`);
      } catch (error) {
        console.error('Error joining user group:', error);
      }
    }
  }

  async leaveUserGroup(userId) {
    if (this.connection && this.connection.state === 'Connected') {
      try {
        await this.connection.invoke('LeaveUserGroup', userId);
        console.log(`Left user group: ${userId}`);
      } catch (error) {
        console.error('Error leaving user group:', error);
      }
    }
  }

  onNewUpgradeScheduled(callback) {
    this.callbacks.newUpgradeScheduled.push(callback);
  }

  onPreferenceUpdated(callback) {
    this.callbacks.preferenceUpdated.push(callback);
  }

  onUpgradeExecuted(callback) {
    this.callbacks.upgradeExecuted.push(callback);
  }

  // Remove callback when component unmounts
  removeCallback(eventType, callback) {
    const index = this.callbacks[eventType].indexOf(callback);
    if (index > -1) {
      this.callbacks[eventType].splice(index, 1);
    }
  }
}

export default SignalRService;