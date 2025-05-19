using System.ComponentModel;

namespace FolderCompareApp.Models
{
    public class FileComparisonModel : INotifyPropertyChanged
    {
        private string _path;
        private ComparisonStatus _status;
        private long _sizeLeft;
        private long _sizeRight;
        private DateTime _dateLeft;
        private DateTime _dateRight;
        private bool _sizeDifferent;
        private bool _dateDifferent;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public ComparisonStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText => Status switch
        {
            ComparisonStatus.Identical => "Identical",
            ComparisonStatus.Different => "Different",
            ComparisonStatus.LeftOnly => "Left Only",
            ComparisonStatus.RightOnly => "Right Only",
            _ => "Unknown"
        };

        public long SizeLeft
        {
            get => _sizeLeft;
            set
            {
                _sizeLeft = value;
                OnPropertyChanged(nameof(SizeLeft));
                OnPropertyChanged(nameof(SizeLeftFormatted));
            }
        }

        public long SizeRight
        {
            get => _sizeRight;
            set
            {
                _sizeRight = value;
                OnPropertyChanged(nameof(SizeRight));
                OnPropertyChanged(nameof(SizeRightFormatted));
            }
        }

        public DateTime DateLeft
        {
            get => _dateLeft;
            set
            {
                _dateLeft = value;
                OnPropertyChanged(nameof(DateLeft));
                OnPropertyChanged(nameof(DateLeftFormatted));
            }
        }

        public DateTime DateRight
        {
            get => _dateRight;
            set
            {
                _dateRight = value;
                OnPropertyChanged(nameof(DateRight));
                OnPropertyChanged(nameof(DateRightFormatted));
            }
        }

        public bool SizeDifferent
        {
            get => _sizeDifferent;
            set
            {
                _sizeDifferent = value;
                OnPropertyChanged(nameof(SizeDifferent));
            }
        }

        public bool DateDifferent
        {
            get => _dateDifferent;
            set
            {
                _dateDifferent = value;
                OnPropertyChanged(nameof(DateDifferent));
            }
        }

        public string SizeLeftFormatted => FormatFileSize(SizeLeft);
        public string SizeRightFormatted => FormatFileSize(SizeRight);

        public string DateLeftFormatted => DateLeft != DateTime.MinValue ? DateLeft.ToString("yyyy-MM-dd HH:mm:ss") : "-";
        public string DateRightFormatted => DateRight != DateTime.MinValue ? DateRight.ToString("yyyy-MM-dd HH:mm:ss") : "-";

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0)
                return "-";

            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblBytes = bytes;
            while (dblBytes >= 1024 && i < suffixes.Length - 1)
            {
                dblBytes /= 1024;
                i++;
            }
            return $"{dblBytes:0.##} {suffixes[i]}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}