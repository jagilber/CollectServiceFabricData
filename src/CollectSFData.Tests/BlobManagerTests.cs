using System;
using Azure.Storage.Blobs;
using CollectSFData.Azure;
using CollectSFData.Common;
using FluentAssertions;
using Xunit;

namespace CollectSFData.Tests
{
    [Collection("TestAuditCollection")]
    public class BlobManagerTests
    {
        [Fact]
        public void CreateBlobClient_WithoutQuery_AppendsSasWithoutChangingBlobPath()
        {
            ConfigurationOptions config = ConfigurationOptions.Singleton();
            SasEndpoints originalEndpoint = config.SasEndpointInfo;

            try
            {
                config.SasEndpointInfo = new SasEndpoints
                {
                    SasToken = "?sv=2023-08-03&sp=r&sig=test-signature"
                };
                BlobManager blobManager = new BlobManager(new Instance());

                BlobClient client = blobManager.CreateBlobClient("https://account.blob.core.windows.net/container/blob.dtr.zip");

                client.Uri.AbsolutePath.Should().Be("/container/blob.dtr.zip");
                client.Uri.Query.Should().Be("?sv=2023-08-03&sp=r&sig=test-signature");
            }
            finally
            {
                config.SasEndpointInfo = originalEndpoint;
            }
        }

        [Fact]
        public void CreateBlobClient_WithQuery_PreservesExistingQuery()
        {
            ConfigurationOptions config = ConfigurationOptions.Singleton();
            SasEndpoints originalEndpoint = config.SasEndpointInfo;

            try
            {
                config.SasEndpointInfo = new SasEndpoints
                {
                    SasToken = "?sv=configured&sig=configured"
                };
                BlobManager blobManager = new BlobManager(new Instance());

                BlobClient client = blobManager.CreateBlobClient("https://account.blob.core.windows.net/container/blob.dtr.zip?sv=existing&sig=existing");

                client.Uri.AbsolutePath.Should().Be("/container/blob.dtr.zip");
                client.Uri.Query.Should().Be("?sv=existing&sig=existing");
            }
            finally
            {
                config.SasEndpointInfo = originalEndpoint;
            }
        }

        [Fact]
        public void Instance_DiscoveredTimeRange_RequiresValidBounds()
        {
            Instance instance = new Instance();
            DateTime minimum = new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc);
            DateTime maximum = minimum.AddHours(8);

            instance.HasDiscoveredTimeRange.Should().BeFalse();

            instance.SetMinMaxDate(minimum.Ticks, maximum.Ticks);

            instance.HasDiscoveredTimeRange.Should().BeTrue();
            instance.DiscoveredMinDateTicks.Should().Be(minimum.Ticks);
            instance.DiscoveredMaxDateTicks.Should().Be(maximum.Ticks);
        }
    }
}