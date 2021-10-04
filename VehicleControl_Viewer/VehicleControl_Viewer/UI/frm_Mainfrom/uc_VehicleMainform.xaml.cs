
using com.mirle.AK0.ProtocolFormat;
using GenericParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VehicleControl_Viewer.App;
using VehicleControl_Viewer.Data.Interface;
using VehicleControl_Viewer.frm_Help;
using VehicleControl_Viewer.UI.Components;
using VehicleControl_Viewer.Vo;
using VehicleControl_Viewer.Vo.ValueObjToShow;

namespace VehicleControl_Viewer.frm_Mainfrom
{
    /// <summary>
    /// uc_AGC_MCS_Mainform.xaml 的互動邏輯
    /// </summary>
    public partial class uc_VehicleMainform : UserControl
    {
        #region 全域變數
        private static System.Windows.Threading.DispatcherTimer positionUpdat3Timer;
        private DataSet ohxcConfig = null;
        public DataSet OHxCConfig { get { return ohxcConfig; } }
        List<Address> addresses;
        List<Section> sections;
        List<VehicleInfo> vehicleInfos;
        List<VehicleShow> vehicleShows;
        ShapeCollection RailsCollection = null;
        ShapeCollection DisableRailsCollection = null;
        ShapeCollection TestGuideRailsCollection = null;
        object lock_guide_info_refresh = new object();
        Dictionary<VehicleInfo, ShapeCollection> GuideRailCollection = null;
        private readonly WindownApplication app;
        GridLength gl80 = new GridLength(80, GridUnitType.Pixel);
        GridLength gl250 = new GridLength(250, GridUnitType.Pixel);
        IVehicleCommand vehicleCommand = null;
        frm_TipMessage_OK MessageBox = new frm_TipMessage_OK();
        #endregion 全域變數


        public uc_VehicleMainform(WindownApplication _app)
        {
            InitializeComponent();
            ohxcConfig = new DataSet();
            RailsCollection = new ShapeCollection();
            DisableRailsCollection = new ShapeCollection();
            TestGuideRailsCollection = new ShapeCollection();
            GuideRailCollection = new Dictionary<VehicleInfo, ShapeCollection>();
            this.app = _app;

            vehicleCommand = app.VehicleControlService;
            tbx_ActionType.SelectedIndex = 0;
            initConfig();
            initialObj();
            initialPath();
            initialDisablePath();
            initialVhNew();
            initialDataGrid_VehicleStatus();
            initialTimer();
            initialActionCombobox();
            initialCommandEventTypeCombobox();
            initialPortAdrCombobox();

            registeredEvent();
        }

        private void initialPortAdrCombobox()
        {
            transferByAddress();
            int port_count = app.objCacheManager.PortsInfo.Count;
            if (port_count == 0)
            {
                rdo_transferByPort.Visibility = Visibility.Hidden;
            }
            rdo_transferByAdr.IsChecked = true;
        }

        private void transferByAddress()
        {
            txt_LPort.Text = "L Adr";
            txt_ULPort.Text = "T Adr";

            tbx_LPort.ItemsSource = addresses.ToList();
            tbx_LPort.DisplayMemberPath = "ID";
            tbx_LPort.SelectedValuePath = "ID";

            tbx_ULPort.ItemsSource = addresses.ToList();
            tbx_ULPort.DisplayMemberPath = "ID";
            tbx_ULPort.SelectedValuePath = "ID";
        }
        private void transferByPort()
        {
            txt_LPort.Text = "L Port";
            txt_ULPort.Text = "T Port";
            var all_port = app.objCacheManager.PortsInfo;
            tbx_LPort.ItemsSource = all_port.Values.ToList();
            tbx_LPort.DisplayMemberPath = "PortId";
            tbx_LPort.SelectedValuePath = "PortId";

            tbx_ULPort.ItemsSource = all_port.Values.ToList();
            tbx_ULPort.DisplayMemberPath = "PortId";
            tbx_ULPort.SelectedValuePath = "PortId";

        }

        private void initialCommandEventTypeCombobox()
        {
            tbx_ActionType.ItemsSource = Enum.GetValues(typeof(CommandEventType)).Cast<CommandEventType>().ToList();
        }

