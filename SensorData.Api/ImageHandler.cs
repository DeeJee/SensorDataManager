using Microsoft.AspNetCore.Http;
using System.IO;

namespace SensorData.Api
{
    public class ImageHandler
    {
        const string imagesSubdir = "images";
        const string datasourcesSubdir = "datasources";
        public void SaveImage(string contentRoot, string deviceId, IFormFile postedFile)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(contentRoot, imagesSubdir, datasourcesSubdir));
            if (!dir.Exists)
            {
                dir.Create();
            }
            var fullName = Path.Combine(dir.FullName, $"{deviceId}.jpg");
            using (var fs = System.IO.File.Create(fullName))
            {
                postedFile.CopyTo(fs);
            }
        }

        public Stream LoadImage(string contentRoot, string deviceId)
        {
            var file = new FileInfo(Path.Combine(contentRoot, imagesSubdir, datasourcesSubdir, $"{deviceId}.jpg"));
            if (!file.Exists)
            {
                return null;
            }

            return File.OpenRead(file.FullName);
        }
    }
}
