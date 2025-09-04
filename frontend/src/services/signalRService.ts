import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

export class SignalRService {
  private connection: HubConnection | null = null;
  private readonly hubUrl: string;

  constructor() {
    this.hubUrl = process.env.REACT_APP_HUB_URL || 'https://localhost:7030/upgrade-hub';
  }

  async start(): Promise<void> {
    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .build();

    try {
      await this.connection.start();
      console.log('SignalR connected');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      throw error;
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR disconnected');
    }
  }

  async joinUserGroup(userId: string): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('JoinUserGroup', userId);
    }
  }

  async leaveUserGroup(userId: string): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('LeaveUserGroup', userId);
    }
  }

  onUpgradePlanned(callback: (upgrade: any) => void): void {
    if (this.connection) {
      this.connection.on('UpgradePlanned', callback);
    }
  }

  onUpgradeScheduled(callback: (payload: any) => void): void {
    if (this.connection) {
      this.connection.on('UpgradeScheduled', callback);
    }
  }

  off(methodName: string): void {
    if (this.connection) {
      this.connection.off(methodName);
    }
  }

  getConnectionState(): string {
    return this.connection?.state || 'Disconnected';
  }
}