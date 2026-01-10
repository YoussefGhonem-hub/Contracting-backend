using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Storage.AWS3.Models;
using System.Net;

namespace Storage.AWS3.Extensions
{
    public static class S3Extensions
    {
        public static async Task<StoredFile> UploadToS3Async(this IConfiguration configuration, IFormFile? file, CancellationToken cancellationToken = default)
        {
            if (file is null) return new StoredFile();

            try
            {
                var options = configuration.GetAWSConfigurationOptions();
                var region = AWS3ConfigurationExtension.GetRgionAWS();
                var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(configuration);

                var key = Guid.NewGuid();
                var stream = file.OpenReadStream();
                var bucketName = options.DefaultBucket;
                var fileNameStorage = GetFileName(key, file.FileName);

                var uploadRequest = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = fileNameStorage,
                    InputStream = stream
                };

                using var client = new AmazonS3Client(credential, region);
                await client.PutObjectAsync(uploadRequest, cancellationToken);

                return new StoredFile()
                {
                    FileName = file.FileName,
                    Key = fileNameStorage,
                    Extension = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                };
            }
            catch
            {
                throw;
            }
        }

        public static async Task<List<StoredFile>> UploadFilesToS3Async(this IConfiguration configuration, List<IFormFile>? files, CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0) return new List<StoredFile>();

            var uploadedFiles = new List<StoredFile>();

            foreach (var file in files)
            {
                var result = await configuration.UploadToS3Async(file, cancellationToken);
                uploadedFiles.Add(result);
            }

            return uploadedFiles;
        }

        public static async Task<bool> DeleteFromS3Async(this IConfiguration configuration, string key, CancellationToken cancellationToken = default)
        {
            var options = configuration.GetAWSConfigurationOptions();
            var region = AWS3ConfigurationExtension.GetRgionAWS();
            var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(configuration);
            var bucketName = options.DefaultBucket;

            using var client = new AmazonS3Client(credential, region);
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            try
            {
                var response = await client.DeleteObjectAsync(deleteRequest, cancellationToken);
                return response.HttpStatusCode == HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string?> GetPreSignedUrlAsync(this IConfiguration configuration, string? key)
        {
            try
            {
                if (string.IsNullOrEmpty(key)) return null;
                var options = configuration.GetAWSConfigurationOptions();
                var region = AWS3ConfigurationExtension.GetRgionAWS();
                var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(configuration);
                var bucketName = options.DefaultBucket;

                using var client = new AmazonS3Client(credential, region);
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    Expires = DateTime.Now.AddHours(1)
                };

                return client.GetPreSignedURL(request);
            }
            catch
            {
                throw;
            }
        }

        public static async Task<DownloadedFile> DownloadFromS3Async(this IConfiguration configuration, string key, CancellationToken cancellationToken)
        {
            var options = configuration.GetAWSConfigurationOptions();
            var bucketName = options.DefaultBucket;
            var region = AWS3ConfigurationExtension.GetRgionAWS();
            var credential = AWS3ConfigurationExtension.GetBasicAWSCredentials(configuration);

            using var client = new AmazonS3Client(credential, region);
            using var transferUtility = new TransferUtility(client);

            var downloadRequest = new TransferUtilityDownloadRequest
            {
                BucketName = bucketName,
                Key = key,
                FilePath = "D:\\images\\" + key
            };

            await transferUtility.DownloadAsync(downloadRequest, cancellationToken);

            var fileBytes = File.ReadAllBytes(downloadRequest.FilePath);
            return new DownloadedFile(fileBytes, GetContentType(key), key);
        }

        #region Helpers

        private static string GetFileName(Guid blobId, string contentType)
        {
            var fileSplit = contentType.Split('.');
            var fileExtension = fileSplit[^1];
            var blobFileName = blobId.ToString();
            if (fileSplit.Length > 1) blobFileName += "." + fileExtension;
            return blobFileName;
        }

        private static string GetContentType(string? filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLower();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }

        #endregion
    }

}
