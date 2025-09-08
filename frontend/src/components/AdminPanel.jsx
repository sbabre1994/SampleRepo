import {
  Box,
  Card,
  CardHeader,
  CardBody,
  Heading,
  VStack,
  HStack,
  FormControl,
  FormLabel,
  Input,
  Textarea,
  Button,
  Alert,
  AlertIcon,
  AlertDescription,
  useToast,
  Badge
} from '@chakra-ui/react';
import { useState } from 'react';
import apiService from '../services/ApiService';

const AdminPanel = ({ onReleaseCreated }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    version: '',
    plannedDateTime: '',
    description: ''
  });
  const toast = useToast();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const releaseData = {
        version: formData.version,
        plannedDateTime: new Date(formData.plannedDateTime).toISOString(),
        description: formData.description || null
      };

      const newRelease = await apiService.createPlannedRelease(releaseData);
      
      toast({
        title: 'Release Scheduled',
        description: `Version ${newRelease.version} has been scheduled successfully`,
        status: 'success',
        duration: 5000,
        isClosable: true,
      });

      // Reset form
      setFormData({
        version: '',
        plannedDateTime: '',
        description: ''
      });

      // Notify parent component
      if (onReleaseCreated) {
        onReleaseCreated(newRelease);
      }
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
      setLoading(false);
    }
  };

  const handleInputChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  return (
    <Card borderLeft="4px solid" borderLeftColor="orange.400">
      <CardHeader>
        <HStack>
          <Heading size="md" color="orange.600">Admin Panel</Heading>
          <Badge colorScheme="orange" variant="subtle">Admin Only</Badge>
        </HStack>
      </CardHeader>
      <CardBody>
        <form onSubmit={handleSubmit}>
          <VStack spacing={4} align="stretch">
            <FormControl isRequired>
              <FormLabel>Version</FormLabel>
              <Input
                placeholder="e.g., 2.1.0"
                value={formData.version}
                onChange={(e) => handleInputChange('version', e.target.value)}
              />
            </FormControl>

            <FormControl isRequired>
              <FormLabel>Planned Date & Time</FormLabel>
              <Input
                type="datetime-local"
                value={formData.plannedDateTime}
                onChange={(e) => handleInputChange('plannedDateTime', e.target.value)}
                min={new Date().toISOString().slice(0, 16)}
              />
            </FormControl>

            <FormControl>
              <FormLabel>Description (Optional)</FormLabel>
              <Textarea
                placeholder="Describe what's included in this upgrade..."
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                rows={3}
              />
            </FormControl>

            {error && (
              <Alert status="error">
                <AlertIcon />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <Button
              type="submit"
              colorScheme="orange"
              isLoading={loading}
              loadingText="Scheduling..."
              size="lg"
            >
              Schedule Upgrade
            </Button>
          </VStack>
        </form>
      </CardBody>
    </Card>
  );
};

export default AdminPanel;