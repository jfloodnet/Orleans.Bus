using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using Orleans.Bus;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Sample
{
    /// <summary>
    /// Based on: https://github.com/OrleansContrib/OrleansBlobStorageProvider
    /// </summary>
    public class TopicStorageProvider : StorageProvider<TopicState>
    {
        CloudBlobContainer container;

        public override Task Init(IDictionary<string, string> properties)
        {
            var account = CloudStorageAccount.Parse(properties["StorageAccountConnectionString"]);
            var blobClient = account.CreateCloudBlobClient();

            container = blobClient.GetContainerReference("topics");
            return container.CreateIfNotExistsAsync();
        }

        public override async Task ReadStateAsync(string id, GrainType type, TopicState state)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(type, id));
            if (!(await blob.ExistsAsync()))
                return;

            var contents = await blob.DownloadTextAsync();
            if (string.IsNullOrWhiteSpace(contents))
                return;

            state.Total = int.Parse(contents);
        }

        public override Task WriteStateAsync(string id, GrainType type, TopicState state)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(type, id));
            return blob.UploadTextAsync(state.Total.ToString(CultureInfo.InvariantCulture));
        }

        public override Task ClearStateAsync(string id, GrainType type, TopicState state)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(type, id));
            return blob.DeleteIfExistsAsync();
        }

        static string GetBlobName(GrainType type, string id)
        {
            return string.Format("{0}-{1}.json", type.Name, id);
        }
    }
}
