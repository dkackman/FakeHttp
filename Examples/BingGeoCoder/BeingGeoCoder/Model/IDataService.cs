using System.Threading.Tasks;

namespace BeingGeoCoder.Model
{
    public interface IDataService
    {
        Task<DataItem> GetData();
    }
}