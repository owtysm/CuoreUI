using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Svg;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(PictureBox))]
    public partial class cuiSvgBox : Control
    {
        public cuiSvgBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
        }

        string GetSVGLocation()
        {
            string current = Environment.CurrentDirectory;
            string foldername = "\\CuoreUI";
            string combinedFolders = current + foldername;
            if (Directory.Exists(combinedFolders) == false)
            {
                Directory.CreateDirectory(combinedFolders);
            }

            string filename = "\\" + Name + "_cuore.svg";
            string combinedFile = combinedFolders + filename;

            return combinedFile;
        }

        private string[] privateSvgCode = new string[] { "" };
        public string[] SvgCode
        {
            get
            {
                return privateSvgCode;
            }
            set
            {
                privateSvgCode = value;
                OverrideStroke = OverrideStroke;
                OverrideFill = OverrideFill;
            }
        }

        public string[] RunningSvgCode
        {
            get;
            private set;
        } = new string[] { "" };

        private async void SaveSVG(bool alternativeStroke, bool alternativeFill)
        {
            string path = GetSVGLocation();
            string[] tempSVGCODE = SvgCode;

            if (tempSVGCODE.Contains("class="))
            {
                SvgCode = new string[] { "" };
                RunningSvgCode = new string[] { "" };
                return;
            }

            if (alternativeStroke && OverrideStroke != Color.Empty)
            {
                string strokeColor = ColorToHexString(OverrideStroke);

                string strokeRegex = @"stroke=""([^""]+)""";

                for (int i = 0; i < tempSVGCODE.Length; i++)
                {
                    tempSVGCODE[i] = Regex.Replace(tempSVGCODE[i], strokeRegex, $"stroke=\"{strokeColor}\"");
                }
            }

            if (alternativeFill && OverrideFill != Color.Empty)
            {
                string fillColor = ColorToHexString(OverrideFill);

                string fillRegex = @"fill=""([^""]+)""";

                for (int i = 0; i < tempSVGCODE.Length; i++)
                {
                    tempSVGCODE[i] = Regex.Replace(tempSVGCODE[i], fillRegex, $"fill=\"{fillColor}\"");
                }
            }

            RunningSvgCode = tempSVGCODE;

            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                foreach (var line in tempSVGCODE)
                {
                    await writer.WriteLineAsync(line);
                }
            }

            Invalidate();
        }

        string ColorToHexString(Color color)
        {
            string r = color.R.ToString("x2");
            string g = color.G.ToString("x2");
            string b = color.B.ToString("x2");

            return "#" + r + g + b;
        }

        private Color privateOverrideStroke = Color.Empty;
        public Color OverrideStroke
        {
            get
            {
                return privateOverrideStroke;
            }
            set
            {
                if (value == Color.Transparent || value == Color.Empty)
                {
                    value = BackColor;
                }
                privateOverrideStroke = value;
                SaveSVG(true, false);
            }
        }

        private Color privateOverrideFill = Color.Empty;
        public Color OverrideFill
        {
            get
            {
                return privateOverrideFill;
            }
            set
            {
                if (value == Color.Transparent || value == Color.Empty)
                {
                    value = BackColor;
                }
                privateOverrideFill = value;
                SaveSVG(false, true);
            }
        }

        Bitmap bitmap;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            string svgLocation = GetSVGLocation();
            if (!File.Exists(svgLocation))
            {
                return;
            }

            if (RunningSvgCode.Length > 0)
            {
                try
                {
                    SvgDocument svgDocument = SvgDocument.Open(svgLocation);

                    svgDocument.Width = ClientRectangle.Width;
                    svgDocument.Height = ClientRectangle.Height;

                    bitmap = svgDocument.Draw();
                    e.Graphics.DrawImage(bitmap, Point.Empty);
                }
                catch
                {
                    e.Graphics.Clear(BackColor);
                }
            }

            base.OnPaint(e);
        }
    }
}
