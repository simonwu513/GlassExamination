using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using GlassExamination;
using Newtonsoft.Json.Bson;


namespace GlassExamination
{
    public partial class GlassExamination : Form
    {
        // 視窗物件
        private Panel markerPanel;
        private int PanelItemHeight = 30;
        private int PanelItemWidth;

        // 設定視窗大小
        private int formWidth;
        private int formHeight;
        
        // 登入的user
        string user = "Simon";

        // 紀錄標記點在原始圖片的絕對座標
        private BindingList<Point> markers;
        private BindingList<Point> markersToShow;

        // 圖片縮放的功能
        private int zoomCount = 1; // 初始的縮放比例
        private readonly float zoomFactor = 2f; //每次縮放的倍數
        private readonly float zoomInLimitCount = 6; //放大的次數上限

        // 原始圖片的特性
        private Image orignialImage;
        private Point origin = new Point(0,0); // 因為SizeMode = Normal

        // 最初因pictureBox比較小而縮放的比例
        private float pbShrinkRatio;

        // 隨圖片縮放，會更改的變數        
        private Point offset; // 圖片的左上角原點，當放大或縮小時會產生偏移
        private Point lastClickPoint; // 紀錄放大或縮小時點擊的絕對位置
        private int newWidth;
        private int newHeight;
        private float zoomMultiple; // 相對於原始圖片，縮放的倍數

        // 滑鼠游標圖案設定
        private Cursor zoomInCursor;
        private Cursor zoomOutCursor;
        private Cursor defaultCursor;

        // 當觸發滑鼠點擊事件後一秒內不能再次觸發
        private bool isHandlingMouseEvent = false;

        // 滑鼠是否拖曳
        private bool isDragging = false;
        private Point lastMousePos; // 紀錄拖曳最初按的點的座標

        // 記住當前選取的lstGlass item，若用戶在點擊其他item之前未儲存markers, 則跳出提示視窗請用戶卻定儲存
        private int previousImageId = 0; //無圖片，若有圖片ID從1開始
        private bool previousMarkersSaved = false;

        // 資料庫存取用
        private readonly string conn = "Server=.;Database=GlassExamination;Integrated Security=True;Encrypt=False;";
        private DataTable dt; 

        public GlassExamination()
        {
            InitializeComponent();
            markers = new BindingList<Point>();
            dt = new DataTable();

            // 滑鼠游標圖案設定
            zoomInCursor = CreateCursor("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\Images\\zoomin.ico",25,25); 
            zoomOutCursor = CreateCursor("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\Images\\zoomout.ico", 25, 25); 
            defaultCursor = Cursors.Default;

            // 取得螢幕解析度
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // 設定視窗為螢幕大小的 80%
            formWidth = (int)(screenWidth * 0.8);
            formHeight = (int)(screenHeight * 0.8);

            this.Width = formWidth;
            this.Height = formHeight;

            // 設定圖片框為視窗大小的 80%
            pictureBox1.Width = (int)(formWidth * 0.8);
            pictureBox1.Height = (int)(formHeight * 0.8);

            // 圖片外框
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;

            // 設定圖片框位置
            pictureBox1.Left = (int)(0.95 * formWidth) - pictureBox1.Width;
            pictureBox1.Top = (formHeight - pictureBox1.Height) / 2;

            // 設定文字框位置
            txtZoomPoint.Left = (int)(0.95 * formWidth) - pictureBox1.Width;
            txtZoomPoint.Top = 0;
            txtZoomPoint.Text = "滑鼠目前所在的座標: (0, 0); 在原始圖片上的座標: (0, 0); 縮放倍數: 1.00; 在pictureBox上offset原點座標: (0, 0)";
            txtZoomPoint.BackColor = Control.DefaultBackColor;

            txtMarkers.Left = (int)(0.95 * formWidth) - pictureBox1.Width;
            txtMarkers.Top = formHeight - 75;
            txtMarkers.Text = "圈選的座標點："; 
            txtMarkers.BackColor = Control.DefaultBackColor;
            txtMarkers.Visible = false; // 因lstMarkers已建立，故可不用顯示


            // 設定listbox位置
            lstGlass.Width = (int)(0.12 * formWidth);
            lstGlass.Height = (int)(formHeight * 0.4);
            lstGlass.Left = (int)(0.01 * formWidth);
            lstGlass.Top = (int)(0.1 * formHeight);            

            // 設定ToolStrip放大鏡、縮小鏡、回復原圖按鈕
            CreateToolStrip();

           
        }

