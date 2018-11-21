using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace hjudgeWeb.Utils
{
    public class ImageScaler
    {
        public static byte[] ScaleImage(byte[] content, int height, int weight)
        {
            using (var imgStream = new MemoryStream())
            {
                using (var image = Image.Load(content))
                {
                    image.Mutate(i => i.Resize(height, weight));
                    image.Save(imgStream, Image.DetectFormat(content));
                    return imgStream.GetBuffer();
                }
            }
        }
    }
}
