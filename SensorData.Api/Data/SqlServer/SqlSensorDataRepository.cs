using AutoMapper;
using NLog;
using SensorData.Api.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SensorData.Api.Data.SqlServer
{
    public class SqlSensorDataRepository : ISensorDataRepository
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        CultureInfo nederland = CultureInfo.CurrentCulture;
        private SensorDataSqlContext db;
        private readonly IMapper mapper;

        public SqlSensorDataRepository(SensorDataSqlContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public IEnumerable<SensorDataModel> Get(string deviceId, string vanDatum, string totDatum)
        {
            var query = db.SensorData.Where(w => w.DeviceId == deviceId);

            DateTime vanDateTime;
            if (string.IsNullOrEmpty(vanDatum))
            {
                vanDateTime = DateTime.Now.Date.ToUniversalTime();

                query = query.Where(w => w.TimeStamp >= vanDateTime);
            }
            else
            {
                vanDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(vanDatum, "yyyy-MM-dd", nederland), info);
                //results=results.Where(w => DbFunctions.TruncateTime(w.TimeStamp) >= vanDateTime);
                query = query.Where(w => w.TimeStamp >= vanDateTime);
            }

            DateTime totDateTime;
            if (string.IsNullOrEmpty(totDatum))
            {
                var morgen = DateTime.Now.Date.AddDays(1).ToUniversalTime();
                query = query.Where(w => w.TimeStamp < morgen);
            }
            else
            {
                totDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(totDatum, "yyyy-MM-dd", nederland), info);
                //results=results.Where(w => DbFunctions.TruncateTime(w.TimeStamp) <= totDateTime);
                query = query.Where(w => w.TimeStamp <= totDateTime);
            }

            //execute query
            var results = query.ToList();

            foreach (var result in results)
            {
                result.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(result.TimeStamp, info);
            }

            logger.Info($"{results.Count()} items retrieved");

            return results.Select(item=>mapper.Map<SensorDataModel>(item)).ToList();
        }

        public IEnumerable<SensorDataModel> Get()
        {
            var data = db.SensorData.Take(1000);
            return data.Select(item=> mapper.Map<SensorDataModel>(item)).ToList();
        }

        public SensorDataModel MostRecent(string id)
        {
            var item = db.SensorData.Where(w => w.DeviceId == id).OrderByDescending(o => o.Id).Take(1).SingleOrDefault();
            if (item != null)
            {
                item.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(item.TimeStamp, info);
            }
            
            return mapper.Map<SensorDataModel>(item);
        }

        public IEnumerable<SensorDataModel> MostRecent()
        {
            var data = new List<SensorDataModel>();
            var datasources = db.DataSource;
            foreach (var ds in datasources)
            {
                var item = db.SensorData.Where(w => w.DeviceId == ds.DeviceId).OrderBy(o => o.Id).Last();
                item.TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(item.TimeStamp, info);

                data.Add(mapper.Map<SensorDataModel>(item));
            }
            return data;
        }

        public int Count(string id)
        {
            var data = db.SensorData.Count(w => w.DeviceId == id);
            return data;
        }

        public void DeleteSensorData(string deviceId)
        {
            //TODO: omschrijven naar efficiente code
            //db.SensorData.Where(w => w.DeviceId == deviceId).Delete();
            var toBeDeleted = db.SensorData.Where(w => w.DeviceId == deviceId);
            db.SensorData.RemoveRange(toBeDeleted);
            db.SaveChanges();
        }

        public void PostSensorData(SensorDataModel sensorData)
        {
            db.SensorData.Add(new SqlSensorData
            {
                DeviceId = sensorData.DeviceId,
                TimeStamp = DateTime.Now,
                Payload = sensorData.Payload.ToString()
            });
            db.SaveChanges();
        }

        public void blaat()
        {
            throw new NotImplementedException();
        }
    }
}
