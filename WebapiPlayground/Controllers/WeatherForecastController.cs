using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebapiPlayground.Controllers
{
    /* A controller is a group of actions
     * An action is a method on a controller which handles requests
     * Parameters on actions are bound to request data
     * Controllers in a web API are classes that derive from ControllerBase
     * In Web Api, actions use attribute routing
     * Placing a route on the controller or action makes it attribute-routed
     * Attribute routing maps actions to route templates
     * Route template ex. [Route("Home/About")], [Route("Home/About/{id?}")] id is a route parameter
    */

    [Authorize]
    [ApiController]
    [Route("[controller]")] // or [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /*
         * All httpverb templates are route templates and are preferred over just using [Route("")]
         * This action with httpverb attribute constrains matching to HTTP GET requests only
         * This is Attribute routing with Http verb attributes as opposed to conventional routing
         * You can't add [HttpGet] to another action or exception unless you change the route template like [HttpGet("nyc")]
         * Exception thrown will be AmbiguousMatchException: The request matched multiple endpoints.
         * https://localhost:7150/weatherforecast
         */
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("nyc")]    //https://localhost:7150/weatherforecast/nyc
        public IEnumerable<char> Get2() => "abc".ToCharArray();

        /* Route constraints
         * [HttpGet("int/{id:int}")] :int portion of the template constrains the id route values to strings that can be converted to an integer
         * https://localhost:7150/weatherforecast/nyc/abc won't match this route
         
         * [HttpGet("int/{id}")] If you remove :int
         * https://localhost:7150/weatherforecast/nyc/abc will match this route but will fail at model binding when converting abc to int
         * 
         * https://localhost:7150/weatherforecast/nyc/3 will work
         * Route constraints execute when a match has occurred to the incoming URL and the URL path is tokenized into route values.
         */
        [HttpGet("nyc/{id:int}")]
        public IEnumerable<char> Get3(int id) => id.ToString().ToCharArray();

        [HttpGet("r1")]
        [HttpGet("r1/ra")]
        [HttpGet("r1/ra/{id?}")]
        public IEnumerable<char> Get4(int? id) => id.HasValue ? id.Value.ToString().ToCharArray() : "empty".ToCharArray();        
    }
}