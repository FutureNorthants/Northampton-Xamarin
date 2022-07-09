using System.IO;
using System.Threading.Tasks;

namespace Northampton
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}