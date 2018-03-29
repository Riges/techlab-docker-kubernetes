using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace lab_survey_front.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly ConnectionMultiplexer redis;
        private readonly string redisServer;

        public ApiController(IConfiguration configuration)
        {
            redisServer = $"{ configuration["REDIS"] }:6379";
            redis = ConnectionMultiplexer.Connect(redisServer);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SurveyResult surveyResult)
        {
            IDatabase db = redis.GetDatabase();

            db.StringSet($"survey:{surveyResult.Id}", JsonConvert.SerializeObject(surveyResult));

            return Ok(surveyResult);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetSurveyResuts()
        {
            IDatabase db = redis.GetDatabase();
            var server = redis.GetServer(redisServer);

            var keys = server.Keys(pattern: "survey:*").ToArray();

            var values = await db.StringGetAsync(keys);
            var surveyResults = values.Select(v => JsonConvert.DeserializeObject<SurveyResult>(v));
           
            return Json(surveyResults);
        }
    }

    public class SurveyResult
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

         [JsonProperty("completedSteps")]
        public Dictionary<int, DateTimeOffset> CompletedSteps { get; set; }
    }
}