using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class VoteService
    {

        private readonly TableClient _tableClient;
        private readonly TableClient _questionTableClient;
        private readonly TableClient _commentTableClient;

        public VoteService(string connectionString)
        {
            _tableClient = new TableClient(connectionString, "Votes");
            _tableClient.CreateIfNotExists();
            _questionTableClient = new TableClient(connectionString, "Questions");
            _questionTableClient.CreateIfNotExists();
            _commentTableClient = new TableClient(connectionString, "Comments");
            _commentTableClient.CreateIfNotExists();
        }

    }
}