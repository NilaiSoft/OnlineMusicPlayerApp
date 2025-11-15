using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OnlineMusicPlayerApp.Models
{
    public class Category
    {
        public string Master { get; set; }
        public List<Detail> Details { get; set; }
        public string AlbumImageSrc { get; set; }
    }

    //public class Detail
    //{
    //    public string Title { get; set; }
    //    public int Id { get; set; }
    //    public string Href { get; set; }
    //    public bool IsVisible { get; set; }
    //    public string TagImageSrc { get; set; }
    //    public string ListImageSrc { get; set; }
    //    public int ParentId { get; set; }
    //    public double CurrentSecond { get; set; }
    //    public bool IsPlay { get; set; }
    //    public List<Detail> Children { get; set; } = new List<Detail>();
    //}

    public class Detail : INotifyPropertyChanged
    {
        // -------------------------
        // پراپرتی‌های اصلی مدل
        // -------------------------
        public string Title { get; set; }
        public int Id { get; set; }
        public string Href { get; set; }
        public bool IsVisible { get; set; }
        public string TagImageSrc { get; set; }
        public string ListImageSrc { get; set; }
        public int ParentId { get; set; }
        public double CurrentSecond { get; set; }
        public bool IsPlay { get; set; }
        public List<Detail> Children { get; set; } = new List<Detail>();

        public bool IsDeleteVisible { get; set; } = true;
        // -------------------------
        // پراپرتی‌های جدید برای ProgressBar دانلود
        // -------------------------

        private bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                OnPropertyChanged();
            }
        }

        private double _downloadProgress;
        public double DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                _downloadProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DownloadProgressPercent));
            }
        }

        public string DownloadProgressPercent =>
            $"{(int)(_downloadProgress * 100)}٪";

        private string _downloadStatus;
        public string DownloadStatus
        {
            get => _downloadStatus;
            set
            {
                _downloadStatus = value;
                OnPropertyChanged();
            }
        }


        // -------------------------
        // پشتیبانی از Notify UI
        // -------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class CategoryWrapper
    {
        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }
    }
}
