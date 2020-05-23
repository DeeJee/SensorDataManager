using NLog;
using SensorDataCommon.Models;
using System;
using System.Globalization;
using MySensorData.Common.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Linq;
using System.Web.Http.Description;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SensorDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SensorDataController : ControllerBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SensorDataSqlContext db;
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;
        private readonly IHubContext<SensorDataHub> hubContext;

        public SensorDataController(SensorDataSqlContext db, IHubContext<SensorDataHub> hubContext)
        {
            this.db = db;
            this.hubContext = hubContext;
        }

        // GET: api/SensorData
        //[TokenValidation]
        [HttpGet]
        public IQueryable<MySensorData.Common.Data.SensorData> Get()
        {
            Stopwatch stopwatch = new Stopwatch();
            logger.Info($"GET: {Request.Path} called");
            stopwatch.Start();
            var data = db.SensorData.Take(1000);
            stopwatch.Stop();
            logger.Info($"GET: {Request.Path} took {stopwatch.ElapsedMilliseconds} ms");
            return data;
        }

        // GET: api/SensorData/5
        [ResponseType(typeof(MySensorData.Common.Data.SensorData))]
        [System.Web.Http.HttpGet]
        [Route("{deviceId}")]
        public IQueryable<MySensorData.Common.Data.SensorData> Get(string deviceId)
        {
            logger.Info($"GET: {Request.Path} called");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var query = db.SensorData.Where(w => w.DeviceId == deviceId);

            string vanDatum = Request.Query["van"];
            string totDatum = Request.Query["tot"];

            DateTime vanDateTime;
            if (string.IsNullOrEmpty(vanDatum))
            {
                vanDateTime = DateTime.Now.Date.ToUniversalTime();

                query = query.Where(w => w.TimeStamp >= vanDateTime);
            }
            else
            {
                vanDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(vanDatum, "yyyy-MM-dd", nederland), info);
                //results=results.Where(w => DbFunctions.TruncateTime(w.TimeStamp) >= vanDateTime);
                query = query.Where(w => w.TimeStamp >= vanDateTime);
            }

            DateTime totDateTime;
            if (string.IsNullOrEmpty(totDatum))
            {
                var morgen = DateTime.Now.Date.AddDays(1).ToUniversalTime();
                query = query.Where(w => w.TimeStamp < morgen);
            }
            else
            {
                totDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(totDatum, "yyyy-MM-dd", nederland), info);
                //results=results.Where(w => DbFunctions.TruncateTime(w.TimeStamp) <= totDateTime);
                query = query.Where(w => w.TimeStamp <= totDateTime);
            }

            //execute query
            var results = query.ToList();

            foreach (var result in results)
            {
                result.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(result.TimeStamp, info);
            }

            logger.Info($"{results.Count()} items retrieved");
            stopwatch.Stop();
            logger.Info($"GET: {Request.Path} finished in {stopwatch.ElapsedMilliseconds} ms");
            return results.AsQueryable();
        }


        // GET: api/SensorData/5
        [ResponseType(typeof(MySensorData.Common.Data.SensorData))]
        [Route("{id}/MostRecent")]
        [HttpGet]
        public ActionResult MostRecent(string id)
        {
            var data = db.SensorData.Where(w => w.DeviceId == id).OrderByDescending(o => o.Id).Take(1).SingleOrDefault();
            if (data != null)
            {
                data.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(data.TimeStamp, info);
                logger.Info($"GET: {Request.Path} finished");
                return Ok(data);
            }
            return NotFound();
        }

        [ResponseType(typeof(MySensorData.Common.Data.SensorData))]
        [Route("{id}/count")]
        [HttpGet]
        public ActionResult Count(string id)
        {
            logger.Info($"GET: {Request.Path} called");
            var data = db.SensorData.Count(w => w.DeviceId == id);
            logger.Info($"GET: {Request.Path} finished");
            return Ok(data);
        }

        // PUT: api/SensorData/5
        [ResponseType(typeof(void))]
        [HttpPut]
        public ActionResult PutSensorData(int id, MySensorData.Common.Data.SensorData sensorData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sensorData.Id)
            {
                return BadRequest();
            }

            db.Entry(sensorData).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SensorDataExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/SensorData/5
        //[Route("{deviceId}")]
        [HttpDelete("{deviceId}")]
        public ActionResult DeleteSensorData(string deviceId)
        {
            //TODO: omschrijven naar efficiente code
            //db.SensorData.Where(w => w.DeviceId == deviceId).Delete();
            var toBeDeleted = db.SensorData.Where(w => w.DeviceId == deviceId);
            db.SensorData.RemoveRange(toBeDeleted);

            //db.SensorData.RemoveRange(toBeDeleted);
            db.SaveChanges();

            return Ok();
        }

        // POST: api/SensorData
        [HttpPost]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<ActionResult> PostSensorData(DataModel sensorData)
        {
            logger.Info($"POST: {Request.Path} called");
            if (!ModelState.IsValid)
            {
                logger.Info("Bad request");
                return BadRequest(ModelState);
            }

            db.SensorData.Add(new MySensorData.Common.Data.SensorData
            {
                DeviceId = sensorData.DeviceId,
                TimeStamp = DateTime.Now,
                Payload = sensorData.Payload.ToString()
            });
            db.SaveChanges();

            sensorData.TimeStamp = DateTime.Now;
            await hubContext.Clients.All.SendAsync("SensorData", sensorData);

            logger.Info("Dataset added for sensor ", sensorData.DeviceId);
            return NoContent();
        }

        //// GET: api/SensorData/5
        //[ResponseType(typeof(SensorData))]
        //public IQueryable<SensorData> Get(string dataSource, string van, string tot)
        //{
        //    logger.Info($"GET: {Request.Path} called");
        //    var vanDateTime = DateTime.ParseExact(van, "yyyyMMdd", CultureInfo.CurrentCulture);
        //    var totDateTime = DateTime.ParseExact(tot, "yyyyMMdd", CultureInfo.CurrentCulture);

        //    var results = db.SensorData.Where(w => w.DeviceId == dataSource && w.TimeStamp >= vanDateTime && w.TimeStamp <= totDateTime);

        //    logger.Info($"GET: {Request.Path} finished");
        //    logger.Info($"{results.Count()} items retrieved");
        //    return results;
        //}

        private bool SensorDataExists(int id)
        {
            return db.SensorData.Count(e => e.Id == id) > 0;
        }
    }
}