        // 當點擊每張圖片後，生成儲存或刪除makers的Panel、並更新新圖片的markers
        // markers: 資料庫的舊紀錄，markersToShow: 當前顯示的紀錄，預設和舊紀錄相同
        private async void GenerateMarkerPanel(int ImageId)
        {
            if (markerPanel != null)
            {
                markerPanel.Controls.Clear();
            }
            else
            {
                markerPanel = new Panel();
            }

            // 根據顯示框相對大小設定PanelItem寬高
            PanelItemWidth = (int)(0.1 * formWidth);
            PanelItemHeight = 30;

            // 設定markerPanel位置與大小
            markerPanel.Size = new Size((int)(0.12 * formWidth), (int)(formHeight * 0.4));
            markerPanel.Left = (int)(0.01 * formWidth);
            markerPanel.Top = (int)(0.5 * formHeight);

            markerPanel.AutoScroll = true;


            // 創建 Label 顯示座標
            Label markerLabel = new Label();
            markerLabel.Name = "markerLabel";
            markerLabel.Text = $"標記點";
            markerLabel.Font = new Font("微軟正黑體", 12);
            markerLabel.Location = new Point(10, 10); 
            markerLabel.Size = new Size(PanelItemWidth, PanelItemHeight);

            markerPanel.Controls.Add(markerLabel);

            // 根據該圖片markedPoints資料創建checkbox
            // DataRow dr = dt.AsEnumerable().FirstOrDefault(row => (int)row["ImageId"] == ImageId); // 拿取Load時儲存的資料，但有可能中途改過未記錄到，故棄用
            DataTable temp =  await DataUtils.queryDataTableAsync("GlassMarkers", "ImageId=@ImageId", new Dictionary<string, object> { { "ImageId", ImageId } }, conn); // 從SQL撈取該筆ImageId的資料
            if(temp.Rows == null || temp.Rows.Count == 0)
            {
                MessageBox.Show("資料庫撈取不到這張影像，您看到了幽靈圖片");
                return;
            }
            DataRow dr = temp.Rows[0];
            
            // 若該圖片沒有紀錄的瑕疵點，則markers為空BindingList
            if (dr["MarkedPoints"] == DBNull.Value || dr["MarkedPoints"].ToString().Trim() == "[]" || dr["MarkedPoints"].ToString().Trim() == string.Empty)
            {
                markers = new BindingList<Point>();
            }
            else
            {
                markers = JsonConvert.DeserializeObject<BindingList<Point>>(dr["MarkedPoints"] as string);
            }
           
            // markedId為同一張圖片中被紀錄的瑕疵位置的id，從0開始
            for (int markedId = 0; markedId < markers.Count; markedId++)
            {
                CheckBox checkbox = new CheckBox();
                checkbox.Text = $"({markers[markedId].X}, {markers[markedId].Y})";
                checkbox.Location = new Point(10, 10 + PanelItemHeight * ( 1+ markedId ));
                checkbox.Size = new Size(PanelItemWidth, PanelItemHeight);
                checkbox.Checked = true;
                checkbox.Name = $"MarkerCheckBox{markedId}";
                checkbox.Tag = new { MarkedId = markedId, MarkedPoint = markers[markedId], ImageId = ImageId };
              
                checkbox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);

                markerPanel.Controls.Add(checkbox);
            }

            // 當圖片剛開啟時，預設既有所有的紅標被標到圖片上
            markersToShow = new BindingList<Point>(markers.Select(p => new Point(p.X, p.Y)).ToList());
            

            // 創建 Save 按鈕
            Button saveButton = new Button();
            saveButton.Name = "markedPointsSaveBtn";
            saveButton.Text = "儲存";
            saveButton.Size = new Size((int)(PanelItemWidth * 0.45), PanelItemHeight);
            saveButton.Location = new Point(10, 10 + PanelItemHeight * (1 + markersToShow.Count) );
            saveButton.Click += new EventHandler(MarkersSaveButton_Click);

