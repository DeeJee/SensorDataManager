using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySensorData.Common.Data
{
    public interface IDataTypeRepository
    {
        IEnumerable<DataType> GetDataTypes();
        DataType GetDataType(int id);
        void DeleteDataType(int id);
        void UpdateDataType(DataType dataType);
        void AddDataType(DataType dataType);
    }
}
