using AutoMapper;
using NLog;
//using SensorDataApi.Data;
using SensorDataApi.infrastructure;
using SensorDataCommon.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc;
using MySensorData.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Web.Http.Description;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;

namespace SensorDataApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DataSourceController : ControllerBase
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        private SensorDataSqlContext db;
        private IMapper mapper = AutomapperConfig.CreateMapper();

        public DataSourceController(SensorDataSqlContext db)
        {
            this.db = db;
        }

        [HttpGet("NewDatasources")]
        public NewDatasource[] NewDatasources()
        {
            //var query = db.SensorData
            //.Where(w => !db.DataSource.Any(a => a.DeviceId == w.DeviceId))
            //.Select(s => s.DeviceId).Distinct().ToArray();

            var query = from n in db.SensorData
                        where !db.DataSource.Any(a => a.DeviceId == n.DeviceId)
                        group n by n.DeviceId into g
                        select new NewDatasource { DeviceId = g.Key, TimeStamp = g.Max(t => t.TimeStamp), Count = g.Count() };

            return query.ToArray();

            //return query.Select(deviceId => new DataSource
            //{
            //    DeviceId = deviceId
            //}).ToArray();
        }

        // GET: api/DataSource
        [HttpGet]
        public IQueryable<DatasourceModel> GetDataSource()
        {
            logger.Info($"GET: {Request.Path} called");
            string channel = Request.Query["channel"];


            IQueryable<DataSource> query = db.DataSource;
            if (!string.IsNullOrEmpty(channel))
            {
                int id;
                if (int.TryParse(channel, out id))
                {
                    query = query.Where(w => w.DataTypeId == id).AsQueryable();
                }
            }

            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{query.Count()} items retrieved");

            try
            {
                List<DatasourceModel> results = new List<DatasourceModel>();
                foreach (var item in query)
                {
                    var ds = mapper.Map<DatasourceModel>(item);
                    results.Add(ds);
                }
                return results.AsQueryable();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost("UploadImage")]
        public IActionResult UploadImage()
        {
            //upload image
            var postedFile = Request.Form.Files.Single();
            //create custom filename
            //imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            //imageName = imageName + DateTime.Now.ToString("yymmddssfff") + Path.GetExtension(postedFile.FileName);
            //var filePath = HttpContext.Current.Server.MapPath("~/Image/" + imageName);
            //            postedFile.SaveAs(filePath);
            var id = Request.Form["id"].Single();

            var dataSource = db.DataSource.Single(s => s.DeviceId == id);

            var filePath = Path.GetTempFileName();
            using (var stream = new MemoryStream())
            {
                postedFile.CopyTo(stream);
                dataSource.Image = stream.ToArray();
            }

            db.SaveChanges();

            return Created(Request.Path, null);
        }

        [HttpGet("{id}/Images")]
        //[Route("api/DataSource/{id}/Images")]
        public IActionResult DownloadImage(string id)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                var dataSource = db.DataSource.Single(s => s.DeviceId == id);
                if (dataSource.Image == null)
                {
                    return NotFound("Datasource does not have an image associated with it");
                }

                return new FileStreamResult(new MemoryStream(dataSource.Image), "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }


        }

        //[HttpGet]
        //[Route("api/DataSource/{id}/Images2")]
        //public FileResult DownloadImage2(string id)
        //{
        //    var response = new HttpResponseMessage(HttpStatusCode.OK);
        //    try
        //    {
        //        var dataSource = db.DataSource.Single(s => s.DeviceId == id);
        //        response.Content = new ByteArrayContent(dataSource.Image);
        //        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        //        return File(dataSource.Image);
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}
        // GET: api/DataSource/5
        [HttpGet("{id}")]
        //[Route("api/DataSource/{id}")]
        public IQueryable<DataSource> GetDataSourceById(string id)
        {
            logger.Info($"GET: {Request.Path} called");

            IQueryable<DataSource> query = db.DataSource;
            query = query.Where(w => w.DeviceId == id).AsQueryable();

            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{query.Count()} items retrieved");

            try
            {
                List<DataSource> results = new List<DataSource>();
                foreach (var item in query)
                {
                    //todo: automapper terug zetten
                    //var ds = mapper.Map<DataSource>(item);
                    var ds = new DataSource
                    {
                        ChannelId = item.ChannelId,
                        //DataType=item.DataType,
                        DataTypeId = item.DataTypeId,
                        Description = item.Description,
                        DeviceId = item.DeviceId,
                        Id = item.Id
                    };
                    results.Add(ds);
                }
                return results.AsQueryable();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        // GET: api/SensorData/5
        [ResponseType(typeof(Channel))]
        //[Route("api/DataSource/Types")]
        [HttpGet("Types")]
        public IQueryable<ChannelViewModel> GetDatasourceType()
        {
            logger.Info($"GET: {Request.Path} called");

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

            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{results.Count()} items retrieved");
            return results.AsQueryable();
        }

        // PUT: api/DataSource/5
        [ResponseType(typeof(void))]
        //[Route("api/DataSource/{id}")]
        [HttpPut("{id}")]
        public ActionResult Put(int id, DataSource datasource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != datasource.Id)
            {
                return BadRequest();
            }

            var ds = db.DataSource.Single(s => s.Id == datasource.Id);
            ds.DataTypeId = datasource.DataTypeId;
            ds.Description = datasource.Description;
            //db.Entry(datasource).State = EntityState.Modified;

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

            return NoContent();
        }

        // POST: api/DataSource
        [ResponseType(typeof(void))]
        [HttpPost]
        public ActionResult Post(DataSource dataSource)
        {
            logger.Info($"POST: {Request.Path} called");

            logger.Info("Storing Datasource: Id={0}, DeviceId={1}, Type={2}, Description={3}",
                dataSource.Id,
                dataSource.DeviceId,
                dataSource.ChannelId,
                dataSource.Description);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.DataSource.Add(dataSource);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.InnerException.Message);
            }
            logger.Info("Datasource added: Id={0}, DeviceId={1}, Type={2}, Description={3}",
                dataSource.Id,
                dataSource.DeviceId,
                dataSource.ChannelId,
                dataSource.Description);
            return Created(Request.Path, dataSource);
        }

        // DELETE: api/DataSource/5
        [HttpDelete("{id}")]
        //[Route("api/DataSource/{id}")]
        public ActionResult Delete(int id)
        {
            logger.Info($"DELETE: {Request.Path} called");
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

        private bool SensorDataExists(int id)
        {
            return db.SensorData.Count(e => e.Id == id) > 0;
        }
    }
}
