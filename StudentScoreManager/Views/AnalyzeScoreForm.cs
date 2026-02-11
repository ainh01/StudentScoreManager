using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Extensions.Primitives;
using StudentScoreManager.Controllers;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class AnalyzeScoreForm : Form
    {
        private readonly AnalyticsController _analyticsController;
        private readonly ClassController _classController;
        private readonly SubjectController _subjectController;

        private ComboBox cboSchoolYear;
        private ComboBox cboSemester;
        private ComboBox cboClass;
        private ComboBox cboSubject;
        private Button btnLoad;
        private Button btnExport;
        private Button btnClose;

        private GroupBox grpStatistics;
        private Label lblTotalStudents;
        private Label lblGradedStudents;
        private Label lblNotGraded;
        private Label lblAverageScore;
        private Label lblHighestScore;
        private Label lblLowestScore;
        private Label lblStandardDeviation;
        private Label lblPassRate;
        private Label lblFailRate;

        private Chart chartDistribution;
        private Chart chartPassFail;

        private DataGridView dgvTopPerformers;
        private DataGridView dgvAtRisk;

        private int _selectedClassId = 0;
        private int _selectedSubjectId = 0;
        private string _selectedSchoolYear = string.Empty;
        private int _selectedSemester = 0;

        private AnalyticsReportDTO _cachedReport;

        private ProgressBar progressBar;

        private Button btnRefresh;
        private ContextMenuStrip exportMenu;

        private PrintDocument _printDocument;
        private int _currentPrintPage = 0;
        private string[] _printLines;

        public AnalyzeScoreForm()
        {
            if (!SessionManager.IsAuthenticated || SessionManager.IsStudent())
            {
                MessageBox.Show(
                    "Truy cập bị từ chối. Tính năng này chỉ dành cho giáo viên và quản trị viên.",
                    "Lỗi Phân Quyền",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                this.Load += (s, e) => this.Close();
                return;
            }

            InitializeComponent();

            _analyticsController = new AnalyticsController();
            _classController = new ClassController();
            _subjectController = new SubjectController();

            InitializeCustomComponents();
            InitializePrintDocument();

            LoadFilterData();
            UpdateLoadButtonState();

            ConfigureFormProperties();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 900);
            this.Name = "AnalyzeScoreForm";
            this.Text = "Phân Tích & Trực Quan Hóa Điểm Số";
            this.ResumeLayout(false);
        }

        private void ConfigureFormProperties()
        {
            this.Text = "Phân Tích & Trực Quan Hóa Điểm Số";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        }

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();

            Panel scrollPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height),
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 240, 245),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            int margin = 15;
            int currentY = margin;

            GroupBox grpFilters = CreateFilterSection(margin, ref currentY);
            scrollPanel.Controls.Add(grpFilters);

            currentY += margin;

            grpStatistics = CreateStatisticsSection(margin, ref currentY);
            scrollPanel.Controls.Add(grpStatistics);

            currentY += margin;

            Panel pnlCharts = CreateChartsSection(margin, ref currentY);
            scrollPanel.Controls.Add(pnlCharts);

            currentY += margin;

            Panel pnlGrids = CreateGridsSection(margin, ref currentY);
            scrollPanel.Controls.Add(pnlGrids);

            currentY += margin;

            CreateActionButtons(margin, currentY, scrollPanel);

            this.Controls.Add(scrollPanel);

            CreateProgressBar(margin);

            this.ResumeLayout(true);
        }

        private void InitializePrintDocument()
        {
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
            _printDocument.DocumentName = "Báo Cáo Phân Tích Điểm Số";
        }

        private void CreateActionButtons(int margin, int currentY, Panel parentPanel)
        {
            btnRefresh = new Button
            {
                Text = "🔄 Làm Mới",
                Size = new Size(100, 40),
                Location = new Point(this.ClientSize.Width - 450, currentY),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;
            ToolTip ttRefresh = new ToolTip();
            ttRefresh.SetToolTip(btnRefresh, "Tải lại dữ liệu phân tích hiện tại");

            btnExport = new Button
            {
                Text = "📊 Xuất File ▼",
                Size = new Size(160, 40),
                Location = new Point(this.ClientSize.Width - 330, currentY),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            exportMenu = new ContextMenuStrip();
            exportMenu.Items.Add("📄 Xuất File TXT (.txt)", null, ExportAsTxt_Click);
            exportMenu.Items.Add("📊 Xuất File CSV (.csv)", null, ExportAsCsv_Click);
            exportMenu.Items.Add(new ToolStripSeparator());
            exportMenu.Items.Add("🖨️ In", null, PrintPreview_Click);

            btnClose = new Button
            {
                Text = "Đóng",
                Size = new Size(120, 40),
                Location = new Point(this.ClientSize.Width - 140, currentY),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            parentPanel.Controls.Add(btnRefresh);
            parentPanel.Controls.Add(btnExport);
            parentPanel.Controls.Add(btnClose);
        }

        private void CreateProgressBar(int margin)
        {
            progressBar = new ProgressBar
            {
                Location = new Point(margin, this.ClientSize.Height - 70),
                Size = new Size(this.ClientSize.Width - (margin * 2), 20),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };
            this.Controls.Add(progressBar);
        }

        private GroupBox CreateFilterSection(int margin, ref int currentY)
        {
            GroupBox grp = new GroupBox
            {
                Text = "Chọn",
                Location = new Point(margin, currentY),
                Size = new Size(this.ClientSize.Width - (margin * 2), 100),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 100)
            };

            int labelX = 20;
            int controlX = 140;
            int controlWidth = 200;
            int rowHeight = 30;
            int startY = 25;

            Label lblYear = new Label
            {
                Text = "Năm học:",
                Location = new Point(labelX, startY),
                Size = new Size(110, 23),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            };

            cboSchoolYear = new ComboBox
            {
                Location = new Point(controlX, startY),
                Size = new Size(controlWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cboSchoolYear.SelectedIndexChanged += CboSchoolYear_SelectedIndexChanged;

            int col2LabelX = controlX + controlWidth + 30;
            int col2ControlX = col2LabelX + 80;

            Label lblSemester = new Label
            {
                Text = "Học Kì:",
                Location = new Point(col2LabelX, startY),
                Size = new Size(70, 23),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            };

            cboSemester = new ComboBox
            {
                Location = new Point(col2ControlX, startY),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cboSemester.Items.AddRange(new object[] { "1", "2" });
            cboSemester.SelectedIndex = 0;
            cboSemester.SelectedIndexChanged += CboSemester_SelectedIndexChanged;

            startY += rowHeight;

            Label lblClass = new Label
            {
                Text = "Lớp:",
                Location = new Point(labelX, startY),
                Size = new Size(110, 23),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            };

            cboClass = new ComboBox
            {
                Location = new Point(controlX, startY),
                Size = new Size(controlWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            cboClass.SelectedIndexChanged += CboClass_SelectedIndexChanged;

            Label lblSubject = new Label
            {
                Text = "Môn Học:",
                Location = new Point(col2LabelX, startY),
                Size = new Size(70, 23),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            };

            cboSubject = new ComboBox
            {
                Location = new Point(col2ControlX, startY),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Enabled = false
            };
            cboSubject.SelectedIndexChanged += CboSubject_SelectedIndexChanged;

            btnLoad = new Button
            {
                Text = "Phân Tích",
                Location = new Point(col2ControlX + 220, startY - rowHeight / 2),
                Size = new Size(140, 50),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.Click += BtnLoad_Click;

            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(btnLoad, "Bấm để phân tích.");

            grp.Controls.AddRange(new Control[]
            {
                lblYear, cboSchoolYear, lblSemester, cboSemester,
                lblClass, cboClass, lblSubject, cboSubject, btnLoad
            });

            currentY += grp.Height;
            return grp;
        }

        private GroupBox CreateStatisticsSection(int margin, ref int currentY)
        {
            GroupBox grp = new GroupBox
            {
                Text = "Thống Kê Chung",
                Location = new Point(margin, currentY),
                Size = new Size(this.ClientSize.Width - (margin * 2), 120),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 100)
            };

            int labelHeight = 20;
            int valueHeight = 25;
            int columnWidth = (grp.Width - 40) / 3;
            int startY = 30;

            int col1X = 20;
            CreateStatLabel(grp, "Số Lượng Học Sinh:", col1X, startY);
            lblTotalStudents = CreateStatValue(grp, "-", col1X, startY + labelHeight);

            CreateStatLabel(grp, "Đã Nhập Điểm:", col1X, startY + labelHeight + valueHeight + 5);
            lblGradedStudents = CreateStatValue(grp, "-", col1X, startY + labelHeight * 2 + valueHeight + 5);

            CreateStatLabel(grp, "Chưa Nhập Điểm:", col1X, startY + (labelHeight + valueHeight) * 2 + 10);
            lblNotGraded = CreateStatValue(grp, "-", col1X, startY + labelHeight * 3 + valueHeight * 2 + 10);

            int col2X = col1X + columnWidth;
            CreateStatLabel(grp, "Điểm Trung Bình:", col2X, startY);
            lblAverageScore = CreateStatValue(grp, "-", col2X, startY + labelHeight);

            CreateStatLabel(grp, "Điểm Cao Nhất:", col2X, startY + labelHeight + valueHeight + 5);
            lblHighestScore = CreateStatValue(grp, "-", col2X, startY + labelHeight * 2 + valueHeight + 5);

            CreateStatLabel(grp, "Điểm Thấp Nhất:", col2X, startY + (labelHeight + valueHeight) * 2 + 10);
            lblLowestScore = CreateStatValue(grp, "-", col2X, startY + labelHeight * 3 + valueHeight * 2 + 10);

            int col3X = col2X + columnWidth;
            CreateStatLabel(grp, "Độ Lệch Chuẩn:", col3X, startY);
            lblStandardDeviation = CreateStatValue(grp, "-", col3X, startY + labelHeight);

            CreateStatLabel(grp, "Tỉ Lệ Đạt:", col3X, startY + labelHeight + valueHeight + 5);
            lblPassRate = CreateStatValue(grp, "-", col3X, startY + labelHeight * 2 + valueHeight + 5);
            lblPassRate.ForeColor = Color.FromArgb(0, 150, 0);

            CreateStatLabel(grp, "Tỉ Lệ Kém:", col3X, startY + (labelHeight + valueHeight) * 2 + 10);
            lblFailRate = CreateStatValue(grp, "-", col3X, startY + labelHeight * 3 + valueHeight * 2 + 10);
            lblFailRate.ForeColor = Color.FromArgb(200, 0, 0);

            currentY += grp.Height;
            return grp;
        }

        private Label CreateStatLabel(Control parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private Label CreateStatValue(Control parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 60)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private Panel CreateChartsSection(int margin, ref int currentY)
        {
            Panel pnl = new Panel
            {
                Location = new Point(margin, currentY),
                Size = new Size(this.ClientSize.Width - (margin * 2), 320),
                BorderStyle = BorderStyle.None
            };

            int chartWidth = (pnl.Width - 30) / 2;
            int chartHeight = pnl.Height;

            chartDistribution = new Chart
            {
                Location = new Point(0, 0),
                Size = new Size(chartWidth, chartHeight),
                BackColor = Color.White,
                BorderlineColor = Color.FromArgb(200, 200, 200),
                BorderlineWidth = 1,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            ChartArea areaDistribution = new ChartArea("MainArea")
            {
                BackColor = Color.White,
                BorderColor = Color.FromArgb(180, 180, 180),
                BorderWidth = 1,
                BorderDashStyle = ChartDashStyle.Solid
            };

            areaDistribution.AxisX.Title = "Phổ Điểm";
            areaDistribution.AxisX.TitleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            areaDistribution.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            areaDistribution.AxisX.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            areaDistribution.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            areaDistribution.AxisX.IsLabelAutoFit = true;
            areaDistribution.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;

            areaDistribution.AxisY.Title = "Số lượng";
            areaDistribution.AxisY.TitleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            areaDistribution.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            areaDistribution.AxisY.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            areaDistribution.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chartDistribution.ChartAreas.Add(areaDistribution);

            Series seriesDistribution = new Series("Distribution")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(0, 120, 215),
                BorderWidth = 2,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            seriesDistribution["PointWidth"] = "0.8";
            chartDistribution.Series.Add(seriesDistribution);

            Title titleDist = new Title
            {
                Text = "Biểu Đồ Phân Bố Điểm",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 100),
                Docking = Docking.Top,
                Alignment = ContentAlignment.MiddleCenter
            };
            chartDistribution.Titles.Add(titleDist);

            pnl.Controls.Add(chartDistribution);

            chartPassFail = new Chart
            {
                Location = new Point(chartWidth + 30, 0),
                Size = new Size(chartWidth, chartHeight),
                BackColor = Color.White,
                BorderlineColor = Color.FromArgb(200, 200, 200),
                BorderlineWidth = 1,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            ChartArea areaPassFail = new ChartArea("MainArea")
            {
                BackColor = Color.White,
                Area3DStyle = { Enable3D = true, Inclination = 15, Rotation = 10 }
            };
            chartPassFail.ChartAreas.Add(areaPassFail);

            Series seriesPassFail = new Series("PassFail")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                LabelFormat = "0.0'%'"
            };
            seriesPassFail["PieLabelStyle"] = "Outside";
            seriesPassFail["PieLineColor"] = "Black";
            chartPassFail.Series.Add(seriesPassFail);

            Legend legendPassFail = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.Transparent
            };
            chartPassFail.Legends.Add(legendPassFail);

            Title titlePassFail = new Title
            {
                Text = "Tỉ Lệ Đạt/Kém",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 100),
                Docking = Docking.Top,
                Alignment = ContentAlignment.MiddleCenter
            };
            chartPassFail.Titles.Add(titlePassFail);

            pnl.Controls.Add(chartPassFail);

            currentY += pnl.Height;
            return pnl;
        }

        private Panel CreateGridsSection(int margin, ref int currentY)
        {
            Panel pnl = new Panel
            {
                Location = new Point(margin, currentY),
                Size = new Size(this.ClientSize.Width - (margin * 2), 200),
                BorderStyle = BorderStyle.None
            };

            int gridWidth = (pnl.Width - 30) / 2;
            int gridHeight = pnl.Height;

            GroupBox grpTop = new GroupBox
            {
                Text = "🏆 Học Sinh Xuất Sắc",
                Location = new Point(0, 0),
                Size = new Size(gridWidth, gridHeight),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 0)
            };

            dgvTopPerformers = new DataGridView
            {
                Location = new Point(10, 25),
                Size = new Size(gridWidth - 20, gridHeight - 35),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Segoe UI", 9F),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(245, 250, 245)
                }
            };

            dgvTopPerformers.AutoGenerateColumns = false;
            dgvTopPerformers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StudentName",
                HeaderText = "Tên Học Sinh",
                FillWeight = 70
            });
            dgvTopPerformers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FnScore",
                HeaderText = "Điểm",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(0, 100, 0),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                FillWeight = 30
            });

            grpTop.Controls.Add(dgvTopPerformers);
            pnl.Controls.Add(grpTop);

            GroupBox grpRisk = new GroupBox
            {
                Text = "⚠️ Học Sinh Cá Biệt",
                Location = new Point(gridWidth + 30, 0),
                Size = new Size(gridWidth, gridHeight),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 0, 0)
            };

            dgvAtRisk = new DataGridView
            {
                Location = new Point(10, 25),
                Size = new Size(gridWidth - 20, gridHeight - 35),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Segoe UI", 9F),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(255, 245, 245)
                }
            };

            dgvAtRisk.AutoGenerateColumns = false;
            dgvAtRisk.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StudentName",
                HeaderText = "Tên Học Sinh",
                FillWeight = 70
            });
            dgvAtRisk.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FnScore",
                HeaderText = "Điểm",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(180, 0, 0),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                },
                FillWeight = 30
            });

            grpRisk.Controls.Add(dgvAtRisk);
            pnl.Controls.Add(grpRisk);

            currentY += pnl.Height;
            return pnl;
        }

        private void LoadFilterData()
        {
            try
            {
                var schoolYears = _classController.GetAllSchoolYears();
                if (schoolYears != null && schoolYears.Count > 0)
                {
                    cboSchoolYear.DataSource = schoolYears;
                    cboSchoolYear.SelectedIndex = 0;
                    _selectedSchoolYear = schoolYears[0];
                }
                else
                {
                    MessageBox.Show(
                        "Không tìm thấy năm học trong cơ sở dữ liệu. Vui lòng liên hệ quản trị viên.",
                        "Không Có Dữ Liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    cboSchoolYear.Enabled = false;
                }

                if (cboSemester.Items.Count > 0)
                {
                    cboSemester.SelectedIndex = 0;
                    _selectedSemester = int.Parse(cboSemester.Text);
                }

                LoadClasses();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải dữ liệu bộ lọc: {ex.Message}",
                    "Lỗi Cơ Sở Dữ Liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadClasses()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedSchoolYear) || _selectedSemester <= 0)
                {
                    cboClass.DataSource = null;
                    cboClass.Enabled = false;
                    cboSubject.DataSource = null;
                    cboSubject.Enabled = false;
                    _selectedClassId = 0;
                    _selectedSubjectId = 0;
                    UpdateLoadButtonState();
                    return;
                }

                var classes = _classController.GetClassesForCurrentUser(_selectedSchoolYear, _selectedSemester);

                if (classes != null && classes.Count > 0)
                {
                    cboClass.DisplayMember = "Name";
                    cboClass.ValueMember = "Id";
                    cboClass.DataSource = classes;
                    cboClass.Enabled = true;
                    _selectedClassId = (int)cboClass.SelectedValue;
                }
                else
                {
                    cboClass.DataSource = null;
                    cboClass.Enabled = false;
                    _selectedClassId = 0;
                }

                cboSubject.DataSource = null;
                cboSubject.Enabled = false;
                _selectedSubjectId = 0;

                UpdateLoadButtonState();
                LoadSubjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải danh sách lớp: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadSubjects()
        {
            try
            {
                if (_selectedClassId <= 0 || string.IsNullOrEmpty(_selectedSchoolYear) || _selectedSemester <= 0)
                {
                    cboSubject.DataSource = null;
                    cboSubject.Enabled = false;
                    _selectedSubjectId = 0;
                    UpdateLoadButtonState();
                    return;
                }

                var subjects = _subjectController.GetSubjectsForCurrentUser(
                    _selectedClassId,
                    _selectedSchoolYear,
                    _selectedSemester);

                if (subjects != null && subjects.Count > 0)
                {
                    cboSubject.DisplayMember = "Name";
                    cboSubject.ValueMember = "Id";
                    cboSubject.DataSource = subjects;
                    cboSubject.Enabled = true;
                    _selectedSubjectId = (int)cboSubject.SelectedValue;
                }
                else
                {
                    cboSubject.DataSource = null;
                    cboSubject.Enabled = false;
                    _selectedSubjectId = 0;
                }
                UpdateLoadButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Đã xảy ra lỗi khi tải danh sách môn học: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void UpdateLoadButtonState()
        {
            btnLoad.Enabled = _selectedClassId > 0 &&
                              _selectedSubjectId > 0 &&
                              !string.IsNullOrEmpty(_selectedSchoolYear) &&
                              _selectedSemester > 0;
        }

        private void CboSchoolYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSchoolYear.SelectedItem == null)
                return;

            _selectedSchoolYear = cboSchoolYear.SelectedItem.ToString();

            cboClass.DataSource = null;
            cboClass.Enabled = false;
            cboSubject.DataSource = null;
            cboSubject.Enabled = false;
            _selectedClassId = 0;
            _selectedSubjectId = 0;

            ClearAnalyticsDisplay();
            UpdateLoadButtonState();

            LoadClasses();
        }

        private void CboSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSemester.SelectedIndex < 0 || cboSemester.SelectedItem == null)
                return;

            _selectedSemester = int.Parse(cboSemester.SelectedItem.ToString());

            cboClass.DataSource = null;
            cboClass.Enabled = false;
            cboSubject.DataSource = null;
            cboSubject.Enabled = false;
            _selectedClassId = 0;
            _selectedSubjectId = 0;

            ClearAnalyticsDisplay();
            UpdateLoadButtonState();

            LoadClasses();
        }

        private void CboClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboClass.SelectedValue == null)
                {
                    _selectedClassId = 0;
                    cboSubject.DataSource = null;
                    cboSubject.Enabled = false;
                    _selectedSubjectId = 0;
                    ClearAnalyticsDisplay();
                    UpdateLoadButtonState();
                    return;
                }

                int classId;
                if (cboClass.SelectedValue is int)
                {
                    classId = (int)cboClass.SelectedValue;
                }
                else if (int.TryParse(cboClass.SelectedValue.ToString(), out int parsed))
                {
                    classId = parsed;
                }
                else
                {
                    _selectedClassId = 0;
                    return;
                }

                _selectedClassId = classId;

                cboSubject.DataSource = null;
                cboSubject.Enabled = false;
                _selectedSubjectId = 0;

                ClearAnalyticsDisplay();
                UpdateLoadButtonState();

                LoadSubjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải môn học: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void CboSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSubject.SelectedValue == null)
            {
                _selectedSubjectId = 0;
                ClearAnalyticsDisplay();
                UpdateLoadButtonState();
                return;
            }

            if (cboSubject.SelectedValue is int subjectId)
            {
                _selectedSubjectId = subjectId;
            }
            else if (int.TryParse(cboSubject.SelectedValue.ToString(), out int parsed))
            {
                _selectedSubjectId = parsed;
            }
            else
            {
                _selectedSubjectId = 0;
            }

            ClearAnalyticsDisplay();
            UpdateLoadButtonState();
        }

        private async void LoadAnalytics()
        {
            if (_selectedClassId <= 0 || _selectedSubjectId <= 0 ||
                string.IsNullOrEmpty(_selectedSchoolYear) || _selectedSemester <= 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn đầy đủ các tiêu chí lọc (Năm Học, Học Kì, Lớp và Môn Học) trước khi tải phân tích.",
                    "Thiếu Bộ Lọc",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            ShowLoadingState(true);

            try
            {
                int currentClassIdForReport = _selectedClassId;
                int currentSubjectIdForReport = _selectedSubjectId;
                string currentSchoolYearForReport = _selectedSchoolYear;
                int currentSemesterForReport = _selectedSemester;

                _cachedReport = await Task.Run(() =>
                    _analyticsController.GenerateReport(
                        currentClassIdForReport,
                        currentSubjectIdForReport,
                        currentSchoolYearForReport,
                        currentSemesterForReport
                    )
                );

                if (_cachedReport == null || _cachedReport.Statistics == null)
                {
                    MessageBox.Show(
                        "Không thể tạo báo cáo phân tích. Điều này có thể do dữ liệu không đủ hoặc lỗi hệ thống.",
                        "Không Có Dữ Liệu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    ClearAnalyticsDisplay();
                    return;
                }

                PopulateStatistics(_cachedReport.Statistics);
                PopulateDistributionChart(_cachedReport.ScoreDistribution);
                PopulatePassFailChart(_cachedReport.PassFailStatistics);
                PopulateTopPerformers(_cachedReport.TopPerformers);
                PopulateAtRiskStudents(_cachedReport.AtRiskStudents);

                btnExport.Enabled = true;
                btnRefresh.Enabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Bạn không có quyền xem phân tích cho sự kết hợp lớp và môn học này.",
                    "Truy Cập Bị Từ Chối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                ClearAnalyticsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Đã xảy ra lỗi khi tải phân tích:\n\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                ClearAnalyticsDisplay();
            }
            finally
            {
                ShowLoadingState(false);
            }
        }

        private void ShowLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                this.Cursor = Cursors.WaitCursor;
                btnLoad.Enabled = false;
                btnLoad.Text = "Đang tải...";
                btnRefresh.Enabled = false;
                progressBar.Visible = true;
            }
            else
            {
                this.Cursor = Cursors.Default;
                UpdateLoadButtonState();
                btnLoad.Text = "Phân Tích";
                progressBar.Visible = false;
            }
        }

        private void PopulateStatistics(StatisticsDTO stats)
        {
            if (stats == null) return;

            lblTotalStudents.Text = stats.TotalStudents.ToString();
            lblGradedStudents.Text = stats.GradedStudents.ToString();
            lblNotGraded.Text = (stats.TotalStudents - stats.GradedStudents).ToString();

            if (stats.GradedStudents == 0)
            {
                lblAverageScore.Text = "N/A";
                lblHighestScore.Text = "N/A";
                lblLowestScore.Text = "N/A";
                lblStandardDeviation.Text = "N/A";
                lblPassRate.Text = "0.0%";
                lblFailRate.Text = "0.0%";
                return;
            }

            lblAverageScore.Text = stats.AverageScore.ToString();
            lblHighestScore.Text = stats.HighestScore.ToString("F2");
            lblLowestScore.Text = stats.LowestScore.ToString("F2");
            lblStandardDeviation.Text = stats.StandardDeviation.ToString();

            decimal passRate = stats.GradedStudents > 0
                ? (stats.PassingStudents / (decimal)stats.GradedStudents * 100)
                : 0;
            decimal failRate = stats.GradedStudents > 0
                ? (stats.FailingStudents / (decimal)stats.GradedStudents * 100)
                : 0;

            lblPassRate.Text = $"{passRate:F1}%";
            lblFailRate.Text = $"{failRate:F1}%";
        }

        private void PopulateDistributionChart(Dictionary<string, int> distribution)
        {
            if (distribution == null || distribution.Count == 0 || distribution.Values.Sum() == 0)
            {
                chartDistribution.Series[0].Points.Clear();
                chartDistribution.Titles[0].Text = "Biểu Đồ Phân Bố Điểm (Không Có Dữ Liệu)";
                return;
            }

            Series series = chartDistribution.Series[0];
            series.Points.Clear();

            var colorMap = new Dictionary<string, Color>
            {
                { "Xuất Sắc (9.0-10.0)", Color.FromArgb(0, 150, 0) },
                { "Giỏi (8.0-8.9)", Color.FromArgb(50, 200, 50) },
                { "Khá (7.0-7.9)", Color.FromArgb(100, 180, 255) },
                { "Trung Bình (6.0-6.9)", Color.FromArgb(0, 120, 215) },
                { "Dưới Trung Bình (5.0-5.9)", Color.FromArgb(255, 160, 0) },
                { "Kém (<5.0)", Color.FromArgb(200, 0, 0) },
                { "Chưa Nhập", Color.FromArgb(180, 180, 180) }
            };

            var orderedCategories = new List<string>
            {
                "Kém (<5.0)",
                "Dưới Trung Bình (5.0-5.9)",
                "Trung Bình (6.0-6.9)",
                "Khá (7.0-7.9)",
                "Giỏi (8.0-8.9)",
                "Xuất Sắc (9.0-10.0)",
                "Chưa Nhập"
            };

            foreach (var category in orderedCategories)
            {
                if (distribution.ContainsKey(category) && distribution[category] > 0)
                {
                    int index = series.Points.AddXY(category, distribution[category]);
                    series.Points[index].Color = colorMap.ContainsKey(category) ? colorMap[category] : Color.Gray;
                    series.Points[index].Label = distribution[category].ToString();
                }
            }
            chartDistribution.Titles[0].Text = "Biểu Đồ Phân Bố Điểm";
            chartDistribution.Invalidate();
        }

        private void PopulatePassFailChart(Dictionary<string, int> passFailStats)
        {
            if (passFailStats == null || passFailStats.Count == 0 || passFailStats.Values.Sum() == 0)
            {
                chartPassFail.Series[0].Points.Clear();
                chartPassFail.Titles[0].Text = "Tỉ Lệ Đạt/Kém";
                chartPassFail.Series[0].Points.AddXY("Không có dữ liệu", 1);
                chartPassFail.Series[0].Points[0].Color = Color.LightGray;
                chartPassFail.Series[0].Points[0].Label = "0%";
                return;
            }

            Series series = chartPassFail.Series[0];
            series.Points.Clear();

            int totalGraded = 0;
            passFailStats.TryGetValue("Đạt (≥5.0)", out int passingCount);
            passFailStats.TryGetValue("Kém (<5.0)", out int failingCount);
            totalGraded = passingCount + failingCount;

            if (totalGraded == 0)
            {
                series.Points.AddXY("Không Có Học Sinh Đã Chấm", 1);
                series.Points[0].Color = Color.Gray;
                series.Points[0].Label = "Không Có Dữ Liệu";
            }
            else
            {
                if (passingCount > 0)
                {
                    var passPoint = series.Points.Add(passingCount);
                    passPoint.Color = Color.FromArgb(0, 180, 0);
                    passPoint.LegendText = $"Đạt (≥5.0): {passingCount} học sinh";
                    passPoint.Label = "#PERCENT";
                }

                if (failingCount > 0)
                {
                    var failPoint = series.Points.Add(failingCount);
                    failPoint.Color = Color.FromArgb(220, 0, 0);
                    failPoint.LegendText = $"Kém (<5.0): {failingCount} học sinh";
                    failPoint.Label = "#PERCENT";
                }
            }
            chartPassFail.Titles[0].Text = "Tỉ Lệ Đạt/Kém";
            chartPassFail.Invalidate();
        }

        private void PopulateTopPerformers(List<ScoreSummaryDTO> topStudents)
        {
            dgvTopPerformers.DataSource = null;
            if (topStudents == null || topStudents.Count == 0)
            {
                return;
            }

            dgvTopPerformers.DataSource = topStudents.Take(10).ToList();
        }

        private void PopulateAtRiskStudents(List<ScoreSummaryDTO> atRiskStudents)
        {
            dgvAtRisk.DataSource = null;
            if (atRiskStudents == null || atRiskStudents.Count == 0)
            {
                return;
            }

            dgvAtRisk.DataSource = atRiskStudents;
        }

        private void ClearAnalyticsDisplay()
        {
            lblTotalStudents.Text = "-";
            lblGradedStudents.Text = "-";
            lblNotGraded.Text = "-";
            lblAverageScore.Text = "-";
            lblHighestScore.Text = "-";
            lblLowestScore.Text = "-";
            lblStandardDeviation.Text = "-";
            lblPassRate.Text = "-";
            lblFailRate.Text = "-";

            chartDistribution.Series[0].Points.Clear();
            chartDistribution.Titles[0].Text = "Biểu Đồ Phân Bố Điểm";
            chartPassFail.Series[0].Points.Clear();
            chartPassFail.Series[0].Points.AddXY("Không có dữ liệu", 1);
            chartPassFail.Series[0].Points[0].Color = Color.LightGray;
            chartPassFail.Series[0].Points[0].Label = "0%";
            chartPassFail.Titles[0].Text = "Tỉ Lệ Đạt/Kém";

            dgvTopPerformers.DataSource = null;
            dgvAtRisk.DataSource = null;

            btnExport.Enabled = false;
            btnRefresh.Enabled = false;

            _cachedReport = null;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (_cachedReport == null)
            {
                MessageBox.Show(
                    "Không có dữ liệu để xuất. Vui lòng tải phân tích trước.",
                    "Không Có Dữ Liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            exportMenu.Show(btnExport, new Point(0, btnExport.Height));
        }

        private void ExportAsTxt_Click(object sender, EventArgs e)
        {
            if (_cachedReport == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Tệp Văn Bản (*.txt)|*.txt";
                sfd.FileName = $"Bao_Cao_Phan_Tich_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                sfd.DefaultExt = "txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportReportToFile(sfd.FileName);
                    MessageBox.Show("Xuất báo cáo văn bản thành công!", "Thành Công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportAsCsv_Click(object sender, EventArgs e)
        {
            if (_cachedReport == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Tệp CSV (*.csv)|*.csv";
                sfd.FileName = $"Bao_Cao_Phan_Tich_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                sfd.DefaultExt = "csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportReportToCSV(sfd.FileName);
                    MessageBox.Show("Xuất báo cáo CSV thành công!", "Thành Công",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void PrintPreview_Click(object sender, EventArgs e)
        {
            if (_cachedReport == null)
            {
                MessageBox.Show(
                    "Không có dữ liệu để in. Vui lòng tải phân tích trước.",
                    "Không Có Dữ Liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                _currentPrintPage = 0;

                PrintPreviewDialog previewDialog = new PrintPreviewDialog
                {
                    Document = _printDocument,
                    Width = 900,
                    Height = 700,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = "Xem Trước Khi In - Báo Cáo Phân Tích"
                };

                previewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi hiển thị bản xem trước in:\n\n{ex.Message}",
                    "Lỗi In Ấn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            Font headerFont = new Font("Segoe UI", 11F, FontStyle.Bold);
            Font normalFont = new Font("Segoe UI", 10F, FontStyle.Regular);
            Font smallFont = new Font("Segoe UI", 9F, FontStyle.Regular);

            Brush blackBrush = Brushes.Black;
            Brush blueBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
            Brush grayBrush = new SolidBrush(Color.FromArgb(100, 100, 100));

            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            float rightMargin = e.MarginBounds.Right;
            float yPos = topMargin;
            float lineHeight = normalFont.GetHeight(g);
            float currentLineHeight;

            string title = "BÁO CÁO PHÂN TÍCH ĐIỂM SỐ";
            SizeF titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, blueBrush,
                leftMargin + (e.MarginBounds.Width - titleSize.Width) / 2, yPos);
            yPos += titleFont.GetHeight(g) + 10;

            g.DrawLine(new Pen(Color.Black, 2), leftMargin, yPos, rightMargin, yPos);
            yPos += 15;

            currentLineHeight = normalFont.GetHeight(g);
            g.DrawString($"Năm Học: {_selectedSchoolYear}", normalFont, blackBrush, leftMargin, yPos);
            g.DrawString($"Học Kì: {_selectedSemester}", normalFont, blackBrush, rightMargin - 150, yPos);
            yPos += currentLineHeight + 5;

            g.DrawString($"Lớp: {cboClass.Text}", normalFont, blackBrush, leftMargin, yPos);
            g.DrawString($"Môn: {cboSubject.Text}", normalFont, blackBrush, rightMargin - 150, yPos);
            yPos += currentLineHeight + 5;

            currentLineHeight = smallFont.GetHeight(g);
            g.DrawString($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}", smallFont, grayBrush, leftMargin, yPos);
            yPos += currentLineHeight + 15;

            currentLineHeight = headerFont.GetHeight(g);
            g.DrawString("THỐNG KÊ CHUNG", headerFont, blueBrush, leftMargin, yPos);
            yPos += currentLineHeight + 10;

            if (_cachedReport?.Statistics != null)
            {
                var stats = _cachedReport.Statistics;
                currentLineHeight = normalFont.GetHeight(g);

                g.DrawString($"Tổng số học sinh: {stats.TotalStudents}", normalFont, blackBrush, leftMargin, yPos);
                g.DrawString($"Đã nhập điểm: {stats.GradedStudents}", normalFont, blackBrush, leftMargin + 250, yPos);
                yPos += currentLineHeight + 5;

                if (stats.GradedStudents > 0)
                {
                    g.DrawString($"Điểm trung bình: {stats.AverageScore:F2}", normalFont, blackBrush, leftMargin, yPos);
                    g.DrawString($"Cao nhất: {stats.HighestScore:F2}", normalFont, blackBrush, leftMargin + 250, yPos);
                    yPos += currentLineHeight + 5;

                    g.DrawString($"Thấp nhất: {stats.LowestScore:F2}", normalFont, blackBrush, leftMargin, yPos);
                    g.DrawString($"Độ lệch chuẩn: {stats.StandardDeviation:F2}", normalFont, blackBrush, leftMargin + 250, yPos);
                    yPos += currentLineHeight + 5;

                    g.DrawString($"Tỉ lệ đạt: {stats.PassRate:F1}%", normalFont, new SolidBrush(Color.Green), leftMargin, yPos);
                    g.DrawString($"Tỉ lệ kém: {stats.FailRate:F1}%", normalFont, new SolidBrush(Color.Red), leftMargin + 250, yPos);
                    yPos += currentLineHeight + 15;
                }
                else
                {
                    g.DrawString("Không có dữ liệu điểm đã chấm.", normalFont, blackBrush, leftMargin, yPos);
                    yPos += currentLineHeight + 15;
                }
            }

            currentLineHeight = headerFont.GetHeight(g);
            g.DrawString("PHÂN BỐ ĐIỂM", headerFont, blueBrush, leftMargin, yPos);
            yPos += currentLineHeight + 10;

            if (_cachedReport?.ScoreDistribution != null)
            {
                var orderedCategories = new List<string>
                {
                    "Xuất Sắc (9.0-10.0)", "Giỏi (8.0-8.9)", "Khá (7.0-7.9)",
                    "Trung Bình (6.0-6.9)", "Dưới Trung Bình (5.0-5.9)", "Kém (<5.0)", "Chưa Nhập"
                };
                currentLineHeight = normalFont.GetHeight(g);

                foreach (var category in orderedCategories)
                {
                    if (_cachedReport.ScoreDistribution.ContainsKey(category))
                    {
                        g.DrawString($"• {category}: {_cachedReport.ScoreDistribution[category]} học sinh",
                            normalFont, blackBrush, leftMargin + 10, yPos);
                        yPos += currentLineHeight + 3;
                    }
                }
                yPos += 10;
            }
            else
            {
                currentLineHeight = normalFont.GetHeight(g);
                g.DrawString("Không có dữ liệu phân bố điểm.", normalFont, blackBrush, leftMargin, yPos);
                yPos += currentLineHeight + 15;
            }

            if (_cachedReport?.TopPerformers != null && _cachedReport.TopPerformers.Count > 0)
            {
                currentLineHeight = headerFont.GetHeight(g);
                g.DrawString("HỌC SINH XUẤT SẮC (Top 10)", headerFont, blueBrush, leftMargin, yPos);
                yPos += currentLineHeight + 8;

                currentLineHeight = normalFont.GetHeight(g);
                int count = 1;
                foreach (var student in _cachedReport.TopPerformers.Take(10))
                {
                    g.DrawString($"{count}. {student.StudentName}", normalFont, blackBrush, leftMargin + 10, yPos);
                    g.DrawString($"{student.FnScore:F2}", normalFont, new SolidBrush(Color.Green), rightMargin - 50, yPos);
                    yPos += currentLineHeight + 3;
                    count++;
                }
                yPos += 10;
            }
            else
            {
                currentLineHeight = normalFont.GetHeight(g);
                g.DrawString("Không có học sinh xuất sắc.", normalFont, blackBrush, leftMargin, yPos);
                yPos += currentLineHeight + 15;
            }

            if (_cachedReport?.AtRiskStudents != null && _cachedReport.AtRiskStudents.Count > 0)
            {
                currentLineHeight = headerFont.GetHeight(g);
                g.DrawString("HỌC SINH CẦN QUAN TÂM (< 5.0)", headerFont, new SolidBrush(Color.Red), leftMargin, yPos);
                yPos += currentLineHeight + 8;

                currentLineHeight = normalFont.GetHeight(g);
                int count = 1;
                foreach (var student in _cachedReport.AtRiskStudents)
                {
                    g.DrawString($"{count}. {student.StudentName}", normalFont, blackBrush, leftMargin + 10, yPos);
                    g.DrawString($"{student.FnScore:F2}", normalFont, new SolidBrush(Color.Red), rightMargin - 50, yPos);
                    yPos += currentLineHeight + 3;
                    count++;
                }
            }
            else
            {
                currentLineHeight = normalFont.GetHeight(g);
                g.DrawString("Không có học sinh cá biệt (điểm dưới 5.0).", normalFont, blackBrush, leftMargin, yPos);
                yPos += currentLineHeight + 15;
            }

            yPos = e.MarginBounds.Bottom - 20;
            g.DrawLine(Pens.Gray, leftMargin, yPos, rightMargin, yPos);
            yPos += 5;
            currentLineHeight = smallFont.GetHeight(g);
            g.DrawString($"Trang 1 - Hệ Thống Quản Lý Điểm Học Sinh",
                smallFont, grayBrush, leftMargin + (e.MarginBounds.Width - g.MeasureString("Trang 1 - Hệ Thống Quản Lý Điểm Học Sinh", smallFont).Width) / 2, yPos);

            e.HasMorePages = false;
        }

        private void ExportReportToFile(string filePath)
        {
            if (_cachedReport == null) return;

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("===================================================");
                sb.AppendLine("BÁO CÁO PHÂN TÍCH ĐIỂM SỐ");
                sb.AppendLine("===================================================");
                sb.AppendLine($"Năm Học: {_selectedSchoolYear}");
                sb.AppendLine($"Học Kì: {_selectedSemester}");
                sb.AppendLine($"Lớp: {cboClass.Text}");
                sb.AppendLine($"Môn Học: {cboSubject.Text}");
                sb.AppendLine($"Ngày Tạo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine("===================================================");
                sb.AppendLine();

                sb.AppendLine("--- THỐNG KÊ CHUNG ---");
                sb.AppendLine($"Tổng số học sinh: {_cachedReport.Statistics.TotalStudents}");
                sb.AppendLine($"Số học sinh đã nhập điểm: {_cachedReport.Statistics.GradedStudents}");
                sb.AppendLine($"Số học sinh chưa nhập điểm: {_cachedReport.Statistics.TotalStudents - _cachedReport.Statistics.GradedStudents}");
                if (_cachedReport.Statistics.GradedStudents > 0)
                {
                    sb.AppendLine($"Điểm trung bình: {_cachedReport.Statistics.AverageScore:F2}");
                    sb.AppendLine($"Điểm cao nhất: {_cachedReport.Statistics.HighestScore:F2}");
                    sb.AppendLine($"Điểm thấp nhất: {_cachedReport.Statistics.LowestScore:F2}");
                    sb.AppendLine($"Độ lệch chuẩn: {_cachedReport.Statistics.StandardDeviation:F2}");
                    sb.AppendLine($"Tỉ lệ đạt (>=5.0): {_cachedReport.Statistics.PassRate:F1}% ({_cachedReport.Statistics.PassingStudents} học sinh)");
                    sb.AppendLine($"Tỉ lệ kém (<5.0): {_cachedReport.Statistics.FailRate:F1}% ({_cachedReport.Statistics.FailingStudents} học sinh)");
                }
                else
                {
                    sb.AppendLine("Không có dữ liệu điểm đã chấm.");
                }
                sb.AppendLine();

                sb.AppendLine("--- PHÂN BỐ ĐIỂM ---");
                if (_cachedReport.ScoreDistribution != null && _cachedReport.ScoreDistribution.Count > 0)
                {
                    var orderedCategories = new List<string>
                    {
                        "Kém (<5.0)",
                        "Dưới Trung Bình (5.0-5.9)",
                        "Trung Bình (6.0-6.9)",
                        "Khá (7.0-7.9)",
                        "Giỏi (8.0-8.9)",
                        "Xuất Sắc (9.0-10.0)",
                        "Chưa Nhập"
                    };

                    foreach (var category in orderedCategories)
                    {
                        if (_cachedReport.ScoreDistribution.ContainsKey(category))
                        {
                            sb.AppendLine($"- {category}: {_cachedReport.ScoreDistribution[category]} học sinh");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Không có dữ liệu phân bố điểm.");
                }
                sb.AppendLine();

                sb.AppendLine("--- HỌC SINH XUẤT SẮC (Top 10) ---");
                if (_cachedReport.TopPerformers != null && _cachedReport.TopPerformers.Count > 0)
                {
                    int rank = 1;
                    foreach (var student in _cachedReport.TopPerformers.Take(10))
                    {
                        sb.AppendLine($"{rank}. {student.StudentName} - Điểm: {student.FnScore:F2}");
                        rank++;
                    }
                }
                else
                {
                    sb.AppendLine("Không có học sinh xuất sắc.");
                }
                sb.AppendLine();

                sb.AppendLine("--- HỌC SINH CÁ BIỆT (< 5.0) ---");
                if (_cachedReport.AtRiskStudents != null && _cachedReport.AtRiskStudents.Count > 0)
                {
                    int rank = 1;
                    foreach (var student in _cachedReport.AtRiskStudents)
                    {
                        sb.AppendLine($"{rank}. {student.StudentName} - Điểm: {student.FnScore:F2}");
                        rank++;
                    }
                }
                else
                {
                    sb.AppendLine("Không có học sinh cá biệt (điểm dưới 5.0).");
                }
                sb.AppendLine();

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo văn bản: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportReportToCSV(string filePath)
        {
            if (_cachedReport == null) return;

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("BÁO CÁO PHÂN TÍCH ĐIỂM SỐ");
                sb.AppendLine($"Năm Học,{EscapeCsvValue(_selectedSchoolYear)}");
                sb.AppendLine($"Học Kì,{_selectedSemester}");
                sb.AppendLine($"Lớp,{EscapeCsvValue(cboClass.Text)}");
                sb.AppendLine($"Môn Học,{EscapeCsvValue(cboSubject.Text)}");
                sb.AppendLine($"Ngày Tạo,{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine();

                sb.AppendLine("THỐNG KÊ CHUNG");
                sb.AppendLine("Chỉ số,Giá trị");
                sb.AppendLine($"Tổng số học sinh,{_cachedReport.Statistics.TotalStudents}");
                sb.AppendLine($"Số học sinh đã nhập điểm,{_cachedReport.Statistics.GradedStudents}");
                sb.AppendLine($"Số học sinh chưa nhập điểm,{_cachedReport.Statistics.TotalStudents - _cachedReport.Statistics.GradedStudents}");
                if (_cachedReport.Statistics.GradedStudents > 0)
                {
                    sb.AppendLine($"Điểm trung bình,{_cachedReport.Statistics.AverageScore:F2}");
                    sb.AppendLine($"Điểm cao nhất,{_cachedReport.Statistics.HighestScore:F2}");
                    sb.AppendLine($"Điểm thấp nhất,{_cachedReport.Statistics.LowestScore:F2}");
                    sb.AppendLine($"Độ lệch chuẩn,{_cachedReport.Statistics.StandardDeviation:F2}");
                    sb.AppendLine($"Tỉ lệ đạt (>=5.0),{_cachedReport.Statistics.PassRate:F1}%");
                    sb.AppendLine($"Tỉ lệ kém (<5.0),{_cachedReport.Statistics.FailRate:F1}%");
                }
                else
                {
                    sb.AppendLine("Thông báo,Không có dữ liệu điểm đã chấm.");
                }
                sb.AppendLine();

                sb.AppendLine("PHÂN BỐ ĐIỂM");
                sb.AppendLine("Khoảng điểm,Số lượng học sinh");
                if (_cachedReport.ScoreDistribution != null && _cachedReport.ScoreDistribution.Count > 0)
                {
                    var orderedCategories = new List<string>
                    {
                        "Kém (<5.0)",
                        "Dưới Trung Bình (5.0-5.9)",
                        "Trung Bình (6.0-6.9)",
                        "Khá (7.0-7.9)",
                        "Giỏi (8.0-8.9)",
                        "Xuất Sắc (9.0-10.0)",
                        "Chưa Nhập"
                    };

                    foreach (var category in orderedCategories)
                    {
                        if (_cachedReport.ScoreDistribution.ContainsKey(category))
                        {
                            sb.AppendLine($"{EscapeCsvValue(category)},{_cachedReport.ScoreDistribution[category]}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Thông báo,Không có dữ liệu phân bố điểm.");
                }
                sb.AppendLine();

                sb.AppendLine("HỌC SINH XUẤT SẮC (Top 10)");
                sb.AppendLine("STT,Tên Học Sinh,Điểm");
                if (_cachedReport.TopPerformers != null && _cachedReport.TopPerformers.Count > 0)
                {
                    int rank = 1;
                    foreach (var student in _cachedReport.TopPerformers.Take(10))
                    {
                        sb.AppendLine($"{rank},{EscapeCsvValue(student.StudentName)},{student.FnScore:F2}");
                        rank++;
                    }
                }
                else
                {
                    sb.AppendLine("Thông báo,Không có học sinh xuất sắc.");
                }
                sb.AppendLine();

                sb.AppendLine("HỌC SINH CÁ BIỆT (< 5.0)");
                sb.AppendLine("STT,Tên Học Sinh,Điểm");
                if (_cachedReport.AtRiskStudents != null && _cachedReport.AtRiskStudents.Count > 0)
                {
                    int rank = 1;
                    foreach (var student in _cachedReport.AtRiskStudents)
                    {
                        sb.AppendLine($"{rank},{EscapeCsvValue(student.StudentName)},{student.FnScore:F2}");
                        rank++;
                    }
                }
                else
                {
                    sb.AppendLine("Thông báo,Không có học sinh cá biệt (điểm dưới 5.0).");
                }
                sb.AppendLine();

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo CSV: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (value == null) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.Controls.Count > 0 && this.Controls[0] is Panel scrollPanel)
            {
                scrollPanel.Size = this.ClientSize;
                if (btnRefresh != null) btnRefresh.Location = new Point(this.ClientSize.Width - 450, btnRefresh.Location.Y);
                if (btnExport != null) btnExport.Location = new Point(this.ClientSize.Width - 330, btnExport.Location.Y);
                if (btnClose != null) btnClose.Location = new Point(this.ClientSize.Width - 140, btnClose.Location.Y);
                if (progressBar != null) progressBar.Size = new Size(this.ClientSize.Width - (15 * 2), progressBar.Height);
            }
        }
    }
}