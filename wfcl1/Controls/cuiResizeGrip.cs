using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [Description("Bring back resizing for FormBorderStyle.None forms")]
    [ToolboxBitmap(typeof(Form))]

    public partial class cuiResizeGrip : UserControl
    {
        Point lastMousePoint = new Point(-1, -1);

        Form privateTargetForm;

        [Category("CuoreUI")]
        public Form TargetForm
        {
            get
            {
                return privateTargetForm;
            }
            set
            {
                privateTargetForm = value;
            }
        }

        private Color privateGripColor = Color.Gray;

        [Category("CuoreUI")]
        public Color GripColor
        {
            get
            {
                return privateGripColor;
            }
            set
            {
                privateGripColor = value;
                Refresh();
            }
        }

        private bool privateGripTexture = true;

        [Category("CuoreUI")]
        public bool GripTexture
        {
            get
            {
                return privateGripTexture;
            }
            set
            {
                privateGripTexture = value;
                Refresh();
            }
        }

        Timer dragTimer = new Timer();

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_LBUTTON = 0x01;

        public cuiResizeGrip()
        {
            InitializeComponent();

            Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            dragTimer.Tick += DragTimer_Tick;
            Size = new Size(24, 24);
            Cursor = Cursors.SizeNWSE;

            Timer refreshTimer = new Timer();
            refreshTimer.Interval = 1000;
            refreshTimer.Start();
            refreshTimer.Tick += (e, s) =>
            {
                dragTimer.Interval = 1000 / CuoreUI.Drawing.GetHighestRefreshRate();
            };
        }

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (TargetForm != null)
                {
                    bool isLeftButtonDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
                    if (isLeftButtonDown == false)
                    {
                        dragTimer.Stop();
                        return;
                    }

                    if (lastMousePoint == new Point(-1, -1))
                    {
                        lastMousePoint = Cursor.Position;
                    }

                    Point currentMousePoint = Cursor.Position;
                    Point mouseDelta = GetDelta(currentMousePoint, lastMousePoint);
                    lastMousePoint = currentMousePoint;
                    TargetForm.Size = Size.Subtract(TargetForm.Size, (Size)mouseDelta);

                }
            }
            catch (NullReferenceException)
            {

            }
        }

        private static Point GetDelta(Point p1, Point p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;

            return new Point((int)deltaX, (int)deltaY);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            lastMousePoint = new Point(-1, -1);

            dragTimer.Interval = 1000 / CuoreUI.Drawing.GetHighestRefreshRate();
            dragTimer.Stop();
            dragTimer.Start();
        }

        private Size privateTextureOffset = new Size(-2, -2);

        [Category("CuoreUI")]
        public Size TextureOffset
        {
            get
            {
                return privateTextureOffset;
            }
            set
            {
                privateTextureOffset = value;
                Refresh();
            }
        }

        GraphicsPath SquareGripPath(int size)
        {
            int halfSize = size;
            size *= 2;
            GraphicsPath gp = new GraphicsPath();

            void CreateAddRect(int x, int y)
            {
                gp.AddRectangle(new Rectangle(x + TextureOffset.Width, y + TextureOffset.Height, halfSize, halfSize));
            }

            if (!SkipBottomRightSquare)
            {
                CreateAddRect(Width - size, Height - size); // b r
            }

            CreateAddRect(Width - size, Height - (size * 2)); // 2/3b r

            CreateAddRect(Width - size, Height - (size * 3)); // 1/3b r

            CreateAddRect(Width - (size * 2), Height - size); // b 2/3b

            CreateAddRect(Width - (size * 3), Height - size); // b 1/3b

            CreateAddRect(Width - (size * 2), Height - (size * 2)); // 2/3b 2/3b

            return gp;
        }

        private int privateGripSize = 2;

        [Category("CuoreUI")]
        public int GripSize
        {
            get
            {
                return privateGripSize;
            }
            set
            {
                privateGripSize = value;
                Refresh();
            }
        }

        private bool privateSkipBottomRightSquare = false;

        [Category("CuoreUI")]
        public bool SkipBottomRightSquare
        {
            get
            {
                return privateSkipBottomRightSquare;
            }
            set
            {
                privateSkipBottomRightSquare = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (GripTexture)
            {
                using (SolidBrush br = new SolidBrush(GripColor))
                {
                    using (GraphicsPath GP = SquareGripPath(GripSize))
                    {
                        //Region = new Region(GP); // mask support at cost of more user mouse precision needed to drag the resizer
                        e.Graphics.FillPath(br, GP);
                    }
                }
            }

            if (TargetForm != null)
            {
                Location = new Point(TargetForm.Size.Width - Width, TargetForm.Size.Height - Height);
            }
        }
    }
}
