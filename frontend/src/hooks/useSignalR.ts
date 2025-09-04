import { useState, useEffect } from 'react';
import { SignalRService } from '../services/signalRService';

export const useSignalR = (userId: string) => {
  const [signalRService] = useState(() => new SignalRService());
  const [isConnected, setIsConnected] = useState(false);
  const [connectionError, setConnectionError] = useState<string | null>(null);

  useEffect(() => {
    const connectSignalR = async () => {
      try {
        await signalRService.start();
        await signalRService.joinUserGroup(userId);
        setIsConnected(true);
        setConnectionError(null);
      } catch (error) {
        console.error('Failed to connect to SignalR:', error);
        setConnectionError('Failed to connect to real-time notifications');
        setIsConnected(false);
      }
    };

    connectSignalR();

    return () => {
      const disconnect = async () => {
        try {
          await signalRService.leaveUserGroup(userId);
          await signalRService.stop();
        } catch (error) {
          console.error('Error disconnecting SignalR:', error);
        }
      };
      disconnect();
    };
  }, [signalRService, userId]);

  return {
    signalRService,
    isConnected,
    connectionError,
  };
};