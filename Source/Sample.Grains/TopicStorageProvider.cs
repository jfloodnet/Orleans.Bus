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
    public class TopicStorageProvider : StorageProvider<int, int, int>
    {
        CloudBlobContainer container;

        public override Task Init(Dictionary<string, string> properties)
        {
            var account = CloudStorageAccount.Parse(properties["StorageAccountConnectionString"]);
            var blobClient = account.CreateCloudBlobClient();

            container = blobClient.GetContainerReference("topics");
            return container.CreateIfNotExistsAsync();
        }

        public override async Task<int> ReadStateAsync(string id, GrainType type)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(type, id));
            if (!(await blob.ExistsAsync()))
                return 0;

            var contents = await blob.DownloadTextAsync();
            if (string.IsNullOrWhiteSpace(contents))
                return 0;

            return int.Parse(contents);
        }

        public override Task WriteStateAsync(string id, GrainType type, int total)
        {
            var blob = container.GetBlockBlobReference(GetBlobName(type, id));
            return blob.UploadTextAsync(total.ToString(CultureInfo.InvariantCulture));
        }

        public override Task ClearStateAsync(string id, GrainType type, int total)
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
