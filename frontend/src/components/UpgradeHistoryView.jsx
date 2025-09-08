import {
  Box,
  Card,
  CardHeader,
  CardBody,
  Heading,
  Text,
  VStack,
  HStack,
  Badge,
  Spinner,
  Center,
  Alert,
  AlertIcon,
  AlertDescription,
  Button,
  Flex,
  Divider,
  Icon,
  Accordion,
  AccordionItem,
  AccordionButton,
  AccordionPanel,
  AccordionIcon
} from '@chakra-ui/react';
import { useState, useEffect } from 'react';
import { FiClock, FiSettings, FiCheckCircle, FiMail, FiCalendar } from 'react-icons/fi';
import apiService from '../services/ApiService';

const UpgradeHistoryView = ({ currentUser }) => {
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);

  useEffect(() => {
    loadHistory();
  }, []);

  const loadHistory = async (pageNum = 1, reset = true) => {
    setLoading(true);
    setError(null);

    try {
      const historyData = await apiService.getUpgradeHistory({
        userId: currentUser.id,
        page: pageNum,
        pageSize: 20
      });

      if (reset) {
        setHistory(historyData);
      } else {
        setHistory(prev => [...prev, ...historyData]);
      }

      setHasMore(historyData.length === 20);
      setPage(pageNum);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const loadMoreHistory = () => {
    if (!loading && hasMore) {
      loadHistory(page + 1, false);
    }
  };

  const getEventIcon = (eventType) => {
    switch (eventType) {
      case 'ReleaseScheduled':
        return { icon: FiCalendar, color: 'blue' };
      case 'PreferenceSet':
      case 'PreferenceUpdated':
        return { icon: FiSettings, color: 'orange' };
      case 'UpgradeExecuted':
        return { icon: FiCheckCircle, color: 'green' };
      case 'NotificationSent':
        return { icon: FiMail, color: 'purple' };
      case 'JobScheduled':
        return { icon: FiClock, color: 'gray' };
      default:
        return { icon: FiClock, color: 'gray' };
    }
  };

  const getEventTypeLabel = (eventType) => {
    switch (eventType) {
      case 'ReleaseScheduled':
        return 'Release Scheduled';
      case 'PreferenceSet':
        return 'Preference Set';
      case 'PreferenceUpdated':
        return 'Preference Updated';
      case 'UpgradeExecuted':
        return 'Upgrade Executed';
      case 'NotificationSent':
        return 'Notification Sent';
      case 'JobScheduled':
        return 'Job Scheduled';
      default:
        return eventType;
    }
  };

  const formatDateTime = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  if (loading && history.length === 0) {
    return (
      <Center py={8}>
        <VStack>
          <Spinner size="lg" color="blue.500" />
          <Text>Loading upgrade history...</Text>
        </VStack>
      </Center>
    );
  }

  return (
    <Card>
      <CardHeader>
        <HStack justify="space-between">
          <Heading size="md" color="blue.600">Upgrade History</Heading>
          <Button
            size="sm"
            variant="outline"
            onClick={() => loadHistory(1, true)}
            isLoading={loading}
          >
            Refresh
          </Button>
        </HStack>
      </CardHeader>
      <CardBody>
        {error && (
          <Alert status="error" mb={4}>
            <AlertIcon />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {history.length === 0 && !loading ? (
          <Alert status="info">
            <AlertIcon />
            <AlertDescription>
              No upgrade history found. History will appear here as you interact with the system.
            </AlertDescription>
          </Alert>
        ) : (
          <VStack spacing={4} align="stretch">
            {history.map((item, index) => {
              const { icon: IconComponent, color } = getEventIcon(item.eventType);
              
              return (
                <Box key={item.id} position="relative">
                  {/* Timeline line */}
                  {index < history.length - 1 && (
                    <Box
                      position="absolute"
                      left="20px"
                      top="50px"
                      bottom="-20px"
                      width="2px"
                      bg="gray.200"
                      zIndex={0}
                    />
                  )}
                  
                  <HStack align="start" spacing={4}>
                    {/* Event icon */}
                    <Box
                      bg={`${color}.100`}
                      border="2px solid"
                      borderColor={`${color}.300`}
                      borderRadius="full"
                      p={2}
                      zIndex={1}
                      position="relative"
                    >
                      <Icon as={IconComponent} color={`${color}.600`} boxSize={4} />
                    </Box>

                    {/* Event content */}
                    <Box flex={1} pt={1}>
                      <Flex justify="space-between" align="start" mb={2}>
                        <VStack align="start" spacing={1}>
                          <HStack>
                            <Badge colorScheme={color} fontSize="xs">
                              {getEventTypeLabel(item.eventType)}
                            </Badge>
                            {item.plannedRelease && (
                              <Badge variant="outline" fontSize="xs">
                                v{item.plannedRelease.version}
                              </Badge>
                            )}
                          </HStack>
                          <Text fontWeight="medium" fontSize="sm">
                            {item.description}
                          </Text>
                        </VStack>
                        <Text fontSize="xs" color="gray.500" flexShrink={0} ml={4}>
                          {formatDateTime(item.timestamp)}
                        </Text>
                      </Flex>

                      {/* Additional data accordion */}
                      {item.additionalData && (
                        <Accordion allowToggle size="sm">
                          <AccordionItem border="none">
                            <AccordionButton px={0} py={1} _hover={{ bg: 'gray.50' }}>
                              <Text fontSize="xs" color="blue.600">
                                View Details
                              </Text>
                              <AccordionIcon />
                            </AccordionButton>
                            <AccordionPanel px={0} py={2}>
                              <Box
                                bg="gray.50"
                                p={3}
                                borderRadius="md"
                                fontSize="xs"
                                fontFamily="mono"
                              >
                                <pre>{JSON.stringify(JSON.parse(item.additionalData), null, 2)}</pre>
                              </Box>
                            </AccordionPanel>
                          </AccordionItem>
                        </Accordion>
                      )}
                    </Box>
                  </HStack>
                </Box>
              );
            })}

            {/* Load more button */}
            {hasMore && (
              <Center pt={4}>
                <Button
                  variant="outline"
                  onClick={loadMoreHistory}
                  isLoading={loading}
                  loadingText="Loading..."
                  size="sm"
                >
                  Load More
                </Button>
              </Center>
            )}
          </VStack>
        )}
      </CardBody>
    </Card>
  );
};

export default UpgradeHistoryView;