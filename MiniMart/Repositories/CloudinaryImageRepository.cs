// Import the required packages for Cloudinary
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;

namespace MiniMart.Repositories
{
    public class CloudinaryImageRepository : IImageRepository
    {
        private readonly IConfiguration configuration;
        private readonly Cloudinary cloudinary;
        public CloudinaryImageRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
            // Set your Cloudinary credentials
            //=================================

            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;
        }

        

        public async Task<string> UploadAsync(IFormFile file)
        {
            // Upload an image and log the response to the console
            //=================

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                DisplayName = file.Name
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            if (uploadResult != null && uploadResult.StatusCode==System.Net.HttpStatusCode.OK) {
                Console.WriteLine(uploadResult.JsonObj);
                return uploadResult.SecureUri.ToString();
            }
            return null;
        }
    }
}
