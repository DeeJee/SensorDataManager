using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NLog;
using SensorData.Api.Models;
using SensorData.Api.infrastructure;

namespace SensorData.Api.Data.SqlServer
{
    public class SqlDataSourceRepository : IDataSourceRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        private IMapper mapper;
        private SensorDataSqlContext db;

        public SqlDataSourceRepository(SensorDataSqlContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }
        public NewDatasourceModel[] GetNewDatasources()
        {
            var query = from n in db.SensorData
                        where !db.DataSource.Any(a => a.DeviceId == n.DeviceId)
                        group n by n.DeviceId into g
                        select new NewDatasourceModel { DeviceId = g.Key, TimeStamp = g.Max(t => t.TimeStamp), Count = g.Count() };

            return query.ToArray();
        }

        public List<DataSourceModel> GetDataSources(string channel)
        {
            IQueryable<SqlDataSource> query = db.DataSource;
            if (!string.IsNullOrEmpty(channel))
            {
                int id;
                if (int.TryParse(channel, out id))
                {
                    query = query.Where(w => w.DataTypeId == id).AsQueryable();
                }
            }

            try
            {
                List<DataSourceModel> results = new List<DataSourceModel>();
                foreach (var item in query)
                {
                    var ds = mapper.Map<DataSourceModel>(item);
                    results.Add(ds);
                }
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UploadImage(string id, IFormFile postedFile)
        {
            var dataSource = db.DataSource.Single(s => s.DeviceId == id);

            var filePath = Path.GetTempFileName();
            using (var stream = new MemoryStream())
            {
                postedFile.CopyTo(stream);
                dataSource.Image = stream.ToArray();
            }

            db.SaveChanges();
        }

        public DataSourceModel GetDataSource(string id)
        {
            var item = db.DataSource.Single(s => s.DeviceId == id);
            return mapper.Map<DataSourceModel>(item);
        }

        public List<ChannelModel> GetChannels()
        {
            var query = db.Channel;

            var results = new List<ChannelModel>();
            foreach (var item in query)
            {
                results.Add(new ChannelModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Created = item.Created
                });
            }
            return results;
        }

        public void Add(DataSourceModel dataSource)
        {
            db.DataSource.Add(mapper.Map<SqlDataSource>(dataSource));
            db.SaveChanges();
        }

        private void StatusCode(object status500InternalServerError, string message)
        {
            throw new NotImplementedException();
        }

        public void Delete(string id)
        {
            SqlDataSource dataSource = db.DataSource.Find(id);
            if (dataSource == null)
            {
                logger.Info("Datasource to be deleted not found: Id={0}", id);
            }
            else
            {
                db.DataSource.Remove(dataSource);
                db.SaveChanges();
            }
        }

        public void Add(CosmosDB.CdbDataSource dataSource)
        {
            throw new NotImplementedException();
        }


        public void Add(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void blaat()
        {
            throw new NotImplementedException();
        }
    }
}