            // 創建 Cancel 按鈕
            Button deleteButton = new Button();
            deleteButton.Name = "markedPointsCancelBtn";
            deleteButton.Text = "取消";
            deleteButton.Size = new Size((int)(PanelItemWidth * 0.45), PanelItemHeight);
            deleteButton.Location = new Point(100, 10 + PanelItemHeight * (1 + markersToShow.Count));
            deleteButton.Click += new EventHandler(MarkersCancelButton_Click);

            // 將刪除、儲存按鈕加入 checkBoxPanel
            markerPanel.Controls.Add(saveButton);
            markerPanel.Controls.Add(deleteButton);

           

            // 將 Panel 加入到 Form
            this.Controls.Add(markerPanel);
        }


        // 當checkbox被勾選或取消時，更新markersToShow
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            Point markedPoint = (Point)(checkbox.Tag as dynamic).MarkedPoint;
            if (checkbox.Checked)
            {
                if (!markersToShow.Any(p => p.X == markedPoint.X && p.Y == markedPoint.Y))
                {
                    markersToShow.Add(markedPoint);
                }
            }
            else
            {
                var pointToRemove = markersToShow.FirstOrDefault(p => p.X == markedPoint.X && p.Y == markedPoint.Y);
                if (pointToRemove != null)
                {
                    markersToShow.Remove(pointToRemove);
                }
            }

