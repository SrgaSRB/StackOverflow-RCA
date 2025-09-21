using StackOverflowServiceWeb.Services;
using System.Web.Http;

namespace StackOverflowServiceWeb.Controllers
{
    public class QuestionsController : ApiController
    {
        private readonly QuestionService _questions;

        public QuestionsController(QuestionService questions)
        {
            _questions = questions;
        }

        [HttpGet, Route("api/questions")]
        public IHttpActionResult GetAll()
        {
            return Ok();
        }
    }
}