using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AWSUploadTestTool.AmazonUpload;
using Amazon;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Win32;

namespace AWSUploadTestTool
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Default S3 Values:

            DataContext = new TestDataContext
                {
                    Account = "",
                    Key = "",
                    BucketName = ""
                };
            AmazonUploadService = new AmazonUploadService();
            
        }

        protected AmazonUploadService AmazonUploadService { get; set; }

        public TestDataContext TestContext
        {
            get { return ((TestDataContext)DataContext); }
        }


        private void btnPickFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
                {
                    Multiselect = false
                };

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                //((TestDataContext)DataContext).FileName = fileDialog.FileName;
                ((TestDataContext)DataContext).FileName = fileDialog.FileName;
            }
        }

        private string testId;
        //Task<Task<TimeSpan>> result = null;
        readonly System.Timers.Timer timer = new System.Timers.Timer();

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            TestContext.CompletedIterations = 0;
            TestContext.TotalDuration = new TimeSpan(0);
            TestContext.Errors = 0;

            if (string.IsNullOrWhiteSpace(TestContext.FileName))
            {
                MessageBox.Show(("File name required"));
                return;
            }

            testId = Guid.NewGuid().ToString();
            UploadToS3();


            //timer.Elapsed += TestContext.RefreshTotalDuration; // Everytime timer ticks, timer_Tick will be called
            //timer.Interval = 100;           // Timer will tick evert 10 seconds
            //timer.Enabled = true;                       // Enable the timer
            //timer.Start();                              // Start the timer




        }

        private void UploadToS3()
        {
            if (TestContext.CompletedIterations < TestContext.Iterations)
            {

                if (StopTest)
                {
                    AddToLog("Test Stopped");
                    return;
                }

                AddToLog("Upload #" + (TestContext.CompletedIterations + 1));

                string key = testId + TestContext.CompletedIterations + new FileInfo(TestContext.FileName).Name;

                AmazonUploadService.UploadAsync(TestContext.BucketName, TestContext.FileName,
                                                         key, TestContext.Account,
                                                         TestContext.Key, StatusNotification,
                                                         DoneNotification);

                TestContext.RefreshTotalDuration(null, null);
            }
            
        }

        private void DoneNotification(object sender, EventArgs e)
        {
            TestContext.CompletedIterations++;

            var exception = sender as Exception;
            if (exception != null)
            {
                AddToLog(exception.ToString());
                TestContext.Errors++;
                
            }
            else
            {
                var result = (UploadResult)sender;

                AddToLog("100% done in " + result.duration.ToString());
                TestContext.TotalDuration = TestContext.TotalDuration.Add(result.duration);

                //Dispatcher.BeginInvoke(new Action(() => DeleteUploadedFile(result)));
            }

            if (TestContext.CompletedIterations == TestContext.Iterations)
            {
                AddToLog(string.Format("TEST COMPLETED"));

                AddToLog(string.Format(@"
Iterations: {0}
Average Time: {1}
Duration: {2}
Errors: {3}
", TestContext.CompletedIterations, TestContext.AverageUploadTime, TestContext.TotalDuration, TestContext.Errors));

                MessageBox.Show("Test Done in " + TestContext.TotalDuration);

                timer.Stop();
            }
            else
            {
                // continue test
            }

            UploadToS3();
        }

        //private void DeleteUploadedFile(UploadResult result)
        //{
        //    var task = Task.Factory.StartNew(() => DeleteUploadedFileAsync(result));
        //}

        //private void DeleteUploadedFileAsync(UploadResult result)
        //{
        //    try
        //    {

        //        using (var client = AWSClientFactory.CreateAmazonS3Client(TestContext.Account, TestContext.Key))
        //        {

        //            client.BeginDeleteObject(new DeleteObjectRequest().WithBucketName(result.bucket).WithKey(result.key), null, null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddToLog("Error deleting upload:" + ex);
        //    }
        //}

        private int percentDone;
        private void StatusNotification(object sender, UploadProgressArgs e)
        {
            if ((e.PercentDone % 10) != 0) return;

            if (e.PercentDone == percentDone) return;
            
            percentDone = e.PercentDone;
            AddToLog(e.PercentDone + "% done");


        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.StopTest = true;
            
            timer.Stop();
            AddToLog("Stopping Test");
        }

        protected bool StopTest { get; set; }

        private void AddToLog(string s)
        {
            Dispatcher.Invoke(new Action(() => TestContext.AddToLog(s)));
        }

        private void Log_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string v = TestContext.Log.Aggregate("", (current, s) => current + Environment.NewLine + s);
            Clipboard.SetText(v);
        }
    }
}