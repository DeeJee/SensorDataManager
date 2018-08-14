using AutoMapper;
using NLog;
using SensorDataCommon.Data;
//using SensorDataApi.Data;
using SensorDataApi.infrastructure;
using SensorDataCommon.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace SensorDataApi.Controllers
{
    public class DataSourceController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SensorDataSqlEntities db = new SensorDataSqlEntities();
        private IMapper mapper = AutomapperConfig.CreateMapper();

        [HttpGet]
        [Route("api/DataSource/NewDatasources")]
        public NewDatasource[] NewDatasources()
        {
            //var query = db.SensorData
            //.Where(w => !db.DataSource.Any(a => a.DeviceId == w.DeviceId))
            //.Select(s => s.DeviceId).Distinct().ToArray();

            var query = from n in db.SensorData
                        where !db.DataSource.Any(a=>a.DeviceId==n.DeviceId)
            group n by n.DeviceId into g
            select new NewDatasource{ DeviceId = g.Key, TimeStamp = g.Max(t => t.TimeStamp) , Count=g.Count()};

            return query.ToArray();
            
            //return query.Select(deviceId => new DataSource
            //{
            //    DeviceId = deviceId
            //}).ToArray();
        }

        // GET: api/DataSource
        [HttpGet]
        public IQueryable<Datasource> GetDataSource()
        {
            logger.Info($"GET: {Request.RequestUri} called");
            var qsp = Request.GetQueryNameValuePairs();
            string channel = qsp.LastOrDefault(x => x.Key == "channel").Value;

            IQueryable<DataSource> query = db.DataSource;
            if (!string.IsNullOrEmpty(channel))
            {
                int id;
                if (int.TryParse(channel, out id))
                {
                    query = query.Where(w => w.DataTypeId == id).AsQueryable();
                }
            }

            logger.Info($"GET: {Request.RequestUri} finished");
            logger.Info($"{query.Count()} items retrieved");

            List< Datasource> results = new List<Datasource>();
            foreach (var item in query)
            {
                var ds = mapper.Map<Datasource>(item);
                results.Add(ds);
            }
            return results.AsQueryable();
        }

        // GET: api/DataSource/5
        [HttpGet]
        [Route("api/DataSource/{id}")]
        public IQueryable<Datasource> GetDataSourceById(string id)
        {
            logger.Info($"GET: {Request.RequestUri} called");
            
            IQueryable<DataSource> query = db.DataSource;
                    query = query.Where(w => w.DeviceId == id).AsQueryable();

            logger.Info($"GET: {Request.RequestUri} finished");
            logger.Info($"{query.Count()} items retrieved");

            List<Datasource> results = new List<Datasource>();
            foreach (var item in query)
            {
                var ds = mapper.Map<Datasource>(item);
                results.Add(ds);
            }
            return results.AsQueryable();
        }

        // GET: api/SensorData/5
        [ResponseType(typeof(Channel))]
        [Route("api/DataSource/Types")]
        public IQueryable<ChannelViewModel> GetDatasourceType()
        {
            logger.Info($"GET: {Request.RequestUri} called");

            var query = db.Channel;

            var results = new List<ChannelViewModel>();
            foreach (var item in query)
            {
                results.Add(new ChannelViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Created = item.Created
                });

            }

            logger.Info($"GET: {Request.RequestUri} finished");
            logger.Info($"{results.Count()} items retrieved");
            return results.AsQueryable();
        }

        // PUT: api/DataSource/5
        [ResponseType(typeof(void))]
        [Route("api/DataSource/{id}")]
        public IHttpActionResult Put(int id, DataSource datasource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != datasource.Id)
            {
                return BadRequest();
            }

            db.Entry(datasource).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error(ex.Message);
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

        // POST: api/DataSource
        [ResponseType(typeof(void))]
        public HttpResponseMessage Post(DataSource dataSource)
        {
            logger.Info($"POST: {Request.RequestUri} called");
            logger.Info("Storing Datasource: Id={0}, DeviceId={1}, Type={2}, Description={3}",
                dataSource.Id,
                dataSource.DeviceId,
                dataSource.ChannelId,
                dataSource.Description);
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            db.DataSource.Add(dataSource);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                HttpError err = new HttpError(ex.InnerException.InnerException.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, err);
            }
            logger.Info("Datasource added: Id={0}, DeviceId={1}, Type={2}, Description={3}",
                dataSource.Id,
                dataSource.DeviceId,
                dataSource.ChannelId,
                dataSource.Description);
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        // DELETE: api/DataSource/5
        [HttpDelete]
        [Route("api/DataSource/{id}")]
        public IHttpActionResult Delete(int id)
        {
            logger.Info($"DELETE: {Request.RequestUri} called");
            logger.Info("Deleting Datasource: Id={0}", id);
            DataSource dataSource = db.DataSource.Find(id);
            if (dataSource == null)
            {
                logger.Info("Datasource to be deleted not found: Id={0}", id);
                return NotFound();
            }

            db.DataSource.Remove(dataSource);
            db.SaveChanges();
            logger.Info("Datasource deleted: Id={0}", id);
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