        private void initialActionCombobox()
        {
            var vhs = app.objCacheManager.LoadAllVehicles();
            tbx_vhID.ItemsSource = vhs;
            tbx_vhID.DisplayMemberPath = "VEHICLE_ID";
            tbx_vhID.SelectedValuePath = "VEHICLE_ID";
        }

        private void registeredEvent()
        {
            app.objCacheManager.RailStatusChanged += ObjCacheManager_RailStatusChanged;
            RailsCollection.AddressSelected += RailsCollection_AddressSelected;
        }

        bool isSourceSelected = false;
        private void RailsCollection_AddressSelected(object sender, EventArgs e)
        {
            if (rdo_transferByAdr.IsChecked == false)
                return;
            var ellipse = sender as Ellipse;
            if (ellipse == null) return;
            var adr = ellipse.Tag as Address;
            if (!isSourceSelected)
            {
                tbx_LPort.SelectedItem = adr;
                isSourceSelected = true;
            }
            else
            {
                tbx_ULPort.SelectedItem = adr;
                isSourceSelected = false;
            }
        }

        private void ObjCacheManager_RailStatusChanged(object sender, EventArgs e)
        {
            refreshRailStatus();
        }

        private void initialDisablePath()
        {
            setDisablePath();
        }

        private void setDisablePath()
        {
            SolidColorBrush Brush = new SolidColorBrush();
            Brush.Color = Color.FromRgb(0xFF, 0x00, 0x00);

            var disable_segments = app.objCacheManager.getDisableSegment();
            foreach (var segment in disable_segments)
            {
                var disable_secitons = sections.Where(sec => segment.SecIds.Contains(sec.ID)).ToList();
                if (disable_secitons == null || disable_secitons.Count == 0)
                    continue;
                foreach (var sec_obj in disable_secitons)
                {
                    DisableRailsCollection.AddLineSegment(this, sec_obj.ID, new Point(sec_obj.StartAddress.Point.X * 1, sec_obj.StartAddress.Point.Y),
                                                          new Point(sec_obj.EndAddress.Point.X * 1, sec_obj.EndAddress.Point.Y), Brush, false);
                }
            }
            foreach (var s in DisableRailsCollection.shapes)
            {
                VehicleTrack.Children.Add(s);
            }
        }

