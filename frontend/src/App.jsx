import { ChakraProvider, Container, VStack, Heading, Box, Text, Tabs, TabList, TabPanels, Tab, TabPanel } from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import UpgradeNotificationToast from './components/UpgradeNotificationToast';
import UpgradePreferenceForm from './components/UpgradePreferenceForm';
import UpgradeHistoryView from './components/UpgradeHistoryView';
import AdminPanel from './components/AdminPanel';
import SignalRService from './services/SignalRService';

function App() {
  const [currentUser] = useState({
    id: 'user1', // In a real app, this would come from authentication
    email: 'user1@example.com'
  });
  
  const [plannedRelease, setPlannedRelease] = useState(null);
  const [userPreference, setUserPreference] = useState(null);
  const [signalRService] = useState(new SignalRService());

  useEffect(() => {
    // Initialize SignalR connection
    signalRService.start().then(() => {
      signalRService.joinUserGroup(currentUser.id);
    });

    // Setup SignalR event handlers
    signalRService.onNewUpgradeScheduled((data) => {
      setPlannedRelease(data);
      // Toast notification will be handled by UpgradeNotificationToast component
    });

    signalRService.onPreferenceUpdated((data) => {
      setUserPreference(data);
      // Toast notification will be handled by UpgradePreferenceForm component
    });

    // Cleanup on unmount
    return () => {
      signalRService.stop();
    };
  }, [currentUser.id, signalRService]);

  const handleReleaseCreated = (newRelease) => {
    setPlannedRelease(newRelease);
    // Clear user preference since it's a new release
    setUserPreference(null);
  };

  return (
    <ChakraProvider>
      <Container maxW="container.xl" py={8}>
        <VStack spacing={8} align="stretch">
          <Box textAlign="center">
            <Heading as="h1" size="xl" color="blue.600" mb={2}>
              Upgrade Notification System
            </Heading>
            <Text color="gray.600">
              Manage your system upgrade preferences and view upgrade history
            </Text>
          </Box>

          <UpgradeNotificationToast 
            signalRService={signalRService}
          />

          <Tabs variant="enclosed" colorScheme="blue">
            <TabList>
              <Tab>Upgrade Preferences</Tab>
              <Tab>Upgrade History</Tab>
              <Tab>Admin Panel</Tab>
            </TabList>

            <TabPanels>
              <TabPanel px={0}>
                <UpgradePreferenceForm 
                  currentUser={currentUser}
                  plannedRelease={plannedRelease}
                  setPlannedRelease={setPlannedRelease}
                  userPreference={userPreference}
                  setUserPreference={setUserPreference}
                  signalRService={signalRService}
                />
              </TabPanel>

              <TabPanel px={0}>
                <UpgradeHistoryView 
                  currentUser={currentUser}
                />
              </TabPanel>

              <TabPanel px={0}>
                <AdminPanel 
                  onReleaseCreated={handleReleaseCreated}
                />
              </TabPanel>
            </TabPanels>
          </Tabs>
        </VStack>
      </Container>
    </ChakraProvider>
  );
}

export default App;
