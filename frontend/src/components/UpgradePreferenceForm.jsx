import {
  Box,
  Card,
  CardHeader,
  CardBody,
  Heading,
  Text,
  VStack,
  HStack,
  FormControl,
  FormLabel,
  Input,
  Switch,
  Button,
  Alert,
  AlertIcon,
  AlertDescription,
  useToast,
  Divider,
  Badge,
  Spinner,
  Center
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import apiService from '../services/ApiService';

const UpgradePreferenceForm = ({ 
  currentUser, 
  plannedRelease, 
  setPlannedRelease, 
  userPreference, 
  setUserPreference,
  signalRService 
}) => {
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [useDefaultTime, setUseDefaultTime] = useState(true);
  const [preferredDateTime, setPreferredDateTime] = useState('');
  const toast = useToast();

  // Load planned release and user preference on component mount
  useEffect(() => {
    loadData();
  }, []);

  // Update form when user preference changes
  useEffect(() => {
    if (userPreference) {
      setUseDefaultTime(userPreference.useDefaultTime);
      if (userPreference.preferredDateTime) {
        // Convert to local datetime-local format
        const date = new Date(userPreference.preferredDateTime);
        const localDateTime = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
          .toISOString()
          .slice(0, 16);
        setPreferredDateTime(localDateTime);
      } else {
        setPreferredDateTime('');
      }
    }
  }, [userPreference]);

  // Listen for preference updates from SignalR
  useEffect(() => {
    const handlePreferenceUpdated = (data) => {
      toast({
        title: 'Preference Updated!',
        description: 'Your upgrade preference has been saved successfully',
        status: 'success',
        duration: 5000,
        isClosable: true,
        position: 'top-right',
      });
    };

    signalRService.onPreferenceUpdated(handlePreferenceUpdated);

    return () => {
      signalRService.removeCallback('preferenceUpdated', handlePreferenceUpdated);
    };
  }, [signalRService, toast]);

  const loadData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      // Load planned release
      const release = await apiService.getPlannedRelease();
      setPlannedRelease(release);

      // Load user preference if release exists
      if (release && currentUser) {
        const preference = await apiService.getUserPreference(currentUser.id, release.id);
        setUserPreference(preference);
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!plannedRelease) {
      setError('No planned release available');
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      const preferenceData = {
        userId: currentUser.id,
        userEmail: currentUser.email,
        plannedReleaseId: plannedRelease.id,
        useDefaultTime: useDefaultTime,
        preferredDateTime: useDefaultTime ? null : new Date(preferredDateTime).toISOString()
      };

      const updatedPreference = await apiService.setUserPreference(preferenceData);
      setUserPreference(updatedPreference);

      toast({
        title: 'Preference Saved',
        description: 'Your upgrade preference has been saved successfully',
        status: 'success',
        duration: 5000,
        isClosable: true,
      });
    } catch (err) {
      setError(err.message);
      toast({
        title: 'Error',
        description: err.message,
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    } finally {
      setSubmitting(false);
    }
  };

  const formatDateTime = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  if (loading) {
    return (
      <Center py={8}>
        <VStack>
          <Spinner size="lg" color="blue.500" />
          <Text>Loading upgrade information...</Text>
        </VStack>
      </Center>
    );
  }

  if (!plannedRelease) {
    return (
      <Card>
        <CardBody>
          <Alert status="info">
            <AlertIcon />
            <AlertDescription>
              No planned upgrades at this time. Check back later for upcoming system upgrades.
            </AlertDescription>
          </Alert>
        </CardBody>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <Heading size="md" color="blue.600">Upgrade Preference</Heading>
      </CardHeader>
      <CardBody>
        <VStack spacing={6} align="stretch">
          {/* Planned Release Information */}
          <Box p={4} bg="blue.50" borderRadius="md" border="1px" borderColor="blue.200">
            <VStack align="start" spacing={2}>
              <HStack>
                <Text fontWeight="bold">Planned Version:</Text>
                <Badge colorScheme="blue" fontSize="md">{plannedRelease.version}</Badge>
              </HStack>
              <HStack>
                <Text fontWeight="bold">Scheduled Time:</Text>
                <Text>{formatDateTime(plannedRelease.plannedDateTime)}</Text>
              </HStack>
              {plannedRelease.description && (
                <VStack align="start" spacing={1} w="full">
                  <Text fontWeight="bold">Description:</Text>
                  <Text fontSize="sm" color="gray.600">{plannedRelease.description}</Text>
                </VStack>
              )}
            </VStack>
          </Box>

          <Divider />

          {/* Current Preference Display */}
          {userPreference && (
            <Box p={4} bg="green.50" borderRadius="md" border="1px" borderColor="green.200">
              <VStack align="start" spacing={2}>
                <Text fontWeight="bold" color="green.700">Current Preference:</Text>
                {userPreference.useDefaultTime ? (
                  <Text>Using default scheduled time</Text>
                ) : (
                  <Text>Custom time: {formatDateTime(userPreference.preferredDateTime)}</Text>
                )}
                <Text fontSize="sm" color="gray.600">
                  Last updated: {formatDateTime(userPreference.updatedAt)}
                </Text>
              </VStack>
            </Box>
          )}

          {/* Preference Form */}
          <form onSubmit={handleSubmit}>
            <VStack spacing={4} align="stretch">
              <FormControl>
                <HStack justify="space-between">
                  <FormLabel htmlFor="use-default-time" mb={0}>
                    Use Default Scheduled Time
                  </FormLabel>
                  <Switch 
                    id="use-default-time"
                    isChecked={useDefaultTime}
                    onChange={(e) => setUseDefaultTime(e.target.checked)}
                    colorScheme="blue"
                  />
                </HStack>
                <Text fontSize="sm" color="gray.600" mt={1}>
                  {useDefaultTime 
                    ? "Your upgrade will happen at the default scheduled time" 
                    : "Choose your preferred upgrade time below"
                  }
                </Text>
              </FormControl>

              {!useDefaultTime && (
                <FormControl isRequired>
                  <FormLabel>Preferred Upgrade Time</FormLabel>
                  <Input
                    type="datetime-local"
                    value={preferredDateTime}
                    onChange={(e) => setPreferredDateTime(e.target.value)}
                    min={new Date().toISOString().slice(0, 16)}
                  />
                  <Text fontSize="sm" color="gray.600" mt={1}>
                    Choose a time that works best for you
                  </Text>
                </FormControl>
              )}

              {error && (
                <Alert status="error">
                  <AlertIcon />
                  <AlertDescription>{error}</AlertDescription>
                </Alert>
              )}

              <Button
                type="submit"
                colorScheme="blue"
                isLoading={submitting}
                loadingText="Saving..."
                size="lg"
              >
                Save Preference
              </Button>
            </VStack>
          </form>
        </VStack>
      </CardBody>
    </Card>
  );
};

export default UpgradePreferenceForm;