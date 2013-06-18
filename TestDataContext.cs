using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using AWSUploadTestTool.Annotations;

namespace AWSUploadTestTool
{
    public class TestDataContext : INotifyPropertyChanged
    {
        private string _fileName;
        private string _account;
        private string _key;
        private string _bucketName;
        private ObservableCollection<string> _log;
        private int _iterations;
        private int _completedIterations;
        private TimeSpan _totalDuration;
        private int _errors;


        public TestDataContext()
        {
            Iterations = 1000;
            Log = new ObservableCollection<string>();
        }

        public string Account
        {
            get { return _account; }
            set
            {
                if (value == _account) return;
                _account = value;
                OnPropertyChanged("Account");
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                if (value == _key) return;
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        public string BucketName
        {
            get { return _bucketName; }
            set
            {
                if (value == _bucketName) return;
                _bucketName = value;
                OnPropertyChanged("BucketName");
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public int Iterations
        {
            get { return _iterations; }
            set
            {
                if (value == _iterations) return;
                _iterations = value;
                OnPropertyChanged("Iterations");
            }
        }

        public int CompletedIterations
        {
            get { return _completedIterations; }
            set
            {
                if (value == _completedIterations) return;
                _completedIterations = value;

                OnPropertyChanged("CompletedIterations");
                OnPropertyChanged("AverageUploadTime");
                OnPropertyChanged("Errors");
                OnPropertyChanged("TotalDuration");

            }
        }

        public double AverageUploadTime
        {
            get
            {
                if (CompletedIterations == 0) return 0;

                double average = TotalDuration.TotalSeconds / CompletedIterations;

                return average;
            }
        }

        public int Errors
        {
            get { return _errors; }
            set
            {
                if (value == _errors) return;
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public ObservableCollection<string> Log
        {
            get { return _log; }
            set
            {
                if (Equals(value, _log)) return;
                _log = value;
                OnPropertyChanged("Log");
            }
        }

        public TimeSpan TotalDuration
        {
            get { return _totalDuration; }
            set
            {
                _totalDuration = value;
                OnPropertyChanged("TotalDuration");
            }
        }

        protected bool TestInProgress { get; set; }

        protected internal void AddToLog(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Log.Add(string.Format("{0}- {1}", DateTime.Now.ToLongTimeString(), message));
                OnPropertyChanged("Log");
                OnPropertyChanged("TotalDuration");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        public void RefreshTotalDuration(object sender, EventArgs e)
        {
         //   TotalDuration += new TimeSpan(1);
            OnPropertyChanged("TotalDuration");
        }
    }
}
