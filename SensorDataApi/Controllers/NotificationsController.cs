using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using SensorDataCommon.Data;
using SensorDataApi.Attributes;
using NLog;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using System.Data.Entity.Validation;
using Microsoft.AspNet.SignalR;

namespace SensorDataApi.Controllers
{
    public class NotificationsController : ApiController
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");        
        private SensorDataSqlEntities db = new SensorDataSqlEntities();

        // GET: api/Notification
        [ResponseType(typeof(IQueryable<Notification>))]
        public IHttpActionResult GetNotifications()
        {
            logger.Info($"GET: {Request.RequestUri} called");

            IEnumerable<KeyValuePair<string, string>> allUrlKeyValues = ControllerContext.Request.GetQueryNameValuePairs();
            var maxResults = allUrlKeyValues.FirstOrDefault(x => x.Key == "maxResults").Value;
            IQueryable<Notification> result;
            if (maxResults == null)
            {
                result = db.Notification.OrderByDescending(o => o.Id).Take(1000);
            }
            else
            {
                int number;
                if (!int.TryParse(maxResults, out number))
                {
                    return BadRequest("Querystring parameter 'maxResults' must have an integer value");
                }
                result = db.Notification.OrderByDescending(o => o.Id).Take(number);
            }

            foreach (var notification in result)
            {
                notification.Created = TimeZoneInfo.ConvertTimeFromUtc(notification.Created.Value, info);
            }

            logger.Info($"GET: {Request.RequestUri} finished");
            logger.Info($"{result.Count()} items retrieved");
            return Ok(result);
        }

        // GET: api/Notification/5
        [ResponseType(typeof(Notification))]
        public IHttpActionResult GetNotifications(int id)
        {
            logger.Info($"GET: {Request.RequestUri} called");
            Notification notifications = db.Notification.Find(id);
            if (notifications == null)
            {
                return NotFound();
            }

            return Ok(notifications);
        }

        // POST: api/Notification
        [ResponseType(typeof(Notification))]
        [BasicAuthenticationAttribute]
        public IHttpActionResult PostNotifications(Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                db.Notification.Add(notification);
                db.SaveChanges();

                var context = GlobalHost.ConnectionManager.GetHubContext<SensorDataHub>();
                notification.Created = DateTime.Now;
                context.Clients.All.notificationReceived(notification);

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var error in ex.EntityValidationErrors)
                {
                    foreach (var valError in error.ValidationErrors)
                    {
                        Debug.WriteLine(valError.ErrorMessage);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }



            return CreatedAtRoute("DefaultApi", new { id = notification.Id }, notification);
        }

        // DELETE: api/Notification/5
        [ResponseType(typeof(Notification))]
        public IHttpActionResult Delete(int id)
        {
            logger.Info($"DELETE: {Request.RequestUri} called");
            logger.Info("Deleting notification: Id={0}", id);
            Notification notifications = db.Notification.Find(id);
            if (notifications == null)
            {
                return NotFound();
            }

            db.Notification.Remove(notifications);
            db.SaveChanges();

            logger.Info("Notification deleted: Id={0}", id);
            return Ok(notifications);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool NotificationsExists(int id)
        {
            return db.Notification.Count(e => e.Id == id) > 0;
        }
    }
}