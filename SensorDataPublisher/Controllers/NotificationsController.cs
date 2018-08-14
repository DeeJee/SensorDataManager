using System;
using System.Web.Http;
using System.Web.Http.Description;
using SensorDataCommon.Data;
using SensorDataCommon.Attributes;
using NLog;
using System.Diagnostics;
using System.Data.Entity.Validation;
using Microsoft.AspNet.SignalR;
using SensorDataPublisher.Signalling;

namespace SensorDataPublisher.Controllers
{
    public class NotificationsController : ApiController
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");        
        private SensorDataSqlEntities db = new SensorDataSqlEntities();

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}