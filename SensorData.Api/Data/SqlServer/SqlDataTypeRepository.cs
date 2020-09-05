using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NLog;
using SensorData.Api.Models;

namespace SensorData.Api.Data.SqlServer
{
    public class SqlDataTypeRepository : IDataTypeRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SensorDataSqlContext db;

        public SqlDataTypeRepository(SensorDataSqlContext db)
        {
            this.db = db;
        }

        public void UpdateDataType(DataTypeModel dataType)
        {
            db.Entry(dataType).State = EntityState.Modified;
            db.SaveChanges();
        }

        public DataTypeModel GetDataType(int id)
        {
            return db.DataType.Find(id);
        }

        public IEnumerable<DataTypeModel> GetDataTypes()
        {
            var result = db.DataType.ToList();
            foreach (var dt in result)
            {
                yield return new DataTypeModel { Id = dt.Id, Name = dt.Name, Properties = dt.Properties };
            }
        }

        public void AddDataType(DataTypeModel dataType)
        {
            try
            {
                db.DataType.Add(dataType);
                var validationContext = new ValidationContext(dataType);
                Validator.ValidateObject(dataType, validationContext);
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (DataTypeExists(dataType.Properties))
                {
                    logger.Error(ex);
                    throw new Exception("A datatype with these properties already exists.");
                }
                else
                {
                    logger.Error(ex);
                    throw;
                }
            }
            catch (ValidationException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public void DeleteDataType(int id)
        {
            DataTypeModel dataType = db.DataType.Find(id);
            if (dataType != null)
            {
                db.DataType.Remove(dataType);
                db.SaveChanges();
            }
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
