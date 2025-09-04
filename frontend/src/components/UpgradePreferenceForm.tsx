import React, { useState, useEffect } from 'react';
import {
  Box,
  Heading,
  Text,
  Badge,
} from '@chakra-ui/react';
import { Button } from '@chakra-ui/button';
import { FormControl, FormLabel } from '@chakra-ui/form-control';
import { Input } from '@chakra-ui/input';
import { VStack, HStack, Container } from '@chakra-ui/layout';
import { Alert, AlertIcon } from '@chakra-ui/alert';
import { useToast } from '@chakra-ui/toast';
import { UpgradeNotificationDto, SetPreferenceRequest } from '../types';
import { ApiService } from '../services/apiService';

interface UpgradePreferenceFormProps {
  upgradeInfo: UpgradeNotificationDto;
  userId: string;
  userEmail: string;
  onPreferenceSet: () => void;
}

const UpgradePreferenceForm: React.FC<UpgradePreferenceFormProps> = ({
  upgradeInfo,
  userId,
  userEmail,
  onPreferenceSet,
}) => {
  const [preferredDateTime, setPreferredDateTime] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [useDefaultTime, setUseDefaultTime] = useState(false);
  const toast = useToast();

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
    
    try {
      const request: SetPreferenceRequest = {
        plannedUpgradeId: upgradeInfo.plannedUpgradeId,
        userId,
        userEmail,
        preferredDateTime: useDefaultTime ? undefined : new Date(preferredDateTime).toISOString(),
      };

      await ApiService.setUserPreference(request);
      onPreferenceSet();
      
      toast({
        title: 'Success!',
        description: useDefaultTime 
          ? 'You will receive the upgrade at the default planned time.'
          : 'Your preferred upgrade time has been saved.',
        status: 'success',
        duration: 5000,
        isClosable: true,
      });
    } catch (error) {
      console.error('Error setting preference:', error);
      toast({
        title: 'Error',
        description: 'Failed to save your preference. Please try again.',
        status: 'error',
        duration: 5000,
        isClosable: true,
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
    <Box mt={6}>
      <Box height="1px" bg="gray.200" mb={6} />
      
      <Box
        p={6}
        borderWidth="1px"
        borderRadius="lg"
        bg="gray.50"
        border="1px solid"
        borderColor="gray.200"
      >
        <VStack align="start" spacing={4}>
          <Box>
            <Heading size="md" mb={2}>
              Set Your Upgrade Preference
            </Heading>
            <Text color="gray.600">
              Choose when you'd like to receive the upgrade. If you don't set a preference,
              the upgrade will proceed at the planned time.
            </Text>
          </Box>

          {upgradeInfo.hasUserPreference && (
            <Alert status="info">
              <AlertIcon />
              <Box>
                <Text fontWeight="medium">Current Preference</Text>
                <Text fontSize="sm">
                  {upgradeInfo.userPreferredDateTime
                    ? `Custom time: ${new Date(upgradeInfo.userPreferredDateTime).toLocaleString()}`
                    : 'Default planned time'
                  }
                </Text>
              </Box>
            </Alert>
          )}

          <form onSubmit={handleSubmit} style={{ width: '100%' }}>
            <VStack spacing={4} align="stretch">
              <HStack spacing={4}>
                <Button
                  variant={useDefaultTime ? 'solid' : 'outline'}
                  colorScheme="blue"
                  onClick={handleUseDefaultTime}
                  flex={1}
                >
                  Use Default Time
                </Button>
                <Button
                  variant={!useDefaultTime ? 'solid' : 'outline'}
                  colorScheme="green"
                  onClick={handleCustomTime}
                  flex={1}
                >
                  Choose Custom Time
                </Button>
              </HStack>

              {useDefaultTime && (
                <Alert status="info">
                  <AlertIcon />
                  <VStack align="start" spacing={1}>
                    <Text fontWeight="medium">Default Planned Time</Text>
                    <Text fontSize="sm">
                      {new Date(upgradeInfo.plannedDateTime).toLocaleString()}
                    </Text>
                    <Badge colorScheme="blue" size="sm">
                      Recommended
                    </Badge>
                  </VStack>
                </Alert>
              )}

              {!useDefaultTime && (
                <FormControl isRequired>
                  <FormLabel>Preferred Date & Time</FormLabel>
                  <Input
                    type="datetime-local"
                    value={preferredDateTime}
                    onChange={(e) => setPreferredDateTime(e.target.value)}
                    min={getMinDateTime()}
                    bg="white"
                  />
                  <Text fontSize="sm" color="gray.500" mt={1}>
                    Select your preferred upgrade time (must be in the future)
                  </Text>
                </FormControl>
              )}

              <HStack spacing={3} justify="flex-end">
                <Button
                  type="submit"
                  colorScheme="blue"
                  isLoading={isSubmitting}
                  loadingText="Saving..."
                  isDisabled={!useDefaultTime && !preferredDateTime}
                >
                  Save Preference
                </Button>
              </HStack>
            </VStack>
          </form>
        </VStack>
      </Box>
    </Box>
  );
};

export default UpgradePreferenceForm;