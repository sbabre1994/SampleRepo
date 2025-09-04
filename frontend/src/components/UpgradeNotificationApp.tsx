import React, { useState, useEffect } from 'react';
import {
  Box,
  Heading,
  Text,
  Badge,
} from '@chakra-ui/react';
import { VStack, Container } from '@chakra-ui/layout';
import { Alert, AlertIcon, AlertTitle, AlertDescription } from '@chakra-ui/alert';
import { useToast } from '@chakra-ui/toast';
import { UpgradeNotificationDto } from '../types';
import { ApiService } from '../services/apiService';
import { useSignalR } from '../hooks/useSignalR';
import UpgradePreferenceForm from './UpgradePreferenceForm';

// Demo user ID - in a real app, this would come from authentication
const DEMO_USER_ID = 'user123';
const DEMO_USER_EMAIL = 'user@example.com';

const UpgradeNotificationApp: React.FC = () => {
  const [upgradeInfo, setUpgradeInfo] = useState<UpgradeNotificationDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const toast = useToast();
  
  const { signalRService, isConnected, connectionError } = useSignalR(DEMO_USER_ID);

  useEffect(() => {
    loadUpgradeInfo();
  }, []);

  useEffect(() => {
    if (signalRService && isConnected) {
      // Listen for new upgrade planned notifications
      signalRService.onUpgradePlanned((upgrade) => {
        toast({
          title: 'New Upgrade Planned! 🚀',
          description: `Version ${upgrade.Version} is scheduled for ${new Date(upgrade.PlannedDateTime).toLocaleString()}`,
          status: 'info',
          duration: 10000,
          isClosable: true,
          position: 'top-right',
        });
        
        // Reload upgrade info
        loadUpgradeInfo();
      });

      // Listen for upgrade scheduled confirmations
      signalRService.onUpgradeScheduled((payload) => {
        toast({
          title: 'Upgrade Scheduled! ✅',
          description: `Your upgrade to version ${payload.version} is scheduled for ${new Date(payload.scheduledDateTime).toLocaleString()}`,
          status: 'success',
          duration: 8000,
          isClosable: true,
          position: 'top-right',
        });
      });
    }

    return () => {
      if (signalRService) {
        signalRService.off('UpgradePlanned');
        signalRService.off('UpgradeScheduled');
      }
    };
  }, [signalRService, isConnected, toast]);

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
    toast({
      title: 'Preference Saved! ✅',
      description: 'Your upgrade preference has been saved successfully.',
      status: 'success',
      duration: 5000,
      isClosable: true,
    });
    
    // Reload upgrade info to get updated preference
    loadUpgradeInfo();
  };

  if (loading) {
    return (
      <Container maxW="container.md" py={8}>
        <Text>Loading upgrade information...</Text>
      </Container>
    );
  }

  return (
    <Container maxW="container.md" py={8}>
      <VStack spacing={6} align="stretch">
        <Box textAlign="center">
          <Heading size="lg" mb={2}>
            System Upgrade Notification
          </Heading>
          <Text color="gray.600">
            Stay informed about planned system upgrades and set your preferences
          </Text>
        </Box>

        {connectionError && (
          <Alert status="warning">
            <AlertIcon />
            <AlertTitle>Real-time notifications unavailable</AlertTitle>
            <AlertDescription>{connectionError}</AlertDescription>
          </Alert>
        )}

        {isConnected && (
          <Alert status="success">
            <AlertIcon />
            <AlertTitle>Connected to real-time notifications</AlertTitle>
            <AlertDescription>
              You'll receive instant notifications about new upgrades
            </AlertDescription>
          </Alert>
        )}

        {error && (
          <Alert status="error">
            <AlertIcon />
            <AlertTitle>Error</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {!upgradeInfo ? (
          <Alert status="info">
            <AlertIcon />
            <AlertTitle>No planned upgrades</AlertTitle>
            <AlertDescription>
              There are currently no planned system upgrades.
            </AlertDescription>
          </Alert>
        ) : (
          <Box>
            <Box
              p={6}
              borderWidth="1px"
              borderRadius="lg"
              bg="blue.50"
              border="1px solid"
              borderColor="blue.200"
            >
              <VStack align="start" spacing={3}>
                <Box>
                  <Heading size="md" color="blue.700">
                    Planned Upgrade: Version {upgradeInfo.version}
                  </Heading>
                  <Badge colorScheme="blue" mt={1}>
                    Active
                  </Badge>
                </Box>
                
                <Text>
                  <strong>Planned Date & Time:</strong>{' '}
                  {new Date(upgradeInfo.plannedDateTime).toLocaleString()}
                </Text>
                
                {upgradeInfo.description && (
                  <Text>
                    <strong>Description:</strong> {upgradeInfo.description}
                  </Text>
                )}

                {upgradeInfo.hasUserPreference && upgradeInfo.userPreferredDateTime && (
                  <Text color="green.600">
                    <strong>Your Preferred Time:</strong>{' '}
                    {new Date(upgradeInfo.userPreferredDateTime).toLocaleString()}
                  </Text>
                )}
              </VStack>
            </Box>

            <UpgradePreferenceForm
              upgradeInfo={upgradeInfo}
              userId={DEMO_USER_ID}
              userEmail={DEMO_USER_EMAIL}
              onPreferenceSet={handlePreferenceSet}
            />
          </Box>
        )}
      </VStack>
    </Container>
  );
};

export default UpgradeNotificationApp;