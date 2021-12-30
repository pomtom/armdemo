using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace uamiApp
{
    public static class testuami
    {
        public static BlobServiceClientClass client;


        [FunctionName("testuami")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = CallManageIdentity(log);
            //string responseMessage = CallConnectionString(log);

            return new OkObjectResult(responseMessage);
        }

        private static string CallManageIdentity(ILogger log)
        {
            client = new BlobServiceClientClass("btltrial", "surpassone", "b52f6e31-7c27-4546-84ae-4d33bae35d59");

            byte[] smallArray = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            return client.UploadBinary(smallArray, "magic.jpg", log);
        }

        private static string CallConnectionString(ILogger log)
        {
            client = new BlobServiceClientClass("UseDevelopmentStorage=true", "surpassone");
            byte[] smallArray = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            return client.UploadBinary(smallArray, "magic.jpg", log);
        }
    }

    public class BlobServiceClientClass
    {
        private string connectionString;
        private static BlobServiceClient blobServiceClient;
        private static BlobContainerClient containerClient;
        private static BlobClient blobClient;
        public BlobServiceClientClass(string accountName, string containerName, string userAssignedClientId)
        {
            // Construct the blob container endpoint from the arguments.
            string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
                                                        accountName,
                                                        containerName);

            // Get a credential and create a service client object for the blob container.
            containerClient = new BlobContainerClient
                                    (new Uri(containerEndpoint),
                                        new DefaultAzureCredential
                                            (new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId }));

        }

        public BlobServiceClientClass(string connectionString, string blobContainerName)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(blobContainerName))
            {
                throw new ArgumentOutOfRangeException("connectionString", "Invalid connection string was introduced!");
            }
            this.connectionString = connectionString;
            blobServiceClient = new BlobServiceClient(connectionString);
            containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            containerClient = CreateBlobContainerAsync(blobContainerName).GetAwaiter().GetResult();
        }

        private async Task<BlobContainerClient> CreateBlobContainerAsync(string containerName)
        {
            containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            if (!await containerClient.ExistsAsync())
            {
                await containerClient.CreateIfNotExistsAsync();
            }

            return containerClient;
        }
        public string UploadBinary(byte[] bytes, string fileName, ILogger log)
        {
            blobClient = containerClient.GetBlobClient(fileName);

            var blobHttpHeader = new BlobHttpHeaders();
            blobHttpHeader.ContentType = "image/jpg";


            try
            {
                if (!blobClient.Exists())
                {
                    using (Stream stream = new MemoryStream(bytes, false))
                    {
                        blobClient.Upload(stream, blobHttpHeader);
                    }
                }
                else
                {
                    log.LogInformation($"Blob {blobClient.Name} allready exist into {blobClient.BlobContainerName} container");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message.ToString());
                return ex.Message.ToString();
            }

            return blobClient.Uri.ToString();
        }
    }
}