            previousMarkersSaved = false;
            pictureBox1.Invalidate();
            pictureBox1.Refresh();
        }

    
        // 當markersToShow有新增項目時，讓用戶更新上傳到資料庫
        private async void MarkersSaveButton_Click(object sender, EventArgs e)
        {
            // 跳出DialogBox讓用戶確認儲存的項目，只有checkbox有勾選的項目(markersToShow)才會被儲存            
            if (markersToShow.Count > 0)
            {
                string MakersToSave = "確定儲存標記點？\r\n";
                for (int i = 0; i < markersToShow.Count; i++)
                {
                    MakersToSave += $"({markersToShow[i].X}, {markersToShow[i].Y}) \r\n";
                }
                DialogResult result = MessageBox.Show(MakersToSave, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    bool isSaved = false;
                    isSaved = await SavePreviousMarkersAsync(previousImageId, markersToShow, user);
                    if (!isSaved)
                    {
                        MessageBox.Show("儲存失敗！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    previousMarkersSaved = true;
                    MessageBox.Show("儲存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("請至少選擇一個標記點！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        // 取消當前在視窗新增的項目，恢復成原資料庫的設定
        private void MarkersCancelButton_Click(object sender, EventArgs e)
        {
            string RevertToOldMarkers = "取消當前繪製項目，退回至原資料庫的紀錄？\r\n";
            DialogResult result = MessageBox.Show(RevertToOldMarkers, "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                GlassMarkerSelectedItem selectedItem = (GlassMarkerSelectedItem)lstGlass.SelectedItem;

                GenerateMarkerPanel(selectedItem.ImageId);
                LoadImage(selectedItem);
                previousMarkersSaved = false;
                return;
            }
        }



        private void CreateToolStrip()
        {
            // 添加ToolStrip
            //toolStrip1 = new ToolStrip
            //{
            //    AutoSize = true,
            //    Dock = DockStyle.Top,
            //    BackColor = Control.DefaultBackColor,
            //};

            toolStrip1.BackColor = Control.DefaultBackColor;

            // 在上方工具列提供放大鏡、縮小鏡按鈕
            ToolStripButton zoomInButton = new ToolStripButton
            {
                Image = ResizeImage(Image.FromFile("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\Images\\zoomin.ico"), 30, 30),
                ImageScaling = ToolStripItemImageScaling.None,
                CheckOnClick = true,
            };
            zoomInButton.Click += ZoomInButton_Click;
            toolStrip1.Items.Add(zoomInButton);


            ToolStripButton zoomOutButton = new ToolStripButton
            {
                Image = ResizeImage(Image.FromFile("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\Images\\zoomout.ico"), 30, 30),
                ImageScaling = ToolStripItemImageScaling.None,
                CheckOnClick = true,
            };
            zoomOutButton.Click += ZoomOutButton_Click;
            toolStrip1.Items.Add(zoomOutButton);
            toolStrip1.Items[1].Enabled = false;

            ToolStripButton resetButton = new ToolStripButton
            {
                Image = ResizeImage(Image.FromFile("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\Images\\compress-solid.ico"), 30, 30),
                ImageScaling = ToolStripItemImageScaling.None
            };
            resetButton.Click += ResetButton_Click;
            toolStrip1.Items.Add(resetButton);
        }

        private Cursor CreateCursor(string filePath, int width, int height)
        {
            using (Icon originalIcon = new Icon(filePath))
            {
                Image resizedImage = ResizeImage(originalIcon.ToBitmap(), width, height);
                using (Bitmap bitmap = new Bitmap(resizedImage))
                {
                    IntPtr hIcon = bitmap.GetHicon(); 
                    return new Cursor(hIcon);
                }
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            // 取得原始圖片清單
            //string[] jpgFiles = Directory.GetFiles("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\GlassImages", "*.jpg"); // 直接從資料夾取得
            // 原始圖片
            //orignialImage =  Image.FromFile("C:\\ASP NET作品集\\GlassExamination\\GlassExamination\\GlassImages\\04_Pexels Julia Khalimova.jpg"); //5806*3703

            // 將資料庫資料載入listbox，包含所有圖片的欄位
            dt = await DataUtils.queryDataTableAsync("GlassMarkers", null, null,  conn);
            
            lstGlass.Items.Clear();
            lstGlass.DisplayMember = "FileName";
            lstGlass.ValueMember = "ImageId";

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                string imagePath = dt.Rows[i]["ImagePath"].ToString();

                lstGlass.Items.Add(new GlassMarkerSelectedItem  { 
                    ImageId = (Int32)dt.Rows[i]["ImageId"],
                    ImagePath = imagePath,
                    FileName = imagePath.Split('\\')[imagePath.Split('\\').Length-1].Split('.')[0]
                });
            }
             // 設定第一張照片 
             lstGlass.SelectedIndexChanged += new EventHandler(LstGlass_SelectedIndexChanged); // 加載圖片
             lstGlass.SelectedItem = lstGlass.Items[0];
           
            // 根據pictureBox1長寬，計算原始圖片縮放比例
            GetOriginalImageZoom(pictureBox1, orignialImage);
            
            // 添加事件
            pictureBox1.Paint += pictureBox1_Paint; // 根據新座標繪製圖片、繪製所有marker
            pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick; // 標記新的marker
            pictureBox1.MouseClick += PictureBox1_MouseClick; // 放大或縮小圖片用
            // 當為預設鼠標時，可划動圖片
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove; // 當滑鼠在pictureBox1上移動時，顯示座標資訊
            pictureBox1.MouseUp += PictureBox1_MouseUp;           
            pictureBox1.MouseLeave += new EventHandler(PictureBox1_MouseLeave); // 當滑鼠離開pictureBox1時，清除資訊框

        }

        private void PictureBox1_MouseLeave(object sender, EventArgs e)
        {
            txtZoomPoint.Text = "滑鼠未落在圖片上";
        }

        // 計算原始圖片在pictureBox1上縮放比例(pbShrinkRatio)
        private void GetOriginalImageZoom(PictureBox pb, Image OriginalImage)
        {
            float widthAspect = OriginalImage.Width/ pb.Width;
            float heighAspect = OriginalImage.Height/ pb.Height;

            if(widthAspect >= heighAspect)
            {
                zoomMultiple = 1/ widthAspect;
                newWidth = pb.Width;
                newHeight = (int)(OriginalImage.Height / widthAspect);
            }
            else
            {
                zoomMultiple = 1 / heighAspect;
                newWidth = (int)(OriginalImage.Width / heighAspect);
                newHeight = pb.Height;
            }
            
            pbShrinkRatio = zoomMultiple; //取得原始圖片在pictureBox的縮放比例
            offset = new Point(0, 0);
 
        }

        // 縮放圖片用
        private Image ResizeImage(Image OriginalImage, int width, int height)
        {

            Bitmap newImage = new Bitmap(width,height);

            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(OriginalImage, new Rectangle(0,0,width,height));
            }

            return newImage;
        }


        
        private async void LstGlass_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlassMarkerSelectedItem selectedItem = (GlassMarkerSelectedItem)lstGlass.SelectedItem;
            

            if (previousImageId == 0)
            {
                GenerateMarkerPanel(selectedItem.ImageId);
                LoadImage(selectedItem);
                previousMarkersSaved = false;
                return;
            }

            // 檢查是否儲存條件
            // 1. 只要有一個checkbox被unchecked
            // 2. 只要checkbox的總數和markers的數量不同

            string checkSavingCondition = "PASS";

            if(markerPanel.Controls.OfType<CheckBox>().Count(c => !c.Checked) > 0)
            {
                checkSavingCondition = "FAIL";
            }
            
            if(markers.Count != markerPanel.Controls.OfType<CheckBox>().Count())
            {
                checkSavingCondition = "FAIL";
            }
 
            if (previousImageId > 0 && !previousMarkersSaved && checkSavingCondition == "FAIL")
            {
                string message = "本張影像尚未儲存，是否要更新選取的標記點？\r\n";
                for (int i = 0; i < markersToShow.Count; i++)
                {
                    message += $"({markersToShow[i].X}, {markersToShow[i].Y}) \r\n";
                }
                DialogResult result = MessageBox.Show(message, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    int countResult = 0;
                    countResult = await DataUtils.queryDataCountAysnc("GlassMarkers", "ImageId = @ImageId", new Dictionary<string, object> { { "ImageId",  previousImageId} }, conn);
                    if(countResult > 0)
                    {
                        bool isSaved = false;
                        isSaved = await SavePreviousMarkersAsync(previousImageId, markersToShow, user);
                        if (!isSaved)
                        {
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("資料庫讀不到這個影像，您是否看到幽靈圖片");
                        return;
                    }

                    
                }
            }

            markers.Clear();
            GenerateMarkerPanel(selectedItem.ImageId);
            LoadImage(selectedItem);
            previousMarkersSaved = false;


        }

        private void LoadImage(GlassMarkerSelectedItem selectedItem)
        {
            // selecteditem屬性:
            // ImageId, ImagePath, FileName

            if (selectedItem != null)
            {
                string imagePath = selectedItem.ImagePath; // 從選取的項目中取得圖片路徑

                // 嘗試從文件路徑加載圖片
                try
                {
                    orignialImage = Image.FromFile(imagePath);
                    previousImageId = selectedItem.ImageId;
                    pictureBox1.Image = orignialImage;
                 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"無法加載圖片: {ex.Message}");
                    return;
                }

            }

        }

        private async Task<bool> SavePreviousMarkersAsync(int ImageId, BindingList<Point> markers, string user)
        {
            string jsonMarkers = JsonConvert.SerializeObject(markers);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                string tableName = "GlassMarkers";
                string condition = "ImageId = @ImageId";  // 條件語句

                var parameters = new Dictionary<string, object>
                {
                    { "MarkedPoints", jsonMarkers },        // 新的 MarkedPoints 值
                    { "TxtTimestamp", timestamp },          // 更新的時間戳
                    { "ThisUser", user },                    // 更新的用戶ID
                    { "ImageId", ImageId }                  // 傳遞 ImageId 作為參數
                };

                // 呼叫萬用的 update 函數
                await DataUtils.updateDataAsync(tableName, condition, parameters, conn);
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法更新資料: {ex.Message}");
                return false;
            }
            

        }

       

       
        // 根據新圖片長寬、新原點座標在pictureBox繪製圖片
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(pictureBox1.BackColor);
            e.Graphics.DrawImage(orignialImage, new Rectangle(offset.X, offset.Y, newWidth, newHeight));
            PictureBox1_PaintMarker(this, e, markersToShow);

        }
     
        private void ZoomInButton_Click(object sender, EventArgs e)
        {   

            if(this.Cursor == zoomInCursor)
            {
                (sender as ToolStripButton).Checked = false;
                this.Cursor = defaultCursor;
            }

            else if(this.Cursor == zoomOutCursor || this.Cursor == defaultCursor)
            {
                (toolStrip1.Items[1] as ToolStripButton).Checked = false;
                (toolStrip1.Items[0] as ToolStripButton).Checked = true;
                this.Cursor = zoomInCursor;
            }

  
        }

        private void ZoomOutButton_Click(object sender, EventArgs e)
        {
            if (this.Cursor == zoomOutCursor)
            {
                (sender as ToolStripButton).Checked = false;
                this.Cursor = defaultCursor;
            }

            else if (this.Cursor == zoomInCursor || this.Cursor == defaultCursor)
            {
                (toolStrip1.Items[0] as ToolStripButton).Checked = false;
                (toolStrip1.Items[1] as ToolStripButton).Checked = true;
                this.Cursor = zoomOutCursor;
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            (toolStrip1.Items[0] as ToolStripButton).Checked = false;
            (toolStrip1.Items[1] as ToolStripButton).Checked = false;
            this.Cursor = defaultCursor;
            zoomCount = 1;

            offset = new Point(0, 0);
            newWidth = (int)(pbShrinkRatio * orignialImage.Width);
            newHeight = (int)(pbShrinkRatio * orignialImage.Height);

            pictureBox1.Invalidate();
            pictureBox1.Refresh();

        }


        private async void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (isHandlingMouseEvent) return;
            if (this.Cursor == defaultCursor)
            {
                return;
            }
            float toZoom = 1;

            if (this.Cursor == zoomInCursor)
            {
               if(zoomCount + 1 == zoomInLimitCount)
               {
                    (toolStrip1.Items[0] as ToolStripButton).Checked = false;
                    this.Cursor = defaultCursor;
               }
                zoomCount++;
                toZoom = zoomFactor;

            }
            if(this.Cursor == zoomOutCursor)
            {
                if (zoomCount - 1 == 1)
                {
                    (toolStrip1.Items[1] as ToolStripButton).Checked = false;                   
                    this.Cursor = defaultCursor;
                }

                zoomCount--;               
                toZoom = 1/zoomFactor;
            }

            if (zoomCount == 1)
            {
                toolStrip1.Items[0].Enabled = true;
                toolStrip1.Items[1].Enabled = false;
            }
            else if (zoomCount == zoomInLimitCount)
            {
                toolStrip1.Items[0].Enabled = false;
                toolStrip1.Items[1].Enabled = true;
            }
            else
            {
                toolStrip1.Items[0].Enabled = true;
                toolStrip1.Items[1].Enabled = true;
            }


            isHandlingMouseEvent = true;

            // 取得圖片框內點擊的位置
            Point mouseLocation = e.Location;

            // 取得圖片框內點擊的位置，以圖片大小為依據、左上角(imageRect.X, imageRect.Y)為原點的絕對座標
            lastClickPoint = GetImageAbsoluteCoordinate(mouseLocation, offset, zoomMultiple);

            

            // 計算新的zoomMultiple
            newWidth = (int)(newWidth * toZoom);
            newHeight = (int)(newHeight * toZoom);
            zoomMultiple = (float)newWidth / orignialImage.Width;

            // 計算新的原點位置
            int newX = mouseLocation.X - (int)((mouseLocation.X - offset.X) * toZoom);
            int newY = mouseLocation.Y - (int)((mouseLocation.Y - offset.Y) * toZoom);
            offset = new Point(newX, newY);


            // 計算縮放後圖片的長方形範圍
            //Rectangle zoomedImageRect = new Rectangle(newX, newY, newWidth, newHeight);

           

            // 重新繪製圖片
            pictureBox1.Invalidate();
            pictureBox1.Refresh();

            await Task.Delay(1000);
            isHandlingMouseEvent = false;
        }

        private async void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (isHandlingMouseEvent) return;

            if (Cursor.Current == zoomInCursor || Cursor.Current == zoomOutCursor)
            {
                return;
            }

            isHandlingMouseEvent = true;
            Point PointToPaint = e.Location;
            Point AbsolutePoint = GetImageAbsoluteCoordinate(e.Location, offset, zoomMultiple);

            // 新增新的點到markersToShow
            markersToShow.Add(AbsolutePoint);
            previousMarkersSaved = false;

            addNewMarkersToPanel(AbsolutePoint);

            using (Graphics g = pictureBox1.CreateGraphics())
            {
                PictureBox1_PaintMarker(this, new PaintEventArgs(g, pictureBox1.ClientRectangle), markersToShow);
            
            }
            await Task.Delay(1000);
            isHandlingMouseEvent = false;
        }

        private void addNewMarkersToPanel(Point newMarker)
        {
            // 新增一個新的 CheckBox 項目
            int newMarkedId = markersToShow.Count - 1;          

            // 新增新的checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.Text = $"({newMarker.X}, {newMarker.Y})";
            checkbox.Location = new Point(10, 10 + (1 + newMarkedId) * PanelItemHeight);
            checkbox.Size = new Size(PanelItemWidth, PanelItemHeight);
            checkbox.Checked = true;
            checkbox.Name = $"MarkerCheckBox{newMarkedId}";
            checkbox.Tag = new { MarkedId = newMarkedId, MarkedPoint = newMarker, ImageId = previousImageId };

            checkbox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
            markerPanel.Controls.Add(checkbox);

            // 根據內容高度調整markerPanel元件位置與大小
            Button markedPointsSaveBtn = markerPanel.Controls.OfType<Button>().SingleOrDefault(btn => btn.Name == "markedPointsSaveBtn");
            Button markedPointsCancelBtn = markerPanel.Controls.OfType<Button>().SingleOrDefault(btn => btn.Name == "markedPointsCancelBtn");

            markedPointsSaveBtn.Location = new Point(10, (1 + markersToShow.Count) * PanelItemHeight + 10);
            markedPointsCancelBtn.Location = new Point(100, (1 + markersToShow.Count) * PanelItemHeight + 10);

            // 強制重繪 markerPanel，防止舊位置殘留
            markerPanel.Invalidate();
            markerPanel.Update();

            // 需要儲存提示
            previousMarkersSaved = false;
        }

        private void PictureBox1_PaintMarker(object sender, PaintEventArgs e, BindingList<Point> markersToShow)
        {
            string strMarkers = "圈選的座標點：";
               
            using (Pen redPen = new Pen(Color.Red, 5))
            {
                int radius = 10; // 圓圈的半徑
                for(int i = 0; i < markersToShow.Count; i++)
                {
                    Point relativePoint = GetImageRelativeCoordinate(markersToShow[i], offset, zoomMultiple);
                    e.Graphics.DrawEllipse(redPen, relativePoint.X - radius, relativePoint.Y - radius, radius * 2, radius * 2);
                    strMarkers += $"({markersToShow[i].X}, {markersToShow[i].Y}), ";                   
                }
                
            }
            txtMarkers.Text = strMarkers;
        }

        // 以offset為原點，再根據原始圖片縮放倍數，計算點擊位置的絕對座標
        private Point GetImageAbsoluteCoordinate(Point clickPoint, Point offest , float zoomMultiple)
        {

            int absoluteX = (int)((clickPoint.X - offest.X) / zoomMultiple);
            int absoluteY = (int)((clickPoint.Y - offest.Y) / zoomMultiple);
           
            return new Point(absoluteX, absoluteY);
        }

        // 以(0,0)為原點，原始圖片大小為絕對座標，根據zoomMultiple換算成在pictureBox1呈現的相對座標點
        private Point GetImageRelativeCoordinate(Point absolutePoint, Point offest, float zoomMultiple)
        {
            int relativeX = (int)((absolutePoint.X * zoomMultiple) + offest.X);
            int relativeY = (int)((absolutePoint.Y * zoomMultiple) + offest.Y);

            return new Point(relativeX, relativeY);
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.Cursor == defaultCursor)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    lastMousePos = e.Location;
                }
            }
            else 
            {
                return;
            }
            
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // 計算鼠標拖曳的位移量
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;

                // 更新圖片的偏移量
                offset.X += dx;
                offset.Y += dy;

                // 更新鼠標的位置
                lastMousePos = e.Location;
                // GetImageAbsoluteCoordinate(offset, new Point(e.X,e.Y), zoomMultiple); // 絕對位置不變不用算

                // 重新繪製 PictureBox
                pictureBox1.Invalidate(); 
            }

            // 在文字框呈現縮放所在的座標位置訊息
            Point absolutePoint = GetImageAbsoluteCoordinate(e.Location, offset, zoomMultiple);

            txtZoomPoint.Text = $"滑鼠目前所在的座標: ({e.Location.X}, {e.Location.Y}); " +
                $"在原始圖片上的座標: ({absolutePoint.X}, {absolutePoint.Y});" +
                $"縮放倍數: {zoomMultiple.ToString("F2")}; " +
                $"在pictureBox上offset原點座標: ({offset.X}, {offset.Y})";

        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                pictureBox1.Invalidate(); // 觸發重繪事件
            }

        }

    }
}
