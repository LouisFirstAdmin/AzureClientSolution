using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureClientFunction
{
    public static class ClientAPI
    {
        [FunctionName("CreateClient")]
        public static async Task<IActionResult> CreateClient(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "client")]
           [Table("clients", Connection = "AzureWebJobsStorage")]
           IAsyncCollector<ClientTableEntity> clientTable,
           HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a new client entity.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ClientCreateModel>(requestBody);

            var client = new Client
            {
                Name = input.Name,
                ContactName = input.ContactName,
                ContactEmailAddress = input.ContactEmailAddress,
                ContactPhoneNumber = input.ContactPhoneNumber,
                CompanyVAT = input.CompanyVAT,
                Address = input.Address
            };

            await clientTable.AddAsync(client.ToTableEntity());

            return new OkObjectResult(client);
        }

        [FunctionName("GetAllClients")]
        public static async Task<IActionResult> GetClients(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "client")]
           [Table("clients", Connection = "AzureWebJobsStorage")]
           CloudTable clientTable,
           HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting all clients.");

            var query = new TableQuery<ClientTableEntity>();
            var segment = await clientTable.ExecuteQuerySegmentedAsync(query, null);

            return new OkObjectResult(segment.Select(Mappings.ToClient));
        }

        [FunctionName("GetClientByID")]
        public static async Task<IActionResult> GetClientByID(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "client/{id}")]
            [Table("client", "CLIENT", "{id}", Connection = "AzureWebJobsStorage")]
            ClientTableEntity client,
            HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Getting client by id.");

            if (client == null)
            {
                log.LogInformation($"Client {id} not found.");
                return new NotFoundResult();
            }

            return new OkObjectResult(client.ToClient());
        }

        [FunctionName("UpdateClient")]
        public static async Task<IActionResult> UpdateClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "client/{id}")]
            [Table("clients", Connection = "AzureWebJobsStorage")]
            CloudTable clientTable,
            HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Updating client.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ClientUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<ClientTableEntity>("CLIENT", id);
            var findResult = await clientTable.ExecuteAsync(findOperation);

            if (findResult == null)
                return new NotFoundResult();

            var existingRow = (ClientTableEntity)findResult.Result;

            if (!string.IsNullOrEmpty(updated.Name))
                existingRow.Name = updated.Name;
            if (!string.IsNullOrEmpty(updated.ContactName))
                existingRow.ContactName = updated.ContactName;
            if (!string.IsNullOrEmpty(updated.ContactEmailAddress))
                existingRow.ContactEmailAddress = updated.ContactEmailAddress;
            if (!string.IsNullOrEmpty(updated.ContactPhoneNumber))
                existingRow.ContactPhoneNumber = updated.ContactPhoneNumber;
            if (!string.IsNullOrEmpty(updated.CompanyVAT))
                existingRow.CompanyVAT = updated.CompanyVAT;
            if (!string.IsNullOrEmpty(updated.Address))
                existingRow.Address = updated.Address;

            var replaceOperation = TableOperation.Replace(existingRow);
            await clientTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToClient());
        }

        [FunctionName("DeleteClient")]
        public static async Task<IActionResult> DeleteClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "client/{id}")]
            [Table("clients", Connection = "AzureWebJobsStorage")]
            CloudTable clientTable,
            HttpRequest req, ILogger log, string id)
        {
            log.LogInformation("Deleting client.");

            var deleteOperation = TableOperation.Delete(new TableEntity()
            {
                PartitionKey = "CLIENT",
                RowKey = id,
                ETag = "*"
            });

            try
            {
                var deleteResult = await clientTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {

                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
