using NLog;
using SensorDataApi.Attributes;
//using SensorDataApi.Data;
using SensorDataCommon.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.SignalR;
using EntityFramework.Extensions;
using SensorDataCommon.Data;
using SensorDataApi.Security;

namespace SensorDataApi.Controllers
{
    public class SensorDataController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private SensorDataSqlEntities db;
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;
        public SensorDataController()
        {
            try
            {
                db = new SensorDataSqlEntities();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            //db.Database.Log = s => Debug.WriteLine(s);
        }

        // GET: api/SensorData
        [TokenValidation]
        public IQueryable<SensorData> Get()
        {
            Stopwatch stopwatch = new Stopwatch();
            logger.Info($"GET: {Request.RequestUri} called");
            stopwatch.Start();
            var data = db.SensorData.Take(1000);
            stopwatch.Stop();
            logger.Info($"GET: {Request.RequestUri} took {stopwatch.ElapsedMilliseconds} ms");
            return data;
        }

        // GET: api/SensorData/5
        [ResponseType(typeof(SensorData))]
        [HttpGet]
        [Route("api/SensorData/{deviceId}")]    
        public IQueryable<SensorData> Get(string deviceId)
        {
            logger.Info($"GET: {Request.RequestUri} called");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            db.Database.Log = s => Debug.WriteLine(s);
            var query = db.SensorData.Where(w => w.DeviceId == deviceId);

            IEnumerable<KeyValuePair<string, string>> allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs();
            string vanDatum = allUrlKeyValues.FirstOrDefault(x => x.Key == "van").Value;
            string totDatum = allUrlKeyValues.FirstOrDefault(x => x.Key == "tot").Value;

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
            logger.Info($"GET: {Request.RequestUri} finished in {stopwatch.ElapsedMilliseconds} ms");
            return results.AsQueryable();
        }

        // GET: api/SensorData/5
        [ResponseType(typeof(SensorData))]
        [Route("api/SensorData/{id}/MostRecent")]
        [HttpGet]
        public IHttpActionResult MostRecent(string id)
        {
            var data = db.SensorData.Where(w => w.DeviceId == id).OrderByDescending(o => o.Id).Take(1).SingleOrDefault();
            if (data != null)
            {
                data.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(data.TimeStamp, info);
                logger.Info($"GET: {Request.RequestUri} finished");
                return Ok(data);
            }
            return NotFound();
        }

        [ResponseType(typeof(SensorData))]
        [Route("api/SensorData/{id}/count")]
        [HttpGet]
        public IHttpActionResult Count(string id)
        {
            logger.Info($"GET: {Request.RequestUri} called");
            var data = db.SensorData.Count(w => w.DeviceId == id);
            logger.Info($"GET: {Request.RequestUri} finished");
            return Ok(data);
        }

        //// GET: api/SensorData/5
        //[ResponseType(typeof(SensorData))]
        //public IQueryable<SensorData> Get(string dataSource, string van, string tot)
        //{
        //    logger.Info($"GET: {Request.RequestUri} called");
        //    var vanDateTime = DateTime.ParseExact(van, "yyyyMMdd", CultureInfo.CurrentCulture);
        //    var totDateTime = DateTime.ParseExact(tot, "yyyyMMdd", CultureInfo.CurrentCulture);

        //    var results = db.SensorData.Where(w => w.DeviceId == dataSource && w.TimeStamp >= vanDateTime && w.TimeStamp <= totDateTime);

        //    logger.Info($"GET: {Request.RequestUri} finished");
        //    logger.Info($"{results.Count()} items retrieved");
        //    return results;
        //}

        // PUT: api/SensorData/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSensorData(int id, SensorData sensorData)
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

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/SensorData
        [BasicAuthenticationAttribute]
        public IHttpActionResult PostSensorData(DataModel sensorData)
        {
            logger.Info($"POST: {Request.RequestUri} called");
            if (!ModelState.IsValid)
            {
                logger.Info("Bad request");
                return BadRequest(ModelState);
            }

            db.SensorData.Add(new SensorData
            {
                DeviceId = sensorData.DeviceId,
                TimeStamp = DateTime.Now,
                Payload = sensorData.Payload.ToString()
            });
            db.SaveChanges();

            try
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<SensorDataHub>();
                sensorData.TimeStamp = DateTime.Now;
                context.Clients.All.SensorDataReceived(sensorData);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            logger.Info("Dataset added for sensor ", sensorData.DeviceId);
            return Ok();
        }

        // DELETE: api/SensorData/5
        [ResponseType(typeof(SensorData))]
        [Route("api/SensorData/{deviceId}/{datasetId}")]
        public IHttpActionResult DeleteSensorData(int deviceId, int datasetId)
        {
            SensorData sensorData = db.SensorData.Find(datasetId);
            if (sensorData == null)
            {
                return NotFound();
            }

            db.SensorData.Remove(sensorData);
            db.SaveChanges();

            return Ok(sensorData);
        }

        // DELETE: api/SensorData/5
        [Route("api/SensorData/{deviceId}")]
        public IHttpActionResult DeleteSensorData(string deviceId)
        {
            //TODO: omschrijven naar efficiente code
            //db.SensorData.Where(w => w.DeviceId == deviceId).Delete();
            var toBeDeleted = db.SensorData.Where(w => w.DeviceId == deviceId);
            db.SensorData.RemoveRange(toBeDeleted);

            //db.SensorData.RemoveRange(toBeDeleted);
            db.SaveChanges();

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SensorDataExists(int id)
        {
            return db.SensorData.Count(e => e.Id == id) > 0;
        }


    }
}
