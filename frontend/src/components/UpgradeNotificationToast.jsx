import { useToast } from '@chakra-ui/react';
import { useEffect } from 'react';

const UpgradeNotificationToast = ({ signalRService }) => {
  const toast = useToast();

  useEffect(() => {
    const handleNewUpgradeScheduled = (data) => {
      toast({
        title: 'New Upgrade Scheduled!',
        description: `Version ${data.version} is scheduled for ${new Date(data.plannedDateTime).toLocaleDateString()} at ${new Date(data.plannedDateTime).toLocaleTimeString()}`,
        status: 'info',
        duration: 10000,
        isClosable: true,
        position: 'top-right',
      });
    };

    const handleUpgradeExecuted = (data) => {
      toast({
        title: 'Upgrade Executed',
        description: `Version ${data.version} has been successfully upgraded`,
        status: 'success',
        duration: 8000,
        isClosable: true,
        position: 'top-right',
      });
    };

    // Subscribe to SignalR events
    signalRService.onNewUpgradeScheduled(handleNewUpgradeScheduled);
    signalRService.onUpgradeExecuted(handleUpgradeExecuted);

    // Cleanup function
    return () => {
      signalRService.removeCallback('newUpgradeScheduled', handleNewUpgradeScheduled);
      signalRService.removeCallback('upgradeExecuted', handleUpgradeExecuted);
    };
  }, [signalRService, toast]);

  // This component doesn't render anything visible
  return null;
};

export default UpgradeNotificationToast;