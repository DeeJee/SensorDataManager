using System;
using System.Data;
using System.Linq;
using System.Web.Http.Description;
using SensorDataApi.Attributes;
using NLog;
using System.Diagnostics;
using MySensorData.Common.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SensorDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class NotificationsController : ControllerBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        private SensorDataSqlContext db;
        private readonly IHubContext<SensorDataHub> hubContext;


        public NotificationsController(SensorDataSqlContext db, IHubContext<SensorDataHub> hubContext)
        {
            this.db = db;
            this.hubContext = hubContext;
        }

        // GET: api/Notification
        [HttpGet]
        public ActionResult GetNotifications()
        {
            logger.Info($"GET: {Request.Path} called");

            var maxResults = (string)Request.Query["maxResults"];
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
            //return Ok(new List<string>());
            foreach (var notification in result)
            {
                notification.Created = TimeZoneInfo.ConvertTimeFromUtc(notification.Created.Value, info);
            }

            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{result.Count()} items retrieved");
            return Ok(result);
        }

        // GET: api/Notification/5
        [HttpGet("{id}")]
        public ActionResult GetNotifications(int id)
        {
            logger.Info($"GET: {Request.Path} called");
            Notification notifications = db.Notification.Find(id);
            if (notifications == null)
            {
                return NotFound();
            }

            return Ok(notifications);
        }

        // POST: api/Notification
        //[BasicAuthentication]
        //[HttpPost("api/Notification")]
        //[Authorize(AuthenticationSchemes = "Basic")]
        public async Task<ActionResult> Post(Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var validationContext = new ValidationContext(notification);
                Validator.ValidateObject(notification, validationContext);
                db.Notification.Add(notification);
                db.SaveChanges();

                notification.Created = DateTime.Now;
                await hubContext.Clients.All.SendAsync("notification", notification);
            }
            catch (ValidationException ex)
            {
                Debug.WriteLine(ex);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            //return CreatedAtRoute("DefaultApi", new { id = notification.Id }, notification);
            return NoContent();
        }

        // DELETE: api/Notification/5
        [ResponseType(typeof(Notification))]
        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            logger.Info($"DELETE: {Request.Path} called");
            logger.Info("Deleting notification: Id={0}", id);
            Notification notification = db.Notification.Find(id);
            if (notification == null)
            {
                return NotFound();
            }

            db.Notification.Remove(notification);
            db.SaveChanges();

            logger.Info("Notification deleted: Id={0}", id);
            return Ok(notification);
        }

        private bool NotificationsExists(int id)
        {
            return db.Notification.Count(e => e.Id == id) > 0;
        }
    }
}