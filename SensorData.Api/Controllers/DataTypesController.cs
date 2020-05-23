using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using NLog;
using Microsoft.AspNetCore.Mvc;
using MySensorData.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace SensorDataApi.Controllers
{
    //[TokenValidation]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DataTypesController : ControllerBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SensorDataSqlContext db;

        public DataTypesController(SensorDataSqlContext db)
        {
            this.db = db;
        }
        
        // GET: api/DataTypes
        [HttpGet]
        public IEnumerable<MySensorData.Common.Data.DataType> GetDataTypes()
        {
            var result = db.DataType.ToList();
            foreach (var dt in result)
            {
                yield return new MySensorData.Common.Data.DataType { Id = dt.Id, Name = dt.Name, Properties = dt.Properties };
            }
        }

        // GET: api/DataTypes/5
        [ResponseType(typeof(MySensorData.Common.Data.DataType))]
        [HttpGet("{id}")]
        public ActionResult GetDataType(int id)
        {
            MySensorData.Common.Data.DataType dataType = db.DataType.Find(id);
            if (dataType == null)
            {
                return NotFound();
            }

            var item = new MySensorData.Common.Data.DataType
            {
                Id = dataType.Id,
                Name = dataType.Name,
                Properties = dataType.Properties
            };

            return Ok(item);
        }

        // PUT: api/DataTypes/5
        [ResponseType(typeof(void))]
        public ActionResult PutDataType(int id, MySensorData.Common.Data.DataType dataType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dataType.Id)
            {
                return BadRequest();
            }

            db.Entry(dataType).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DataTypeExists(id))
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

        // POST: api/DataTypes
        [ResponseType(typeof(SensorDataCommon.Models.DataType))]
        [HttpPost]
        public IActionResult PostDataType(MySensorData.Common.Data.DataType dataType)
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

            db.DataType.Add(dataType);

            try
            {
                var validationContext = new ValidationContext(dataType);
                Validator.ValidateObject(dataType, validationContext);
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (DataTypeExists(dataType.Properties))
                {
                    logger.Error(ex);
                    return StatusCode(StatusCodes.Status500InternalServerError, "A datatype with these properties already exists.");
                }
                else
                {
                    logger.Error(ex);
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.InnerException.InnerException.Message);
                }
            }
            catch (ValidationException ex)
            {
                logger.Error(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }

            return Created(Request.Path, dataType);
        }

        // DELETE: api/DataTypes/5
        [ResponseType(typeof(MySensorData.Common.Data.DataType))]
        public ActionResult DeleteDataType(int id)
        {
            MySensorData.Common.Data.DataType dataType = db.DataType.Find(id);
            if (dataType == null)
            {
                return NotFound();
            }

            db.DataType.Remove(dataType);
            db.SaveChanges();

            return Ok(dataType);
        }

        private bool DataTypeExists(string properties)
        {
            return db.DataType.Count(e => e.Properties == properties) > 0;
        }
        private bool DataTypeExists(int id)
        {
            return db.DataType.Count(e => e.Id == id) > 0;
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