using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using NLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using SensorData.Api.Data;
using SensorData.Api.Models;

namespace SensorDataApi.Controllers
{
    //[TokenValidation]
    [Route("api/[controller]")]
    [ApiController]
#if DEBUG
#else
[Authorize]
#endif
    public class DataTypesController : ControllerBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IDataTypeRepository datatypeRepo;

        public DataTypesController(IDataTypeRepository repo)
        {
            this.datatypeRepo = repo;
        }

        // GET: api/DataTypes
        [HttpGet]
        public IEnumerable<DataTypeModel> GetDataTypes()
        {
            return datatypeRepo.GetDataTypes();

        }

        // GET: api/DataTypes/5
        [ResponseType(typeof(DataTypeModel))]
        [HttpGet("{id}")]
        public ActionResult GetDataType(int id)
        {
            DataTypeModel dataType = datatypeRepo.GetDataType(id);
            if (dataType == null)
            {
                return NotFound();
            }

            var item = new DataTypeModel
            {
                Id = dataType.Id,
                Name = dataType.Name,
                Properties = dataType.Properties
            };

            return Ok(item);
        }

        // PUT: api/DataTypes/5
        [ResponseType(typeof(void))]
        [HttpPut("{id}")]
        public ActionResult PutDataType(int id, DataTypeModel dataType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dataType.Id)
            {
                return BadRequest();
            }

            try
            {
                datatypeRepo.UpdateDataType(dataType);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error(ex);
                throw;
            }

            return NoContent();
        }

        // POST: api/DataTypes
        [ResponseType(typeof(DataTypeModel))]
        [HttpPost]
        public IActionResult PostDataType(DataTypeModel dataType)
        {
            logger.Info($"POST: {Request.Path} called");
            logger.Info("Storing DataType: Id={0}, Name={1}, Properties={2}",
                dataType.Id,
                dataType.Name,
                dataType.Properties);
            if (!ModelState.IsValid)
            {
                logger.Info("Bad request");
                return BadRequest(ModelState);
            }

            try
            {
                datatypeRepo.AddDataType(dataType);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return Created(Request.Path, dataType);
        }

        // DELETE: api/DataTypes/5
        [ResponseType(typeof(DataType))]
        [HttpDelete]
        public ActionResult DeleteDataType(int id)
        {
            datatypeRepo.DeleteDataType(id);

            return NoContent();
        }

        private static Uri GetUri(HttpRequest request)
        {
            var builder = new UriBuilder();
            builder.Scheme = request.Scheme;
            builder.Host = request.Host.Value;
            builder.Path = request.Path;
            builder.Query = request.QueryString.ToUriComponent();
            return builder.Uri;
        }
    }
}