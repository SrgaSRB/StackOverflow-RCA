using Azure.Data.Tables;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class UserService
    {

        private readonly TableClient _tableClient;
        private readonly BlobContainerClient _blobContainerClient;

        public UserService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Users");
            _blobContainerClient = new BlobContainerClient(connectionString, "profile-pictures");
            _blobContainerClient.CreateIfNotExists();
        }

    }
}