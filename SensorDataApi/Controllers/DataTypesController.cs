using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using SensorDataCommon.Data;
using NLog;
using System.Data.Entity.Validation;
using System.Web.Http.Results;

namespace SensorDataApi.Controllers
{
    public class DataTypesController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private SensorDataSqlEntities db = new SensorDataSqlEntities();

        // GET: api/DataTypes
        public IEnumerable<Models.DataType> GetDataType()
        {
            var result = db.DataType.ToList();
            foreach (var dt in result)
            {
                yield return new Models.DataType { Id = dt.Id, Name = dt.Name, Properties = dt.Properties };
            }
        }

        // GET: api/DataTypes/5
        [ResponseType(typeof(DataType))]
        [HttpGet]
        public IHttpActionResult GetDataType(int id)
        {
            DataType dataType = db.DataType.Find(id);
            if (dataType == null)
            {
                return NotFound();
            }

            var item = new Models.DataType
            {
                Id = dataType.Id,
                Name = dataType.Name,
                Properties = dataType.Properties
            };

            return Ok(item);
        }

        // PUT: api/DataTypes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDataType(int id, DataType dataType)
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

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/DataTypes
        [ResponseType(typeof(DataType))]
        [HttpPost]
        public HttpResponseMessage PostDataType(DataType dataType)
        {
            logger.Info($"POST: {Request.RequestUri} called");
            logger.Info("Storing DataType: Id={0}, Name={1}, Properties={2}",
                dataType.Id,
                dataType.Name,
                dataType.Properties);
            if (!ModelState.IsValid)
            {
                logger.Info("Bad request");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            db.DataType.Add(dataType);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (DataTypeExists(dataType.Properties))
                {
                    logger.Error(ex);
                    HttpError err = new HttpError("A datatype with these properties already exists.");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, err);
                }
                else
                {
                    logger.Error(ex);
                    HttpError err = new HttpError(ex.InnerException.InnerException.Message);
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, err);
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var err in ex.EntityValidationErrors)
                {
                    foreach (var itm in err.ValidationErrors)
                    {
                        logger.Error(itm.ErrorMessage);
                    }

                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        // DELETE: api/DataTypes/5
        [ResponseType(typeof(DataType))]
        public IHttpActionResult DeleteDataType(int id)
        {
            DataType dataType = db.DataType.Find(id);
            if (dataType == null)
            {
                return NotFound();
            }

            db.DataType.Remove(dataType);
            db.SaveChanges();

            return Ok(dataType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DataTypeExists(string properties)
        {
            return db.DataType.Count(e => e.Properties == properties) > 0;
        }
        private bool DataTypeExists(int id)
        {
            return db.DataType.Count(e => e.Id == id) > 0;
        }
    }
}