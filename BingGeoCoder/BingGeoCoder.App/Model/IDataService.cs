using System.Threading.Tasks;

namespace BingGeoCoder.App.Model
{
    public interface IDataService
    {
        Task<DataItem> GetData();
    }
}