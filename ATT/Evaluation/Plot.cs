﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAIR.Collections.Generic;
using LAIR.Extensions;
using System.Drawing;
using System.IO;

namespace PTL.ATT.Evaluation
{
    [Serializable]
    public abstract class Plot
    {
        public enum Format
        {
            JPEG,
            EPS
        }

        private Dictionary<string, List<PointF>> _seriesPoints;
        private string _title;
        private Image _image;
        private Format _imageFormat;
        private string _imagePath;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Plot titles cannot be null");

                if (value != _title)
                {
                    _title = value;
                    if (_image != null)
                        Render(_image.Height, _image.Width, true, false, false, false);
                }
            }
        }

        public Dictionary<string, List<PointF>> SeriesPoints
        {
            get { return _seriesPoints; }
            set { _seriesPoints = value; }
        }

        public Image Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public Format ImageFormat
        {
            get { return _imageFormat; }
            set { _imageFormat = value; }
        }

        public string ImagePath
        {
            get { return _imagePath; }
        }

        protected Plot(string title, Dictionary<string, List<PointF>> seriesPoints, int height, int width, Format format)
        {
            _title = title;
            _seriesPoints = seriesPoints;
            _imageFormat = format;

            Render(height, width, true, false, false, false);
        }

        protected Plot(string title, Dictionary<string, List<PointF>> seriesPoints, Image image, Format format)
        {
            _title = title;
            _seriesPoints = seriesPoints;
            _image = image;
            _imageFormat = format;

            if (_image == null)
                throw new NullReferenceException("Passed null image to plot constructor");
        }

        public void Render(int height, int width, bool includeTitle, bool plotSeriesDifference, bool blackAndWhite, bool retainImageOnDisk, params string[] args)
        {
            _imagePath = CreateImageOnDisk(height, width, includeTitle, plotSeriesDifference, blackAndWhite, args);

            // must create from file then copy to memory in order to delete file
            Image image = null;
            if (ImageFormat == Format.JPEG)
            {
                MemoryStream mem = new MemoryStream();
                using (Image fileImage = Image.FromFile(_imagePath))
                {
                    fileImage.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
                }

                mem.Position = 0;
                image = Image.FromStream(mem);

                _image = image;
            }

            if (!retainImageOnDisk)
            {
                File.Delete(_imagePath);
                _imagePath = null;
            }
        }

        protected abstract string CreateImageOnDisk(int height, int width, bool includeTitle, bool plotSeriesDifference, bool blackAndWhite, params string[] args);
    }
}