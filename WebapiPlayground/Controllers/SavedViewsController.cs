using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebapiPlayground.Models;

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

    // You can restrict access at the controller level or at the action level
    // If you Authorize at controller level, you can add [AllowAnonymous] to the action
    [Authorize]
    [ApiController]
    [Route("api/[controller]")] // or [Route("api/[controller]/[action]")]
    public class SavedViewsController : ControllerBase
    {
        private readonly ILogger<SavedViewsController> _logger;
        private ISavedViewsRepositiory svrepo;

        public SavedViewsController(ILogger<SavedViewsController> logger)
        {
            _logger = logger;
            svrepo = new SavedViewsRepositioryMock();
        }

        /* CRUD
        GetViews()  returns all or filtered if query string
        GetViews controller/{id}
        SaveView should create or update
        DeleteView

        return views
        if arg is null, what should be returned
        if return is null, or throws exceptions, what to do
        Saving, if arg null or saving returns issue, what to return?
        Deleting
        */

        // GET: api/SavedViews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Savedview>>> GetViews()
        {
            IEnumerable<Savedview> res = await svrepo.GetViews();
            return res.ToList();
        }

        // If you call this and don't put a value for query, query will not be null but the fields are null
        // https://localhost:7150/api/SavedViews/allorfilter
        // https://localhost:7150/api/SavedViews/allorfilter?userid=2
        [HttpGet("allorfilter")]
        public async Task<ActionResult<IEnumerable<Savedview>>> GetViews([FromQuery] LoadViewQuery query)
        {
            var auth = true;
            if (!auth)
            {
                return Forbid();
            }

            var views = await svrepo.GetViews(query);
            if (views == null)
            {
                return NotFound();
            }

            if (views.First().Id == 0)
            {
                //You can do a check for say bad user
                return BadRequest("Cannot delete bad user");

            }
            return Ok(views);
        }

        // GET api/<SavedViewsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // GET: api/<SavedViewsController>
        [HttpPost("saveview")]
        public async Task<ActionResult<Savedview>> SaveView([FromQuery] Savedview sv)
        {
            //return new string[] { "value1", "value2" };
            if (sv.Id == 0)
            {
                return BadRequest("Id cant be 0");
            }
            var res = await svrepo.SaveView(sv);
            return CreatedAtAction("GetViews", new { id = res.Id }, res);
        }

        [HttpGet("json")]
        public ActionResult<string> GetJson(Savedview sv)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonstr = JsonSerializer.Serialize(sv, options);
            return Ok(jsonstr);
        }

        // PUT api/<SavedViewsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SavedViewsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public interface ILoadViewQuery
    {
        int? UserId { get; set; }
        string? ViewName { get; set; }
        string? ViewType { get; set; }
    }

    public class LoadViewQuery : ILoadViewQuery
    {
        public int? UserId { get; set; }
        public string? ViewName { get; set; }
        public string? ViewType { get; set; }
    }

    public interface ISavedViewsRepositiory
    {
        Task<IEnumerable<Savedview>> GetViews(ILoadViewQuery? query = null);
        Task<Savedview> SaveView(Savedview savedview);
    }

    public class SavedViewsRepositiory : ISavedViewsRepositiory
    {
        public SavedViewsRepositiory()
        {
        }

        public async Task<IEnumerable<Savedview>> GetViews(ILoadViewQuery? query = null)
        {
            try
            {
                var con = new AlanContext();
                var res = await con.Savedviews.ToListAsync();
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Savedview> SaveView(Savedview savedview)
        {
            try
            {
                var con = new AlanContext();
                var sqlparams = new List<SqlParameter> {
                new SqlParameter(){ParameterName="ViewType", Value=savedview.Viewtype},
                new SqlParameter(){ParameterName="saveobj", Value=savedview.Saveobj },
                new SqlParameter() { ParameterName = "isglobal", Value = savedview.Isglobal }
            };

                string sql = "exec alan.dbo.proc_savedviews_upsertview @viewtype, @saveobj, @isglobal";
                var res = await con.Savedviews.FromSqlRaw(sql, sqlparams.ToArray()).ToListAsync();
                return res.First();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> TestView()
        {
            var con = new AlanContext();
            var sqlparams = new List<SqlParameter> {
                new SqlParameter(){ParameterName="retval", SqlDbType=System.Data.SqlDbType.Int, Direction=System.Data.ParameterDirection.Output}
            };
            //var res = await con.Savedviews($"execute alan.dbo.proc_savedviews_testview @retval output", sqlparams.ToArray()).ToListAsync();
            return "true";
        }
    }

    public class SavedViewsRepositioryMock : ISavedViewsRepositiory
    {
        List<Savedview> savedviews = new List<Savedview>();
        public SavedViewsRepositioryMock()
        {
            savedviews = new List<Savedview>
            {
                new Savedview { Id = 1, Saveobj = "save1" },
                new Savedview { Id = 2, Saveobj = "save2" },
                new Savedview { Id = 3, Saveobj = "save3" },
                new Savedview { Id = 4, Saveobj = "save4" }
            };
        }

        public Task<IEnumerable<Savedview>> GetViews(ILoadViewQuery? query = null)
        {
            //var result = new List<IEnumerable<Savedview>>();
            if (query != null)
            {
                var result = savedviews.Where(x =>
                    (query.UserId == null || x.Id == query.UserId) &&
                    (query.ViewType == null || x.Viewtype == query.ViewType)
                    );
                return Task.FromResult(result);
            }
            return Task.FromResult(savedviews as IEnumerable<Savedview>);
        }

        public Task<Savedview> SaveView(Savedview savedview)
        {
            if (savedview != null)
            {
                var maxId = savedviews.Max(x => x.Id);
                savedview.Id = maxId;
                savedviews.Add(savedview);
                return Task.FromResult(savedview);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
    }
}