        private void refreshRailStatus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DisableRailsCollection.shapes.ForEach(s => VehicleTrack.Children.Remove(s));
                DisableRailsCollection.shapes.Clear();
                setDisablePath();
            }));
        }

        private void initialDataGrid_VehicleStatus()
        {
            vehicleShows = new List<VehicleShow>();
            var vhs = app.objCacheManager.LoadAllVehicles();
            foreach (var vh in vhs)
            {
                vehicleShows.Add(new VehicleShow(vh));
            }
            dgv_VehicleStatus.ItemsSource = vehicleShows;
        }

        private void initialTimer()
        {
            positionUpdat3Timer = new System.Windows.Threading.DispatcherTimer();
            positionUpdat3Timer.Tick += new EventHandler(timeCycle);
            positionUpdat3Timer.Interval = new TimeSpan(0, 0, 0, 1);
            positionUpdat3Timer.Start();
        }
        public void timeCycle(object sender, EventArgs e)
        {

            vehicleInfos.ForEach(vh =>
            {
                vh.resreshPosition();
                vh.resreshStatus();
            });
            vehicleShows.ForEach(vh => vh.resresh());

            var transfer_command_infos = app.objCacheManager.TransferCommandInfos.Select(tran => new Vo.ObjToShow.TransferCommandShow(tran));
            dgv_TransferCommand.ItemsSource = transfer_command_infos;
            dgv_TaskCommand.ItemsSource = app.objCacheManager.TaskCommandInfos;
        }

        private void initialObj()
        {
            addresses = loadAddresss();
            sections = loadASection();
        }
        private void initialVhNew()
        {
            vehicleInfos = new List<VehicleInfo>();
            var vhs = app.objCacheManager.LoadAllVehicles();
            foreach (var vh in vhs)
            {
                VehicleInfo vh_info = new VehicleInfo(vh);

                Storyboard.SetTarget(vh_info.doubleAnimation_x, vh_info.vhPresenter);
                Storyboard.SetTarget(vh_info.doubleAnimation_y, vh_info.vhPresenter);
                Storyboard.SetTargetProperty(vh_info.doubleAnimation_x, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
                Storyboard.SetTargetProperty(vh_info.doubleAnimation_y, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));

                vehicleInfos.Add(vh_info);
                VehicleTrack.Children.Add(vh_info.vhPresenter);
                vh_info.vhPresenter.MouseDoubleClick += VhPresenter_MouseDoubleClick;

                Canvas.SetZIndex(vh_info.vhPresenter, int.MaxValue);
            }
        }

        private void VhPresenter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var content = sender as ContentControl;
            var vh_info = content.Content as VehicleInfo;
            lock (lock_guide_info_refresh)
            {
                tbx_vhID.SelectedItem = vh_info.vh;
                SolidColorBrush Brush = new SolidColorBrush();
                Brush.Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);

                var show_obj = vehicleShows.Where(o => o.VEHICLE_ID == content.Name).FirstOrDefault();
                //int selected_index = vehicleShows.IndexOf(show_obj);
                dgv_VehicleStatus.SelectedItem = show_obj;
                dgv_VehicleStatus.ScrollIntoView(show_obj);

                resertGuideReail();
                var guide_rail = new ShapeCollection();
                var will_pass_section = vh_info.getWillPassSection();
                if (will_pass_section == null || will_pass_section.Count == 0) return;
                foreach (string sec in will_pass_section)
                {
                    var sec_obj = sections.Where(s => s.ID == sec).FirstOrDefault();
                    guide_rail.AddLineSegment(this, sec, new Point(sec_obj.StartAddress.Point.X * 1, sec_obj.StartAddress.Point.Y),
                                               new Point(sec_obj.EndAddress.Point.X * 1, sec_obj.EndAddress.Point.Y), Brush, will_pass_section.Last() == sec);
                }
                GuideRailCollection.Add(vh_info, guide_rail);
                guide_rail.shapes.ForEach(s => VehicleTrack.Children.Add(s));
            }
        }

        private void resertGuideReail()
        {
            if (GuideRailCollection == null || GuideRailCollection.Count() == 0)
            {
                return;
            }
            foreach (var guide_info in GuideRailCollection)
            {
                guide_info.Value.shapes.ForEach(s => VehicleTrack.Children.Remove(s));
            }
            GuideRailCollection.Clear();
        }

        public List<Address> loadAddresss()
        {
            try
            {
                DataTable dt = OHxCConfig.Tables["AADDRESS"];
                var query = from c in dt.AsEnumerable()
                            select new Address(c.Field<string>("Id"), startToDouble(c.Field<string>("PositionX")), startToDouble(c.Field<string>("PositionY")));
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Section> loadASection()
        {
            try
            {
                DataTable dt = OHxCConfig.Tables["ASECTION"];
                var query = from c in dt.AsEnumerable()
                            select new Section(addresses, c.Field<string>("Id"), c.Field<string>("FromAddress"), c.Field<string>("ToAddress"));
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void initConfig()
        {
            ohxcConfig = new DataSet();
            loadCSVToDataset(ohxcConfig, "AADDRESS");
            loadCSVToDataset(ohxcConfig, "ASECTION");
        }
        private void loadCSVToDataset(DataSet ds, string tableName)
        {
            using (GenericParser parser = new GenericParser())
            {
                parser.SetDataSource(Environment.CurrentDirectory + @"\Config\MapInfo\" + tableName + ".csv", System.Text.Encoding.Default);
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                //parser.SkipStartingDataRows = 1;
                parser.MaxBufferSize = 1024;
                //parser.MaxRows = 500;
                //parser.TextQualifier = '\"';


                DataTable dt = new System.Data.DataTable(tableName);

                bool isfirst = true;
                while (parser.Read())
                {

                    int cs = parser.ColumnCount;
                    if (isfirst)
                    {

                        for (int i = 0; i < cs; i++)
                        {
                            dt.Columns.Add(parser.GetColumnName(i), typeof(string));
                        }
                        isfirst = false;
                    }


                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < cs; i++)
                    {
                        string val = parser[i];
                        //ALARM 要可以接受 16進制的 2015.02.23 by Kevin Wei
                        //if (dt.Columns[i] != null && BCFUtility.isMatche(dt.Columns[i].ColumnName, "ALARM_ID"))
                        //{
                        //    int valInt = Convert.ToInt32(val);
                        //    val = val;
                        //}
                        dr[i] = val;
                        //                        dr[i] = parser[i];
                    }
                    dt.Rows.Add(dr);
                }
                ds.Tables.Add(dt);
            }
        }
        private double startToDouble(string value)
        {
            double.TryParse(value, out double result);
            return result;
        }

        private void initialPath()
        {
            double max_x = 0;
            double max_y = 0;


            foreach (var sec in sections)
            {
                var start_adr_obj = sec.StartAddress;
                var end_adr_obj = sec.EndAddress;
                if (start_adr_obj == null || end_adr_obj == null) continue;
                RailsCollection.AddLineSegment(this, sec.ID, new Point(sec.StartAddress.Point.X * 1, sec.StartAddress.Point.Y),
                                           new Point(sec.EndAddress.Point.X * 1, sec.EndAddress.Point.Y));
            }
            foreach (var adr in addresses)
            {
                double t_x = (adr.Point.X);
                double t_y = (adr.Point.Y);
                RailsCollection.AddEllipse(this, adr, new Point(t_x, t_y));
                if (t_x > max_x)
                    max_x = t_x;
                if (t_y > max_y)
                    max_y = t_y;
                Console.WriteLine($"adr id:{adr.ID},x:{t_x}");
            }
            VehicleTrack.Width = max_x;
            VehicleTrack.Height = max_y;
            //VehicleTrack.RenderTransform = new ScaleTransform(PathEnhance.Scale, PathEnhance.Scale);
            foreach (var s in RailsCollection.shapes)
            {
                VehicleTrack.Children.Add(s);
                if (s is Ellipse)
                {
                    Canvas.SetZIndex(s, int.MaxValue - 1);
                }
            }
        }


        public void Start()
        {
            try
            {

            }
            catch (Exception ex)
            {
            }
        }


        object setTestGuideTail_Sync = new object();
        public void setTestGideRail(IEnumerable<string> guideSection)
        {
            lock (setTestGuideTail_Sync)
            {
                SolidColorBrush Brush = new SolidColorBrush();
                Brush.Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);

                foreach (var s in TestGuideRailsCollection.shapes)
                {
                    VehicleTrack.Children.Remove(s);
                }
                TestGuideRailsCollection.shapes.Clear();
                foreach (var sec_id in guideSection)
                {
                    var sec_obj = sections.Where(sec => sec.ID == sec_id.Trim()).FirstOrDefault();
                    if (sec_obj == null)
                    {

                        continue;
                    }
                    TestGuideRailsCollection.AddLineSegment(this, sec_id, new Point(sec_obj.StartAddress.Point.X * 1, sec_obj.StartAddress.Point.Y),
                                                          new Point(sec_obj.EndAddress.Point.X * 1, sec_obj.EndAddress.Point.Y), Brush, false);
                }
                foreach (var s in TestGuideRailsCollection.shapes)
                {
                    VehicleTrack.Children.Add(s);
                }
            }
        }

        class ShapeCollection
        {
            public EventHandler AddressSelected;
            SolidColorBrush mySolidColorBrush_ForPoint = new SolidColorBrush();
            SolidColorBrush mySolidColorBrush_ForRail = new SolidColorBrush();
            public List<Shape> shapes = null;
            public ShapeCollection()
            {
                //mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
                //mySolidColorBrush.Color = Color.FromArgb(int.Parse("FF0080FF ", NumberStyles.AllowHexSpecifier));
                mySolidColorBrush_ForRail.Color = Color.FromArgb(0xFF, 0, 0x80, 0xFF);
                mySolidColorBrush_ForPoint.Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                shapes = new List<Shape>();
            }
            public void AddEllipse(FrameworkElement frameworkElement, Address adr, Point point)
            {
                Ellipse myEllipse = new Ellipse();
                myEllipse.Fill = mySolidColorBrush_ForPoint;
                myEllipse.StrokeThickness = 10;
                myEllipse.Stroke = mySolidColorBrush_ForRail;
                myEllipse.Width = 400;
                myEllipse.Height = 400;
                double left = point.X - (myEllipse.Width / 2); double top = point.Y - (myEllipse.Height / 2);
                myEllipse.Margin = new Thickness(left, top, 0, 0);
                myEllipse.Cursor = Cursors.Hand;
                myEllipse.MouseDown += MyEllipse_MouseDown;
                myEllipse.Tag = adr;
                var t = new ToolTip();
                t.Style = (Style)frameworkElement.FindResource("MaterialDesignToolTip");
                ToolTipService.SetInitialShowDelay(t, 0);
                t.Content = adr.ID;
                myEllipse.ToolTip = t;

                shapes.Add(myEllipse);
            }

            private void MyEllipse_MouseDown(object sender, MouseButtonEventArgs e)
            {
                AddressSelected?.Invoke(sender, EventArgs.Empty);
            }

            public void AddLineSegment(FrameworkElement frameworkElement, string secID, Point startPoint, Point endPoint)
            {
                Line myLine = new Line();
                //myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.Stroke = mySolidColorBrush_ForRail;
                myLine.X1 = startPoint.X;
                myLine.X2 = endPoint.X;
                myLine.Y1 = startPoint.Y;
                myLine.Y2 = endPoint.Y;
                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 200;
                var t = new ToolTip();
                ToolTipService.SetInitialShowDelay(t, 0);
                t.Content = secID;
                t.Style = (Style)frameworkElement.FindResource("MaterialDesignToolTip");
                myLine.ToolTip = t;
                shapes.Add(myLine);
            }
            public void AddLineSegment(FrameworkElement frameworkElement, string secID, Point startPoint, Point endPoint, SolidColorBrush brush, bool isFinal)
            {
                Line myLine = new Line();
                //myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.Stroke = brush;
                myLine.X1 = startPoint.X;
                myLine.X2 = endPoint.X;
                myLine.Y1 = startPoint.Y;
                myLine.Y2 = endPoint.Y;
                myLine.HorizontalAlignment = HorizontalAlignment.Left;
                myLine.VerticalAlignment = VerticalAlignment.Center;
                myLine.StrokeThickness = 200;
                var t = new ToolTip();
                ToolTipService.SetInitialShowDelay(t, 0);
                t.Content = secID;
                t.Style = (Style)frameworkElement.FindResource("MaterialDesignToolTip");
                myLine.ToolTip = t;

                if (isFinal)
                    myLine.StrokeEndLineCap = PenLineCap.Triangle;


                shapes.Add(myLine);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void VehicleTrack_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            lock (lock_guide_info_refresh)
            {
                resertGuideReail();
            }
        }

        private void Img_extendMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            gc_sideMenu.Width = (gc_sideMenu.Width == gl80) ? gl250 : gl80;
            if (gc_sideMenu.Width == gl80) { sp_vehicleCommandAction.Visibility = Visibility.Hidden; }
            else sp_vehicleCommandAction.Visibility = Visibility.Visible;
        }

        private async void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            var com_info = getVehicleTranCommandInfo();
            var result = await vehicleCommand.RequestTrnsferAsync(com_info);
            if (result.Result == Result.Ok)
            {
                MessageBox.setMessage("Send command to vh success.");
            }
            else
            {
                MessageBox.setMessage($"Send command to vh Fail,{Environment.NewLine}Reson:{result.Reason}");
            }
            MessageBox.ShowDialog();

        }

        private VehicleCommandInfo getVehicleTranCommandInfo()
        {

            Enum.TryParse<CommandEventType>(tbx_ActionType.SelectedValue.ToString(), out var cmd_type);
            string from_id = "";
            string to_id = "";
            string cst_id = "";
            switch (cmd_type)
            {
                case CommandEventType.Move:
                    to_id = tbx_ULPort.Text;
                    break;
                case CommandEventType.Load:
                    from_id = tbx_LPort.Text;
                    cst_id = tbx_cstID.Text;
                    break;
                case CommandEventType.Unload:
                    from_id = tbx_ULPort.Text;
                    cst_id = tbx_cstID.Text;
                    break;
                case CommandEventType.LoadUnload:
                    from_id = tbx_LPort.Text;
                    to_id = tbx_ULPort.Text;
                    cst_id = tbx_cstID.Text;
                    break;
            }
            return new VehicleCommandInfo()
            {
                VhId = tbx_vhID.Text,
                Type = cmd_type,
                CarrierId = cst_id,
                FromPortId = from_id,
                ToPortId = to_id
            };
        }

        private void rdo_transferByAdr_Checked(object sender, RoutedEventArgs e)
        {
            transferByAddress();
        }

        private void rdo_transferByPort_Checked(object sender, RoutedEventArgs e)
        {
            transferByPort();
        }
    }
}
