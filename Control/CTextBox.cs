using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Trainer.net.Control
{
    public class CTextBox : TextBox
    {
        #region Fields

        #region Protected Fields

        protected Color WaterMarkActiveColor; //Color of the watermark when the control has focus
        protected Color WaterMarkColor; //Color of the watermark when the control does not have focus
        protected string WaterMarkText = "Default Watermark..."; //The watermark text

        #endregion

        #region Private Fields

        private SolidBrush _waterMarkBrush; //Brush for the watermark
        private Panel _waterMarkContainer; //Container to hold the watermark
        private Font _waterMarkFont; //Font of the watermark

        #endregion

        #endregion

        #region Constructors

        public CTextBox()
        {
            Initialize();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Initializes watermark properties and adds CtextBox events
        /// </summary>
        private void Initialize()
        {
            //Sets some default values to the watermark properties
            WaterMarkColor = Color.LightGray;
            WaterMarkActiveColor = Color.Gray;
            _waterMarkFont = Font;
            _waterMarkBrush = new SolidBrush(WaterMarkActiveColor);
            _waterMarkContainer = null;

            //Draw the watermark, so we can see it in design time
            DrawWaterMark();

            //Eventhandlers which contains function calls. 
            //Either to draw or to remove the watermark
            Enter += ThisHasFocus;
            Leave += ThisWasLeaved;
            TextChanged += ThisTextChanged;
        }

        /// <summary>
        ///     Removes the watermark if it should
        /// </summary>
        private void RemoveWaterMark()
        {
            if (_waterMarkContainer != null)
            {
                Controls.Remove(_waterMarkContainer);
                _waterMarkContainer = null;
            }
        }

        /// <summary>
        ///     Draws the watermark if the text length is 0
        /// </summary>
        private void DrawWaterMark()
        {
            if (_waterMarkContainer == null && TextLength <= 0)
            {
                _waterMarkContainer = new Panel(); // Creates the new panel instance
                _waterMarkContainer.Paint += waterMarkContainer_Paint;
                _waterMarkContainer.Invalidate();
                _waterMarkContainer.Click += waterMarkContainer_Click;
                Controls.Add(_waterMarkContainer); // adds the control
            }
        }

        #endregion

        #region Eventhandlers

        #region WaterMark Events

        private void waterMarkContainer_Click(object sender, EventArgs e)
        {
            Focus();
        }

        private void waterMarkContainer_Paint(object sender, PaintEventArgs e)
        {
            _waterMarkContainer.Location = new Point(2, 0);
            _waterMarkContainer.Height = Height;
            _waterMarkContainer.Width = Width;
            _waterMarkContainer.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            _waterMarkBrush = ContainsFocus ? new SolidBrush(WaterMarkActiveColor) : new SolidBrush(WaterMarkColor);

            Graphics g = e.Graphics;
            g.DrawString(WaterMarkText, _waterMarkFont, _waterMarkBrush, new PointF(-2f, 1f));
        }

        #endregion

        #region CTextBox Events

        private void ThisHasFocus(object sender, EventArgs e)
        {
            _waterMarkBrush = new SolidBrush(WaterMarkActiveColor);

            if (TextLength <= 0)
            {
                RemoveWaterMark();
                DrawWaterMark();
            }
        }

        private void ThisWasLeaved(object sender, EventArgs e)
        {
            if (TextLength > 0)
            {
                RemoveWaterMark();
            }
            else
            {
                Invalidate();
            }
        }

        private void ThisTextChanged(object sender, EventArgs e)
        {
            if (TextLength > 0)
            {
                RemoveWaterMark();
            }
            else
            {
                DrawWaterMark();
            }
        }

        #region Overrided Events

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawWaterMark();
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            if (_waterMarkContainer != null)
                _waterMarkContainer.Invalidate();
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        [Category("Watermark attribtues")]
        [Description("Sets the text of the watermark")]
        public string WaterMark
        {
            get { return WaterMarkText; }
            set
            {
                WaterMarkText = value;
                Invalidate();
            }
        }

        [Category("Watermark attribtues")]
        [Description("When the control gaines focus, this color will be used as the watermark's forecolor")]
        public Color WaterMarkActiveForeColor
        {
            get { return WaterMarkActiveColor; }

            set
            {
                WaterMarkActiveColor = value;
                Invalidate();
            }
        }

        [Category("Watermark attribtues")]
        [Description("When the control looses focus, this color will be used as the watermark's forecolor")]
        public Color WaterMarkForeColor
        {
            get { return WaterMarkColor; }

            set
            {
                WaterMarkColor = value;
                Invalidate();
            }
        }

        [Category("Watermark attribtues")]
        [Description("The font used on the watermark. Default is the same as the control")]
        public Font WaterMarkFont
        {
            get { return _waterMarkFont; }

            set
            {
                _waterMarkFont = value;
                Invalidate();
            }
        }

        #endregion
    }
}