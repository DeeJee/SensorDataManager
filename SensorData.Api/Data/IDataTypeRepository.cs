using SensorData.Api.Models;
using System.Collections.Generic;

namespace SensorData.Api.Data
{
    public interface IDataTypeRepository
    {
        IEnumerable<DataTypeModel> GetDataTypes();
        DataTypeModel GetDataType(int id);
        void DeleteDataType(int id);
        void UpdateDataType(DataTypeModel dataType);
        void AddDataType(DataTypeModel dataType);
    }
}
