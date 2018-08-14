using NLog;
using SensorDataCommon;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using SensorDataCommon.Models;
using SensorDataCommon.Attributes;
using SensorDataPublisher.Signalling;

namespace SensorDataPublisher.Controllers
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
