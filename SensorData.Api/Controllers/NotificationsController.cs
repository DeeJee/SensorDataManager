using System;
using System.Linq;
using System.Web.Http.Description;
using NLog;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using SensorData.Api.Data;
using SensorData.Api.Models;
using Microsoft.Azure.KeyVault.Models;
using System.Web.Http.Results;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SensorDataApi.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
#if DEBUG
#else
[Authorize]
#endif
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationsController : ControllerBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        private readonly IHubContext<SensorDataHub> hubContext;
        private INotificationRepository notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository, IHubContext<SensorDataHub> hubContext)
        {
            this.notificationRepository = notificationRepository;
            this.hubContext = hubContext;
        }

        // GET: api/Notification
        [HttpGet]
        [Route("api/notifications")]
        public ActionResult GetNotifications()
        {
            logger.Info($"GET: {Request.Path} called");

            var maxResults = (string)Request.Query["maxResults"];
            int number;
            if (maxResults == null)
            {
                number = 1000;
            }
            else
            {
                if (!int.TryParse(maxResults, out number))
                {
                    return BadRequest("Querystring parameter 'maxResults' must have an integer value");
                }
            }

            var result = notificationRepository.GetNotifications(number);
            //return Ok(new List<string>());
            foreach (var notification in result)
            {
                //   notification.Created = TimeZoneInfo.ConvertTimeFromUtc(notification.Created.Value, info);
            }

            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{result.Count()} items retrieved");
            return Ok(result);
        }

        // GET: api/Notification/5
        [HttpGet("{id}")]
        [Route("api/notifications/{id}")]
        public ActionResult GetNotifications(string id)
        {
            logger.Info($"GET: {Request.Path} called");
            NotificationModel notification = notificationRepository.GetById(id);
            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification);
        }

        // POST: api/Notification
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        [HttpPost]
        [Route("api/notifications")]
        public async Task<ActionResult> Post(NotificationModel notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                notificationRepository.AddNotification(notification);

                notification.Created = DateTime.Now;
                await hubContext.Clients.All.SendAsync("notification", notification);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                throw new Exception("Fout bij opslaan notification");
            }

            //return CreatedAtRoute("DefaultApi", new { id = notification.Id }, notification);
            return NoContent();
        }

        // DELETE: api/Notification/5
        [ResponseType(typeof(NotificationModel))]
        [Authorize]
        [HttpDelete]
        [Route("api/notifications/{id}&{deviceId}")]
        public ActionResult Delete(string id, string deviceId)
        {
            logger.Info($"DELETE: {Request.Path} called");
            logger.Info("Deleting notification: Id={0}", id);
            notificationRepository.Delete(id, deviceId);

            logger.Info("Notification deleted: Id={0}", id);
            return NoContent();
        }
    }
}