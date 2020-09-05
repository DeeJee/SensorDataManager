using NLog;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Web.Http.Description;
using Microsoft.AspNetCore.Authorization;
using SensorData.Api.Data;
using AutoMapper;
using SensorData.Api.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using SensorData.Api;

namespace SensorDataApi.Controllers
{
    [ApiController]
#if DEBUG
#else
    [Authorize]
#endif
    [Route("api/[controller]")]
    public class DataSourceController : ControllerBase
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        private IDataSourceRepository datasourceRepo;
        private readonly IWebHostEnvironment hostingEnvironment;
        ImageHandler imageHandler = new ImageHandler();
        
        public DataSourceController(IDataSourceRepository datasource, IWebHostEnvironment hostingEnvironment)
        {
            this.datasourceRepo = datasource;
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("NewDatasources")]
        public NewDatasourceModel[] NewDatasources()
        {
            var newDatasources = datasourceRepo.GetNewDatasources();

            return newDatasources;
        }

        // GET: api/DataSource
        [HttpGet]
        public List<DataSourceModel> GetDataSource()
        {
            //datasourceRepo.blaat();

            logger.Info($"GET: {Request.Path} called");
            string channel = Request.Query["channel"];

            var result = datasourceRepo.GetDataSources(channel);
            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{result.Count()} items retrieved");
            return result;
        }

        [HttpPost("UploadImage")]
        public IActionResult UploadImage()
        {
            //upload image
            var postedFile = Request.Form.Files.Single();

            var id = Request.Form["id"].Single();

            imageHandler.SaveImage(hostingEnvironment.ContentRootPath, id, postedFile);

            return NoContent();
        }

        [HttpGet("{id}/Images")]
        //[Route("api/DataSource/{id}/Images")]
        public IActionResult DownloadImage(string id)
        {
            try
            {
                var image = imageHandler.LoadImage(hostingEnvironment.ContentRootPath, id);
                if (image == null) return NotFound();
                return new FileStreamResult(image, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        // GET: api/DataSource/5
        [HttpGet("{id}")]
        //[Route("api/DataSource/{id}")]
        public DataSourceModel GetDataSourceById(string id)
        {
            logger.Info($"GET: {Request.Path} called");

            var dsource = datasourceRepo.GetDataSource(id);

            logger.Info($"GET: {Request.Path} finished");

            return dsource;
        }

        // GET: api/SensorData/5
        [ResponseType(typeof(ChannelModel))]
        //[Route("api/DataSource/Types")]
        [HttpGet("Types")]
        public IQueryable<ChannelModel> GetDatasourceType()
        {
            logger.Info($"GET: {Request.Path} called");

            var results = datasourceRepo.GetChannels();

            logger.Info($"GET: {Request.Path} finished");
            logger.Info($"{results.Count()} items retrieved");
            return results.AsQueryable();
        }

        //// PUT: api/DataSource/5
        //[ResponseType(typeof(void))]
        ////[Route("api/DataSource/{id}")]
        //[HttpPut("{id}")]
        //public ActionResult Put(int id, DataSource datasource)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != datasource.Id)
        //    {
        //        return BadRequest();
        //    }

        //    var ds = db.DataSource.Single(s => s.Id == datasource.Id);
        //    ds.DataTypeId = datasource.DataTypeId;
        //    ds.Description = datasource.Description;
        //    //db.Entry(datasource).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        logger.Error(ex.Message);
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

        // POST: api/DataSource
        [ResponseType(typeof(void))]
        [HttpPost]
        public ActionResult Post(DataSourceModel dataSource)
        {
            logger.Info($"POST: {Request.Path} called");

            logger.Info("Storing Datasource: Id={0}, DeviceId={1}, Type={2}, Description={3}",
                dataSource.Id,
                dataSource.DeviceId,
                //dataSource.ChannelId,
                dataSource.Description);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                datasourceRepo.Add(dataSource);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.InnerException.Message);
            }
            logger.Info("Datasource added: Id={0}, DeviceId={1}, Type={2}, Description={3}",
                dataSource.Id,
                dataSource.DeviceId,
                //  dataSource.ChannelId,
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

            datasourceRepo.Delete(id.ToString());

            logger.Info("Datasource deleted: Id={0}", id);
            return Ok();
        }

        //private bool SensorDataExists(string id)
        //{
        //    return db.SensorData.Count(e => e.Id == id) > 0;
        //}
    }
}
