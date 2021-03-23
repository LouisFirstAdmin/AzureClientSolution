using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureClientFunction
{
    public class Client
    {
        public string ID { get; set; } = Guid.NewGuid().ToString("n");
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmailAddress { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string CompanyVAT { get; set; }
        public string Address { get; set; }
    }

    public class ClientCreateModel
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmailAddress { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string CompanyVAT { get; set; }
        public string Address { get; set; }
    }

    public class ClientUpdateModel
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmailAddress { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string CompanyVAT { get; set; }
        public string Address { get; set; }
    }

    public class ClientTableEntity : TableEntity
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmailAddress { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string CompanyVAT { get; set; }
        public string Address { get; set; }
    }

    public static class Mappings
    {
        public static ClientTableEntity ToTableEntity(this Client client)
        {
            return new ClientTableEntity
            {
                PartitionKey = "CLIENT",
                RowKey = client.ID,
                Name = client.Name,
                ContactName = client.ContactName,
                ContactEmailAddress = client.ContactEmailAddress,
                ContactPhoneNumber = client.ContactPhoneNumber,
                CompanyVAT = client.CompanyVAT,
                Address = client.Address
            };
        }

        public static Client ToClient(this ClientTableEntity client)
        {
            return new Client
            {
                ID = client.RowKey,
                Name = client.Name,
                ContactName = client.ContactName,
                ContactEmailAddress = client.ContactEmailAddress,
                ContactPhoneNumber = client.ContactPhoneNumber,
                CompanyVAT = client.CompanyVAT,
                Address = client.Address
            };
        }
    }
}
