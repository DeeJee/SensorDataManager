using NLog;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Linq;
using System.Web.Http.Description;
using System.Threading.Tasks;
using System.Collections.Generic;
using SensorData.Api.Data;
using SensorData.Api.Models;

namespace SensorDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[AllowAnonymous]

#if DEBUG
#else
[Authorize]
#endif
    public class SensorDataController : ControllerBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IHubContext<SensorDataHub> hubContext;
        private readonly ISensorDataRepository sensorData;
        private readonly IDataSourceRepository dataSource;

        public SensorDataController(IHubContext<SensorDataHub> hubContext,
            ISensorDataRepository sensorData,
            IDataSourceRepository dataSource)
        {
            this.hubContext = hubContext;
            this.sensorData = sensorData;
            this.dataSource = dataSource;
        }

        // GET: api/SensorData
        //[TokenValidation]
        [HttpGet]
        public IEnumerable<SensorDataModel> Get()
        {
            Stopwatch stopwatch = new Stopwatch();
            logger.Info($"GET: {Request.Path} called");
            stopwatch.Start();
            var data = sensorData.Get();
            stopwatch.Stop();
            logger.Info($"GET: {Request.Path} took {stopwatch.ElapsedMilliseconds} ms");
            return data;
        }

        // GET: api/SensorData/5
        [ResponseType(typeof(SensorDataModel))]
        [System.Web.Http.HttpGet]
        [Route("{deviceId}")]
        public IQueryable<SensorDataModel> Get(string deviceId)
        {
            logger.Info($"GET: {Request.Path} called");

            Stopwatch stopwatch = Stopwatch.StartNew();

            string vanDatum = Request.Query["van"];
            string totDatum = Request.Query["tot"];

            var results = sensorData.Get(deviceId, vanDatum, totDatum);
            stopwatch.Stop();
            logger.Info($"GET: {Request.Path} finished in {stopwatch.ElapsedMilliseconds} ms");
            return results.AsQueryable();
        }


        // GET: api/SensorData/5
        [Route("{id}/MostRecent")]
        [HttpGet]
        public ActionResult MostRecent(string id)
        {
            var data = sensorData.MostRecent(id);
            if (data == null) return NotFound();

            logger.Info($"GET: {Request.Path} finished");
            return Ok(data);

        }

        // GET: api/SensorData/5        
        [ResponseType(typeof(SensorDataModel))]
        [HttpGet]
        [Route("MostRecent")]
        public ActionResult MostRecent()
        {
            var data = sensorData.MostRecent();

            logger.Info($"GET: {Request.Path} finished");
            return Ok(data.OrderByDescending(o => o.TimeStamp));
        }

        [ResponseType(typeof(SensorDataModel))]
        [Route("{id}/count")]
        [HttpGet]
        public ActionResult Count(string id)
        {
            logger.Info($"GET: {Request.Path} called");
            var data = sensorData.Count(id);
            logger.Info($"GET: {Request.Path} finished");
            return Ok(data);
        }

        //// PUT: api/SensorData/5
        //[ResponseType(typeof(void))]
        //[HttpPut]
        //public ActionResult PutSensorData(int id, MySensorData.Common.Data.SensorData sensorData)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != sensorData.Id)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(sensorData).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!SensorDataExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}


        // DELETE: api/SensorData/5
        //[Route("{deviceId}")]
        [HttpDelete("{deviceId}")]
        public ActionResult DeleteSensorData(string deviceId)
        {
            sensorData.DeleteSensorData(deviceId);

            return Ok();
        }

        // POST: api/SensorData
        [HttpPost]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public async Task<ActionResult> PostSensorData(SensorData.Api.Models.SensorDataModel dataModel)
        {
            logger.Info($"POST: {Request.Path} called");
            if (!ModelState.IsValid)
            {
                logger.Info("Bad request");
                return BadRequest(ModelState);
            }

            var ds = dataSource.GetDataSource(dataModel.DeviceId);
            if (ds == null)
            {
                dataSource.Add(dataModel.DeviceId);
            }
            sensorData.PostSensorData(dataModel);

            dataModel.TimeStamp = DateTime.Now;
            await hubContext.Clients.All.SendAsync("SensorData", dataModel);

            logger.Info("Dataset added for sensor ", dataModel.DeviceId);
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

        //private bool SensorDataExists(int id)
        //{
        //    return db.SensorData.Count(e => e.Id == id) > 0;
        //}
    }
}
