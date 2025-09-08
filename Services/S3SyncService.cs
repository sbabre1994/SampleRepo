using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;

namespace UpgradeNotificationSystem.Services
{
    public interface IS3SyncService
    {
        Task SyncPlannedReleaseAsync(object data, string key);
        Task SyncUserPreferenceAsync(object data, string key);
        Task SyncUpgradeHistoryAsync(object data, string key);
    }

    public class S3SyncService : IS3SyncService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3SyncService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public S3SyncService(IAmazonS3 s3Client, ILogger<S3SyncService> logger, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _logger = logger;
            _configuration = configuration;
            _bucketName = _configuration["AWS:S3:BucketName"] ?? "upgrade-notification-backup";
        }

        public async Task SyncPlannedReleaseAsync(object data, string key)
        {
            await SyncToS3Async(data, $"planned-releases/{key}");
        }

        public async Task SyncUserPreferenceAsync(object data, string key)
        {
            await SyncToS3Async(data, $"user-preferences/{key}");
        }

        public async Task SyncUpgradeHistoryAsync(object data, string key)
        {
            await SyncToS3Async(data, $"upgrade-history/{key}");
        }

        private async Task SyncToS3Async(object data, string key)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    ContentBody = jsonData,
                    ContentType = "application/json",
                    Metadata = 
                    {
                        ["sync-timestamp"] = DateTime.UtcNow.ToString("O"),
                        ["data-type"] = data.GetType().Name
                    }
                };

                var response = await _s3Client.PutObjectAsync(request);
                
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Successfully synced data to S3: {Key}", key);
                }
                else
                {
                    _logger.LogWarning("S3 sync returned non-OK status {StatusCode} for key: {Key}", response.HttpStatusCode, key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync data to S3 for key: {Key}", key);
                // Don't rethrow - S3 sync should not break the main application flow
            }
        }
    }

    // Mock implementation for development/testing
    public class MockS3SyncService : IS3SyncService
    {
        private readonly ILogger<MockS3SyncService> _logger;

        public MockS3SyncService(ILogger<MockS3SyncService> logger)
        {
            _logger = logger;
        }

        public async Task SyncPlannedReleaseAsync(object data, string key)
        {
            await MockSyncAsync(data, $"planned-releases/{key}");
        }

        public async Task SyncUserPreferenceAsync(object data, string key)
        {
            await MockSyncAsync(data, $"user-preferences/{key}");
        }

        public async Task SyncUpgradeHistoryAsync(object data, string key)
        {
            await MockSyncAsync(data, $"upgrade-history/{key}");
        }

        private async Task MockSyncAsync(object data, string key)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                _logger.LogInformation("Mock S3 sync for key {Key}: {Data}", key, jsonData);
                
                // Simulate async operation
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mock sync data for key: {Key}", key);
            }
        }
    }
}