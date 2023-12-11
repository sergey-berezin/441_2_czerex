using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NuGetAnswering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

//using WebAnswering.Pages;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAnsweringServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TextController : ControllerBase
    {
        public static Dictionary<string, string> Texts;
        private AnsweringComponent answerTask;

        public TextController(AnsweringComponent answerTask, Dictionary<string, string> TextsTemp)
        {
            this.answerTask = answerTask;
            Texts = TextsTemp;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string textId, string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                var message = "Empty question";
                return BadRequest(message);
            }
            var answer = await answerTask.GetAnswerAsync(Texts[textId], question);
            return Ok(answer.ToString());
        }

        [HttpPost]
        public ActionResult<string> PostText([FromBody] string text)
        {
            if (text.Length == 0)
            {
                var message = "Empty text";
                return BadRequest(message);
            }
            var textId = GenerateUniqueId(text);
            Texts[textId] = text;
            return Ok(textId);
        }

        private string GenerateUniqueId(string text)
        {
            return text.GetHashCode().ToString();
        }
        
    }
}
