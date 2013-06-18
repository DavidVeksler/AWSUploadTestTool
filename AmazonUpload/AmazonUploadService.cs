using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Amazon;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace AWSUploadTestTool.AmazonUpload
{
    public class AmazonUploadService
    {
        public async Task<Task<TimeSpan>> UploadAsync(string bucketName, string fileName, string key, string accessKey, string secretKey, EventHandler<UploadProgressArgs> uploadStatusSubscriber, EventHandler<EventArgs> uploadDoneNotifier)
        {
            //var uploadFileUrl = string.Format("https://{0}.s3.amazonaws.com/{1}_{2}", bucketName, fileName);

            var timer = new Stopwatch();
            timer.Start();

            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    using (var client = AWSClientFactory.CreateAmazonS3Client(accessKey,secretKey))
                    {
                        var transferUtility = new TransferUtility(client);

                        var uploadRequest = new TransferUtilityUploadRequest();
                        uploadRequest.WithBucketName(bucketName)
                                     .WithKey(key)
                                     .WithSubscriber(uploadStatusSubscriber)
                                     .WithInputStream(stream);

                        await transferUtility.UploadAsync(uploadRequest);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
               // MessageBox.Show(ex.ToString(), "Error Uploading File " + fileName);
                //uploadStatusSubscriber.Invoke(ex, new UploadProgressArgs(0, 0, 0) { });
                uploadDoneNotifier.Invoke(ex, new UploadProgressArgs(0, 0, 0) { });
            }

            timer.Stop();

            uploadDoneNotifier(new UploadResult() { duration = timer.Elapsed, key = key, bucket = bucketName }, null);


            var task = Task.Factory.StartNew(() => DeleteObject(accessKey, secretKey, bucketName, key));
            
            return new Task<TimeSpan>(() => timer.Elapsed);
        }

        private void DeleteObject(string accessKey, string secretKey, string bucketName, string key)
        {
            try
            {
                using (var client = AWSClientFactory.CreateAmazonS3Client(accessKey, secretKey))
                {
                    client.DeleteObject(new DeleteObjectRequest().WithBucketName(bucketName).WithKey(key));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void DeleteCallback(IAsyncResult ar)
        {
            
        }
    }
}