using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowServiceWeb.Services
{
    public class CommentService
    {
        private readonly TableClient _tableClient;
        private readonly UserService _userService;
        private readonly VoteService _voteService;

        public CommentService(string connectionString, UserService userService, VoteService voteService)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("Comments");
            _tableClient.CreateIfNotExists();
            _userService = userService;
            _voteService = voteService;
        }
    }
}