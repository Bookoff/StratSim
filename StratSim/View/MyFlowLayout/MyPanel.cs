﻿using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.MyFlowLayout
{
    #region styles
    public enum FillStyles
    {
        None,
        FullWidth,
        FullHeight,
        FullScreen
    };

    public enum AutosizeTypes
    {
        Free,
        Constant,
        AutoWidth,
        AutoHeight
    };

    public enum DockTypes
    {
        TopLeft,
        Left,
        BottomLeft,
        Top,
        None,
        Bottom,
        TopRight,
        Right,
        BottomRight,
        Top2,
        Bottom2
    };
    #endregion

    /// <summary>
    /// The base class for a panel that is displayed on the window flow panel.
    /// Contains event handlers for events on the panel,
    /// and static methods for using the panel.
    /// Contains the base properties of the panel including those used for layout logic.
    /// </summary>
    public class MyPanel : Panel, IDockableControl
    {
        Panel header;
        Label titleLabel;
        PictureBox panelIconBox;
        Button hide, fullScreenToggle, close;
        MyContextMenu contextMenu;

        FillStyles windowFillStyle;
        AutosizeTypes autoSize;
        DockTypes windowDockType;
        Size originalSize;
        Point dockPointLocation;
        Type type;
        Padding padding;

        MainForm parentForm;
        Image icon;

        bool visible;
        bool removed;
        int panelIndex;

        bool inContentPanel;

        public const int controlSpacing = 4;

        /// <summary>
        /// <para>Starts a new instance of a panel to be displayed and resized dynamically
        /// on a window flow panel.</para>
        /// <para>Contains methods for setting up and manipulating the panels.</para>
        /// <para>After the panel is constructed, 'set panel properties' must be called.</para>
        /// </summary>
        /// <param name="width">The minimum width of the panel to be displayed</param>
        /// <param name="height">The minimum height of the panel to be displayed</param>
        /// <param name="title">The text to display in the header of the panel</param>
        /// <param name="ParentForm">The form on which the panel is to be displayed</param>
        /// <param name="Icon">A png file path representing the icon to be displayed in the header of the panel</param>
        public MyPanel(int width, int height, string title, MainForm ParentForm, Image Icon)
        {
            this.Width = width;
            this.Height = height;
            this.Name = title;

            icon = Icon;
            parentForm = ParentForm;

            visible = true;
            padding = new Padding(10, 25, 10, 10);
            DoubleBuffered = true;
            removed = true;
            BackColor = Color.White;

            FlowLayoutEvents.MainPanelResizeEnd += MyEvents_MainPanelResizeEnd;
            FlowLayoutEvents.MainPanelLayoutChanged += MyEvents_MainPanelLayoutChanged;
            PanelClosed += MyPanel_PanelClosed;
            PanelOpened += MyPanel_PanelOpened;

            SetupComponents(title);
            PositionComponents();
            this.type = typeof(MyPanel);
        }

        void MyPanel_PanelOpened()
        {
            removed = false;
        }

        void MyPanel_PanelClosed()
        {
            if (inContentPanel)
            {
                MyEvents.OnRemoveContentPanel(this.PanelIndex);
            }
            removed = true;
        }

        void MyEvents_MainPanelLayoutChanged()
        {
            contextMenu.SetCheckButtons();
        }

        void MyEvents_MainPanelResizeEnd()
        {
            PositionComponents();
        }

        /// <summary>
        /// Sets the panel's layout properties to the specified values.
        /// </summary>
        public void SetPanelProperties(DockTypes dockType, AutosizeTypes autosizeType, FillStyles fillStyle, Size size)
        {
            windowDockType = dockType;
            autoSize = autosizeType;
            windowFillStyle = fillStyle;
            originalSize = size;

            SetupContextMenu();
        }

        void SetupComponents(string title)
        {
            this.BorderStyle = BorderStyle.FixedSingle;

            header = new Panel();
            header.Location = new Point(0, 0);
            header.Size = new Size(this.Width, 25);
            header.BackgroundImage = Properties.Resources.Panel_Header;
            header.BackgroundImageLayout = ImageLayout.Stretch;
            header.MouseDown += header_MouseDown;
            header.MouseUp += header_MouseUp;

            panelIconBox = new PictureBox();
            panelIconBox.Image = icon;
            panelIconBox.Location = new Point(0, 0);
            panelIconBox.Size = new Size(16, 16);

            titleLabel = new Label();
            titleLabel.AutoSize = true;
            titleLabel.Text = title;
            titleLabel.Location = new Point(20, 0);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 43, 236);

            hide = new Button();
            hide.Size = new Size(17, 17);
            hide.FlatStyle = FlatStyle.Flat;
            hide.BackgroundImage = Properties.Resources.Hide_Display;
            hide.MouseHover += (s, e) => hide.BackgroundImage = Properties.Resources.Hide_Hover;
            hide.MouseLeave += (s, e) => hide.BackgroundImage = Properties.Resources.Hide_Display;
            hide.Click += ((s, e) => MyVisible = false);

            fullScreenToggle = new Button();
            fullScreenToggle.Size = new Size(17, 17);
            fullScreenToggle.FlatStyle = FlatStyle.Flat;
            fullScreenToggle.BackgroundImage = Properties.Resources.FullScreen_Display;
            fullScreenToggle.Click += fullScreenToggle_Click;

            close = new Button();
            close.Size = new Size(32, 17);
            close.FlatStyle = FlatStyle.Flat;
            close.BackgroundImage = Properties.Resources.Close_Display;
            close.MouseHover += (s, e) => close.BackgroundImage = Properties.Resources.Close_Hover;
            close.MouseLeave += (s, e) => close.BackgroundImage = Properties.Resources.Close_Display;
            close.Click += ((s, e) => Form.IOController.RemovePanel(this));

            header.Controls.Add(panelIconBox);
            header.Controls.Add(titleLabel);
            header.Controls.Add(hide);
            header.Controls.Add(fullScreenToggle);
            header.Controls.Add(close);

            this.Controls.Add(header);

            SetupContextMenu();
        }

        void fullScreenToggle_Click(object sender, EventArgs e)
        {
            if (windowFillStyle != FillStyles.FullScreen)
            {
                FillStyle = FillStyles.FullScreen;
            }
            else
            {
                FillStyle = FillStyles.None;
            }
        }

        void SetButtonImagesNotFullScreen()
        {
            fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
            fullScreenToggle.MouseHover += (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.FullScreen_Hover;
            fullScreenToggle.MouseLeave += (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
            fullScreenToggle.MouseHover -= (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Hover;
            fullScreenToggle.MouseLeave -= (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Display;
        }
        void SetButtonImagesIsFullScreen()
        {
            fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Display;
            fullScreenToggle.MouseHover += (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Hover;
            fullScreenToggle.MouseLeave += (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.RestoreDown_Display;
            fullScreenToggle.MouseHover -= (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.FullScreen_Hover;
            fullScreenToggle.MouseLeave -= (a, b) => fullScreenToggle.BackgroundImage = StratSim.Properties.Resources.FullScreen_Display;
        }

        void header_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && PanelDropped != null)
                PanelDropped(e.Location);
        }

        void header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (PanelSelected != null)
                    PanelSelected();

                Form.IOController.StartReLayout(this, e.Location);
            }
        }

        public void OnPanelOpened()
        {
            if (PanelOpened != null)
                PanelOpened();
        }

        public void OnPanelClosed()
        {
            if (PanelClosed != null)
                PanelClosed();
        }

        /// <summary>
        /// Removes the buttons displayed in the header, for use when the panel is displayed in a content panel
        /// </summary>
        public void RemoveToolbarButtons()
        {
            header.Controls.Remove(hide);
            header.Controls.Remove(fullScreenToggle);
            header.Controls.Remove(close);
            header.MouseDown -= header_MouseDown;
            header.MouseUp -= header_MouseUp;
        }

        /// <summary>
        /// Positions the components in the header after a re-size
        /// </summary>
        public void PositionComponents()
        {
            header.Size = new Size(this.ClientRectangle.Width, 25);

            int rightBorder = header.ClientRectangle.Right - 3;
            hide.Location = new Point(rightBorder - 66, 0);
            fullScreenToggle.Location = new Point(rightBorder - 49, 0);
            close.Location = new Point(rightBorder - 32, 0);
        }

        void SetupContextMenu()
        {
            contextMenu = new MyContextMenu(this);
            header.ContextMenuStrip = contextMenu;
        }

        /// <returns>True if the dock type is associated with the top of the panel</returns>
        public static bool IsDockedAtTop(DockTypes dockType)
        {
            bool dockedAtTop;

            if (dockType == DockTypes.TopLeft
                || dockType == DockTypes.Top
                || dockType == DockTypes.Top2
                || dockType == DockTypes.TopRight
                || dockType == DockTypes.Left
                || dockType == DockTypes.None)
            {
                dockedAtTop = true;
            }
            else
            {
                dockedAtTop = false;
            }

            return dockedAtTop;
        }

        /// <returns>True if the dock type is associated with the left of the panel</returns>
        public static bool IsDockedAtLeft(DockTypes dockType)
        {
            bool dockedAtLeft;

            if (dockType == DockTypes.TopLeft
                || dockType == DockTypes.Left
                || dockType == DockTypes.BottomLeft
                || dockType == DockTypes.Top
                || dockType == DockTypes.None
                || dockType == DockTypes.Bottom)
            {
                dockedAtLeft = true;
            }
            else
            {
                dockedAtLeft = false;
            }

            return dockedAtLeft;
        }

        public delegate void PanelClosedEventHandler();
        public event PanelClosedEventHandler PanelClosed;
        public delegate void PanelOpenedEventHandler();
        public event PanelOpenedEventHandler PanelOpened;

        public delegate void PanelDroppedEventHandler(Point Location);
        public event PanelDroppedEventHandler PanelDropped;

        public delegate void PanelSelectedEventHandler();
        public event PanelSelectedEventHandler PanelSelected;

        public void OnOpenedInDifferentForm(MainForm Form)
        {
            parentForm = Form;
            parentForm.IOController.AssociatedForm = Form;
            if (OpenedInNewForm != null)
                OpenedInNewForm(Form);
        }

        public delegate void NewFormEventHandler(MainForm NewForm);
        public event NewFormEventHandler OpenedInNewForm;

        public FillStyles FillStyle
        {
            get { return windowFillStyle; }
            set
            {
                if (windowFillStyle != FillStyles.FullScreen && value == FillStyles.FullScreen)
                {
                    SetButtonImagesIsFullScreen();
                }
                if (windowFillStyle == FillStyles.FullScreen && value != FillStyles.FullScreen)
                {
                    SetButtonImagesNotFullScreen();
                }
                windowFillStyle = value;
                FlowLayoutEvents.OnMainPanelLayoutChanged();
            }
        }
        public AutosizeTypes AutoSizeType
        {
            get { return autoSize; }
            set
            {
                autoSize = value;
                FlowLayoutEvents.OnMainPanelLayoutChanged();
            }
        }
        public DockTypes DockType
        {
            get { return windowDockType; }
            set
            {
                windowDockType = value;
                FlowLayoutEvents.OnMainPanelLayoutChanged();
            }
        }
        public Type Type
        {
            get { return type; }
        }
        public Size OriginalSize
        {
            get { return originalSize; }
            set { originalSize = value; }
        }
        /// <summary>
        /// Gets or sets the location of the point that the panel was docked to.
        /// </summary>
        public Point DockPointLocation
        {
            get { return dockPointLocation; }
            set { dockPointLocation = value; }
        }
        public Padding MyPadding
        {
            get { return padding; }
            set { padding = value; }
        }
        public bool MyVisible
        {
            get { return visible; }
            set
            {
                visible = value;
                FlowLayoutEvents.OnMainPanelLayoutChanged();
            }
        }
        public bool Removed
        {
            get { return removed; }
            set { removed = value; }
        }
        public int PanelIndex
        {
            get { return panelIndex; }
            set { panelIndex = value; }
        }

        public MainForm Form
        {
            get { return parentForm; }
            set { parentForm = value; }
        }

        public bool InContentPanel
        {
            get { return inContentPanel; }
            set { inContentPanel = value; }
        }

        public Image PanelIcon
        { get { return icon; } }
    }
}
