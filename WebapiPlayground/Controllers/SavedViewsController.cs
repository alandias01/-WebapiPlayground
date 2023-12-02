using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebapiPlayground.Models;

namespace WebapiPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedViewsController : ControllerBase
    {
        private SavedViewsRepositiory svrepo;
        public SavedViewsController()
        {
            svrepo = new SavedViewsRepositiory();
        }

        // GET: api/<SavedViewsController>
        [HttpGet]
        public async Task<IEnumerable<Savedview>> Get()
        {
            //return new string[] { "value1", "value2" };
            var res = await svrepo.GetViews();
            return res;
        }

        public async Task<ActionResult<IEnumerable<Savedview>>> GetViews([FromQuery] LoadViewQuery query)
        {
            var auth = true;
            if (!auth)
            {
                return Forbid();
            }

            var views = await svrepo.GetViews(query);
            if(views == null)
            {
                return NotFound();
            }

            if(views.First().Id == 0)
            {
                //You can do a check for say bad user
                return BadRequest("Cannot delete bad user");

            }
            return Ok(views);
        }

        // GET: api/<SavedViewsController>
        [HttpGet("save")]
        public async Task<ActionResult<IEnumerable<Savedview>>> sv([FromQuery] Savedview sv)
        {
            //return new string[] { "value1", "value2" };
            if (sv.Id == 0)
            {
                return BadRequest("Id cant be 0");
            }
            var res = await svrepo.SaveView(sv);
            return CreatedAtAction("GetViews", new { id = res.First().Id }, res);
        }

        public ActionResult<string> GetJson(Savedview sv)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonstr = JsonSerializer.Serialize(sv, options);
            return Ok(jsonstr);
        }

        [HttpGet("test")]
        public async Task<string> test()
        {

            var res = await svrepo.TestView();
            return res;
        }

        // GET api/<SavedViewsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<SavedViewsController>
        [HttpPost]
        public void Post([FromQuery] string viewtype, string saveobj, bool isglobal)
        {

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
        string UserId { get; set; }
        string ViewName { get; set; }
        string ViewType { get; set; }
    }

    public class LoadViewQuery : ILoadViewQuery
    {
        public string UserId { get; set; }
        public string ViewName { get; set; }
        public string ViewType { get; set; }
    }

    public class SavedViewsRepositiory
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

        public async Task<IEnumerable<Savedview>> SaveView(Savedview savedview)
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
                return res;
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
}
