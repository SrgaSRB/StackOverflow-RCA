using Azure.Data.Tables;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class QuestionService
    {

        private readonly TableClient _tableClient;
        private readonly TableClient _userTableClient;
        private readonly TableClient _commentsTableClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly VoteService _voteService;

        public QuestionService(string connectionString, VoteService voteService)
        {
            _tableClient = new TableClient(connectionString, "Questions");
            _tableClient.CreateIfNotExists();
            _userTableClient = new TableClient(connectionString, "Users");
            _userTableClient.CreateIfNotExists();
            _commentsTableClient = new TableClient(connectionString, "Comments");
            _commentsTableClient.CreateIfNotExists();
            _blobContainerClient = new BlobContainerClient(connectionString, "question-pictures");
            _blobContainerClient.CreateIfNotExists();
            _voteService = voteService;
        }

    }
}