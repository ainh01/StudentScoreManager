using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using StudentScoreManager.Controllers;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Models.Entities;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class StudentScoreViewForm : Form
    {
        private readonly StudentController _studentController;
        private readonly ClassController _classController;
        private readonly ScoreController _scoreController;

        private int _currentStudentId;
        private string _currentSchoolYear;
        private int _currentSemester;

        private ComboBox cboStudent;
        private ComboBox cboSchoolYear;
        private RadioButton rbSemester1;
        private RadioButton rbSemester2;
        private Button btnLoadScores;
        private DataGridView dgvScores;
        private Chart chartRadar;
        private Chart chartDistribution;

        private Label lblGPA;
        private Label lblHighestScore;
        private Label lblSubjectsPassed;
        private Label lblNeedsImprovement;
        private Label lblLastUpdated;

        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private List<StudentScoreDTO> _printScores;
        private string _printStudentName;
        private int _currentPrintPage = 0;

        public StudentScoreViewForm()
        {
            InitializeComponent();

            _studentController = new StudentController();
            _classController = new ClassController();
            _scoreController = new ScoreController();

            _currentSemester = 1;

            InitializePrintComponents();

            this.Text = "📊 Thành Tích Học Tập Của Tôi";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 800);

            InitializeFormData();

            ConfigureRoleBasedUI();
        }

        private void InitializePrintComponents()
        {
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            printPreviewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                Width = 1000,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Xem Trước Bản In"
            };
        }

        private void InitializeComponent()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            mainLayout.Controls.Add(CreateFilterPanel(), 0, 0);
            mainLayout.Controls.Add(CreateOverviewPanel(), 0, 1);
            mainLayout.Controls.Add(CreateChartsPanel(), 0, 2);
            mainLayout.Controls.Add(CreateDistributionPanel(), 0, 3);
            mainLayout.Controls.Add(CreateActionButtonsPanel(), 0, 4);

            this.Controls.Add(mainLayout);
        }

        private Panel CreateFilterPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(220, 223, 230), ButtonBorderStyle.Solid);
            };

            Label lblStudent = new Label
            {
                Text = "Học Sinh:",
                Location = new Point(10, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            cboStudent = new ComboBox
            {
                Location = new Point(100, 17),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cboStudent.SelectedIndexChanged += CboStudent_SelectedIndexChanged;

            Label lblSchoolYear = new Label
            {
                Text = "Năm Học:",
                Location = new Point(380, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            cboSchoolYear = new ComboBox
            {
                Location = new Point(490, 17),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            cboSchoolYear.SelectedIndexChanged += CboSchoolYear_SelectedIndexChanged;

            Label lblSemester = new Label
            {
                Text = "Học Kì:",
                Location = new Point(670, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            rbSemester1 = new RadioButton
            {
                Text = "Học Kì 1",
                Location = new Point(760, 18),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 10F)
            };
            rbSemester1.CheckedChanged += RbSemester_CheckedChanged;

            rbSemester2 = new RadioButton
            {
                Text = "Học Kì 2",
                Location = new Point(880, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };
            rbSemester2.CheckedChanged += RbSemester_CheckedChanged;

            btnLoadScores = new Button
            {
                Text = "🔍 Tải Điểm",
                Location = new Point(1020, 15),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLoadScores.FlatAppearance.BorderSize = 0;
            btnLoadScores.Click += BtnLoadScores_Click;

            panel.Controls.AddRange(new Control[]
            {
                lblStudent, cboStudent,
                lblSchoolYear, cboSchoolYear,
                lblSemester, rbSemester1, rbSemester2,
                btnLoadScores
            });

            return panel;
        }

        private Panel CreateOverviewPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 10, 0, 10)
            };

            panel.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    panel.ClientRectangle,
                    Color.FromArgb(240, 248, 255),
                    Color.White,
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                }
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(220, 223, 230), ButtonBorderStyle.Solid);
            };

            Label lblTitle = new Label
            {
                Text = "📈 Tổng Quan Thành Tích",
                Location = new Point(15, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            int yPos = 45;
            int col1X = 20, col2X = 380, col3X = 740;

            lblGPA = CreateStatLabel("📈 Điểm Chung: --", new Point(col1X, yPos));
            lblHighestScore = CreateStatLabel("🏆 Điểm Cao Nhất: --", new Point(col2X, yPos));
            lblLastUpdated = CreateStatLabel("📅 Cập Nhật: --", new Point(col3X, yPos));

            yPos += 35;
            lblSubjectsPassed = CreateStatLabel("📊 Môn Đạt: --", new Point(col1X, yPos));
            lblNeedsImprovement = CreateStatLabel("⚠️ Cần Cải Thiện: --", new Point(col2X, yPos));

            panel.Controls.AddRange(new Control[]
            {
                lblTitle, lblGPA, lblHighestScore, lblSubjectsPassed,
                lblNeedsImprovement, lblLastUpdated
            });

            return panel;
        }

        private Label CreateStatLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
        }

        private Panel CreateChartsPanel()
        {
            TableLayoutPanel chartsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            chartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel gridPanel = CreateDataGridPanel();
            chartsLayout.Controls.Add(gridPanel, 0, 0);

            Panel radarPanel = CreateRadarChartPanel();
            chartsLayout.Controls.Add(radarPanel, 1, 0);

            return chartsLayout;
        }

        private Panel CreateDataGridPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 5, 0)
            };

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(220, 223, 230), ButtonBorderStyle.Solid);
            };

            Label lblTitle = new Label
            {
                Text = "📚 Bảng Điểm Chi Tiết",
                Location = new Point(15, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            dgvScores = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(panel.Width - 20, panel.Height - 55),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AllowUserToResizeRows = false,
                Font = new Font("Segoe UI", 9.5F)
            };

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SubjectName",
                HeaderText = "Môn Học",
                Width = 150,
                ReadOnly = true
            });

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "QtScore",
                HeaderText = "QT",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GkScore",
                HeaderText = "GK",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CkScore",
                HeaderText = "CK",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FnScore",
                HeaderText = "Tổng",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
                }
            });

            dgvScores.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            dgvScores.ColumnHeadersHeight = 35;
            dgvScores.RowTemplate.Height = 30;

            dgvScores.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);

            dgvScores.CellFormatting += DgvScores_CellFormatting;

            panel.Controls.AddRange(new Control[] { lblTitle, dgvScores });

            return panel;
        }

        private Panel CreateRadarChartPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10),
                Margin = new Padding(5, 0, 0, 0)
            };

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(220, 223, 230), ButtonBorderStyle.Solid);
            };

            Label lblTitle = new Label
            {
                Text = "🎯 Phân Tích Năng Lực",
                Location = new Point(15, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            chartRadar = new Chart
            {
                Location = new Point(10, 45),
                Size = new Size(panel.Width - 20, panel.Height - 55),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };

            ChartArea chartArea = new ChartArea("RadarArea")
            {
                BackColor = Color.White,
                BorderColor = Color.LightGray,
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 1
            };
            chartRadar.ChartAreas.Add(chartArea);

            Series series = new Series("Điểm")
            {
                ChartType = SeriesChartType.Radar,
                BorderWidth = 3,
                Color = Color.FromArgb(150, 52, 152, 219),
                BackSecondaryColor = Color.FromArgb(100, 52, 152, 219),
                BackGradientStyle = GradientStyle.TopBottom,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                MarkerColor = Color.FromArgb(52, 152, 219),
                MarkerBorderColor = Color.White,
                MarkerBorderWidth = 2
            };
            chartRadar.Series.Add(series);

            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);

            chartArea.AxisY.Maximum = 10;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Interval = 2;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea.AxisY.LabelStyle.Enabled = false;

            Legend legend = new Legend
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9F),
                IsDockedInsideChartArea = false
            };
            chartRadar.Legends.Add(legend);

            Label lblInstruction = new Label
            {
                Text = "💡 Di chuột qua các điểm để xem điểm chi tiết",
                Location = new Point(15, panel.Height - 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            panel.Controls.AddRange(new Control[] { lblTitle, chartRadar, lblInstruction });

            return panel;
        }

        private Panel CreateDistributionPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 10, 0, 10)
            };

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(220, 223, 230), ButtonBorderStyle.Solid);
            };

            Label lblTitle = new Label
            {
                Text = "📊 Biểu Đồ Phân Bố Điểm",
                Location = new Point(15, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            chartDistribution = new Chart
            {
                Location = new Point(15, 45),
                Size = new Size(panel.Width - 30, panel.Height - 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };

            ChartArea chartArea = new ChartArea("DistributionArea")
            {
                BackColor = Color.White
            };
            chartDistribution.ChartAreas.Add(chartArea);

            Series series = new Series("Phân Bố")
            {
                ChartType = SeriesChartType.Bar,
                IsValueShownAsLabel = true,
                LabelFormat = "{0} môn"
            };
            chartDistribution.Series.Add(series);

            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 9F);

            panel.Controls.AddRange(new Control[] { lblTitle, chartDistribution });

            return panel;
        }

        private Panel CreateActionButtonsPanel()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            FlowLayoutPanel buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            Button btnClose = CreateStyledButton("Đóng", Color.FromArgb(127, 140, 141));
            btnClose.Click += (s, e) => this.Close();

            Button btnPrint = CreateStyledButton("🖨️ In", Color.FromArgb(41, 128, 185));
            btnPrint.Click += BtnPrint_Click;

            Button btnExport = CreateStyledButton("📄 Xuất Báo Cáo", Color.FromArgb(41, 128, 185));
            btnExport.Click += BtnExport_Click;

            buttonFlow.Controls.AddRange(new Control[] { btnClose, btnPrint, btnExport });
            panel.Controls.Add(buttonFlow);

            return panel;
        }

        private Button CreateStyledButton(string text, Color backgroundColor)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(150, 40),
                BackColor = backgroundColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Dark(backgroundColor, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = backgroundColor;

            return btn;
        }

        private void InitializeFormData()
        {
            try
            {
                var schoolYears = _classController.GetAllSchoolYears();
                cboSchoolYear.DataSource = schoolYears;
                if (schoolYears.Any())
                {
                    _currentSchoolYear = schoolYears.First();
                    cboSchoolYear.SelectedIndex = 0;
                }

                if (SessionManager.IsAdmin())
                {
                    if (!string.IsNullOrEmpty(_currentSchoolYear))
                    {
                        LoadStudentList(_currentSchoolYear);
                    }
                }
                else if (SessionManager.IsStudent())
                {
                    int? studentId = SessionManager.GetStudentId();
                    if (studentId.HasValue)
                    {
                        _currentStudentId = studentId.Value;
                        LoadStudentScores();
                    }
                    else
                    {
                        MessageBox.Show("Lỗi: Tài khoản học sinh chưa được liên kết đúng cách.",
                            "Lỗi Cấu Hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo form: {ex.Message}",
                    "Lỗi Khởi Tạo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureRoleBasedUI()
        {
            if (SessionManager.IsStudent())
            {
                var student = _studentController.GetStudentById(_currentStudentId);
                cboStudent.Visible = false;
                Label lblStudentName = new Label
                {
                    Text = student?.Name ?? "Unknown",
                    Location = new Point(cboStudent.Location.X, cboStudent.Location.Y + 3),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 152, 219)
                };
                cboStudent.Parent.Controls.Add(lblStudentName);

                this.Text = "📊 Thành Tích Học Tập Của Tôi";
            }
            else if (SessionManager.IsAdmin())
            {
                cboStudent.Visible = true;
                this.Text = "📊 Thành Tích Học Tập Của Học Sinh";
            }
        }

        private void LoadStudentList(string schoolYear = null)
        {
            try
            {
                List<Student> students;

                if (string.IsNullOrEmpty(schoolYear))
                {
                    students = _studentController.SearchStudents("");
                }
                else
                {
                    students = _studentController.GetStudentsBySchoolYear(schoolYear);
                }

                if (students == null || !students.Any())
                {
                    cboStudent.DataSource = null;
                    cboStudent.Enabled = false;
                    MessageBox.Show($"Không tìm thấy học sinh cho năm học {schoolYear ?? "đã chọn"}.",
                        "Không Có Học Sinh", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var classIds = students.Select(s => s.ClassId).Distinct().ToList();

                var classDict = new Dictionary<int, string>();
                foreach (var classId in classIds)
                {
                    var classInfo = _classController.GetClassById(classId);
                    if (classInfo != null)
                    {
                        classDict[classId] = classInfo.Name;
                    }
                    else
                    {
                        classDict[classId] = $"Lớp {classId}";
                    }
                }

                var studentList = students.Select(s => new
                {
                    Id = s.Id,
                    Display = $"{s.Name} (Lớp: {(classDict.ContainsKey(s.ClassId) ? classDict[s.ClassId] : s.ClassId.ToString())})"
                }).OrderBy(s => s.Display).ToList();

                cboStudent.DisplayMember = "Display";
                cboStudent.ValueMember = "Id";
                cboStudent.DataSource = studentList;
                cboStudent.Enabled = true;

                if (studentList.Any())
                {
                    _currentStudentId = studentList.First().Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách học sinh: {ex.Message}",
                    "Lỗi Tải Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentScores()
        {
            try
            {
                if (_currentStudentId <= 0)
                {
                    MessageBox.Show("Vui lòng chọn học sinh.", "Lỗi Xác Thực",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_currentSchoolYear))
                {
                    MessageBox.Show("Vui lòng chọn năm học.", "Lỗi Xác Thực",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;
                btnLoadScores.Enabled = false;
                btnLoadScores.Text = "Đang tải...";

                var scores = _scoreController.GetStudentScores(
                    _currentStudentId,
                    _currentSchoolYear,
                    _currentSemester
                );

                if (scores == null || !scores.Any())
                {
                    MessageBox.Show("Không tìm thấy điểm cho các tham số đã chọn.",
                        "Không Có Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearAllDisplays();
                    return;
                }

                DisplayScoresInGrid(scores);
                DisplayStatistics(scores);
                DisplayRadarChart(scores);
                DisplayDistributionChart(scores);

                lblLastUpdated.Text = $"📅 Ngày Cập Nhật: {DateTime.Now:dd/MM/yyyy HH:mm}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải điểm: {ex.Message}",
                    "Lỗi Tải Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnLoadScores.Enabled = true;
                btnLoadScores.Text = "🔍 Tải Điểm";
            }
        }

        private void DisplayScoresInGrid(List<StudentScoreDTO> scores)
        {
            dgvScores.DataSource = null;
            dgvScores.DataSource = scores;
            dgvScores.Refresh();
        }

        private void DisplayStatistics(List<StudentScoreDTO> scores)
        {
            var gradedScores = scores.Where(s => s.FnScore > 0).ToList();

            if (!gradedScores.Any())
            {
                lblGPA.Text = "📈 Điểm Tổng: Chưa có dữ liệu";
                lblHighestScore.Text = "🏆 Điểm Cao Nhất: --";
                lblSubjectsPassed.Text = "📊 Số Môn Đạt: 0/0";
                lblNeedsImprovement.Text = "⚠️ Số Môn Cần Cải Thiện: 0 môn";
                return;
            }

            decimal gpa = gradedScores.Average(s => s.FnScore) ?? 0m;

            var highestScore = gradedScores.OrderByDescending(s => s.FnScore).First();

            int passedCount = gradedScores.Count(s => s.FnScore >= 5.0m);
            int totalGraded = gradedScores.Count;

            int needsImprovement = gradedScores.Count(s => s.FnScore < 6.0m);

            lblGPA.Text = $"📈 Điểm Tổng: {gpa:F2}";
            lblGPA.ForeColor = GetColorForScore(gpa);

            lblHighestScore.Text = $"🏆 Điểm Cao Nhất: {highestScore.FnScore:F2} ({highestScore.SubjectName})";

            lblSubjectsPassed.Text = $"📊 Số Môn Đạt: {passedCount}/{totalGraded}";
            lblSubjectsPassed.ForeColor = passedCount == totalGraded ? Color.Green : Color.Orange;

            if (needsImprovement > 0)
            {
                lblNeedsImprovement.Text = $"⚠️ Số Môn Cần Cải Thiện: {needsImprovement} môn";
                lblNeedsImprovement.ForeColor = Color.FromArgb(230, 126, 34);
            }
            else
            {
                lblNeedsImprovement.Text = "✅ Tất cả môn học đều đạt!";
                lblNeedsImprovement.ForeColor = Color.FromArgb(39, 174, 96);
            }
        }

        private void DisplayRadarChart(List<StudentScoreDTO> scores)
        {
            chartRadar.Series["Điểm"].Points.Clear();

            var gradedScores = scores.Where(s => s.FnScore > 0).ToList();

            if (!gradedScores.Any())
            {
                return;
            }

            foreach (var score in gradedScores)
            {
                var point = new DataPoint
                {
                    AxisLabel = TruncateSubjectName(score.SubjectName),
                    YValues = new double[] { (double)score.FnScore },
                    ToolTip = $"{score.SubjectName}: {score.FnScore:F2}\n" +
                              $"QT: {score.QtScore:F2} | GK: {score.GkScore:F2} | CK: {score.CkScore:F2}"
                };

                chartRadar.Series["Điểm"].Points.Add(point);
            }

            chartRadar.Invalidate();
        }

        private void DisplayDistributionChart(List<StudentScoreDTO> scores)
        {
            chartDistribution.Series["Phân Bố"].Points.Clear();

            var gradedScores = scores.Where(s => s.FnScore > 0).Select(s => s.FnScore).ToList();

            if (!gradedScores.Any())
            {
                return;
            }

            var categories = new[]
            {
                new { Label = "Xuất Sắc (9.0-10.0)", Min = 9.0m, Max = 10.0m, Color = Color.FromArgb(46, 204, 113) },
                new { Label = "Giỏi (8.0-8.9)", Min = 8.0m, Max = 8.9m, Color = Color.FromArgb(52, 152, 219) },
                new { Label = "Khá (7.0-7.9)", Min = 7.0m, Max = 7.9m, Color = Color.FromArgb(26, 188, 156) },
                new { Label = "Trung Bình (6.0-6.9)", Min = 6.0m, Max = 6.9m, Color = Color.FromArgb(243, 156, 18) },
                new { Label = "Dưới Trung Bình (5.0-5.9)", Min = 5.0m, Max = 5.9m, Color = Color.FromArgb(230, 126, 34) },
                new { Label = "Kém (<5.0)", Min = 0.0m, Max = 4.9m, Color = Color.FromArgb(231, 76, 60) }
            };

            foreach (var category in categories)
            {
                int count = gradedScores.Count(s => s >= category.Min && s <= category.Max);

                var point = new DataPoint
                {
                    AxisLabel = category.Label,
                    YValues = new double[] { count },
                    Color = category.Color,
                    Label = $"{count}",
                    LabelForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };

                chartDistribution.Series["Phân Bố"].Points.Add(point);
            }

            chartDistribution.Invalidate();
        }

        private void ClearAllDisplays()
        {
            dgvScores.DataSource = null;
            chartRadar.Series["Điểm"].Points.Clear();
            chartDistribution.Series["Phân Bố"].Points.Clear();

            lblGPA.Text = "📈 Điểm Tổng: --";
            lblHighestScore.Text = "🏆 Điểm Cao Nhất: --";
            lblSubjectsPassed.Text = "📊 Số Môn Đạt: --";
            lblNeedsImprovement.Text = "⚠️ Số Môn Cần Cải Thiện: --";
            lblLastUpdated.Text = "📅 Ngày Cập Nhật: --";
        }

        private void CboStudent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboStudent.SelectedValue is int studentId)
            {
                _currentStudentId = studentId;
            }
        }

        private void CboSchoolYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSchoolYear.SelectedItem == null)
                return;

            _currentSchoolYear = cboSchoolYear.SelectedItem.ToString();

            if (SessionManager.IsAdmin() && cboStudent.Visible)
            {
                LoadStudentList(_currentSchoolYear);
            }
        }

        private void RbSemester_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSemester1.Checked)
                _currentSemester = 1;
            else if (rbSemester2.Checked)
                _currentSemester = 2;
        }

        private void BtnLoadScores_Click(object sender, EventArgs e)
        {
            _currentSchoolYear = cboSchoolYear.SelectedItem?.ToString();
            LoadStudentScores();
        }

        private void DgvScores_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvScores.Columns[e.ColumnIndex].DataPropertyName.Contains("Score") && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal score))
                {
                    e.CellStyle.ForeColor = GetColorForScore(score);
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Tệp CSV (*.csv)|*.csv|Tệp Văn Bản (*.txt)|*.txt",
                    DefaultExt = "csv",
                    FileName = $"BaoCaoDiem_{_currentStudentId}_{_currentSchoolYear}_HK{_currentSemester}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(saveDialog.FileName);
                    MessageBox.Show($"Báo cáo đã được xuất thành công tới:\n{saveDialog.FileName}",
                        "Xuất Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất báo cáo: {ex.Message}",
                    "Lỗi Xuất", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var scores = dgvScores.DataSource as List<StudentScoreDTO>;
                if (scores == null || !scores.Any())
                {
                    MessageBox.Show("Không có dữ liệu để in.\nVui lòng tải điểm trước khi in.",
                        "Không Có Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var student = _studentController.GetStudentById(_currentStudentId);
                _printStudentName = student?.Name ?? "Không xác định";
                _printScores = scores;

                printPreviewDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}",
                    "Lỗi In", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
                Font headerFont = new Font("Segoe UI", 12, FontStyle.Bold);
                Font normalFont = new Font("Segoe UI", 10);
                Font smallFont = new Font("Segoe UI", 9);

                Brush blackBrush = Brushes.Black;
                Brush grayBrush = Brushes.Gray;
                Pen borderPen = new Pen(Color.Black, 1);

                float yPos = 50;
                float leftMargin = 50;
                float rightMargin = e.PageBounds.Width - 50;

                g.DrawString("BÁO CÁO THÀNH TÍCH HỌC TẬP", titleFont, blackBrush,
                    new PointF(leftMargin, yPos));
                yPos += 40;

                g.DrawString($"Học sinh: {_printStudentName}", headerFont, blackBrush,
                    new PointF(leftMargin, yPos));
                yPos += 30;

                g.DrawString($"Năm học: {_currentSchoolYear}    |    Học kỳ: {_currentSemester}",
                    normalFont, grayBrush, new PointF(leftMargin, yPos));
                yPos += 25;

                g.DrawString($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    smallFont, grayBrush, new PointF(leftMargin, yPos));
                yPos += 40;

                g.DrawLine(borderPen, leftMargin, yPos, rightMargin, yPos);
                yPos += 30;

                g.DrawString("TỔNG QUAN", headerFont, blackBrush,
                    new PointF(leftMargin, yPos));
                yPos += 30;

                var gradedScores = _printScores.Where(s => s.FnScore > 0).ToList();
                if (gradedScores.Any())
                {
                    decimal gpa = gradedScores.Average(s => s.FnScore) ?? 0m;
                    var highestScore = gradedScores.OrderByDescending(s => s.FnScore).First();
                    int passedCount = gradedScores.Count(s => s.FnScore >= 5.0m);

                    g.DrawString($"Điểm Trung Bình: {gpa:F2}", normalFont, blackBrush,
                        new PointF(leftMargin + 20, yPos));
                    yPos += 25;

                    g.DrawString($"Điểm Cao Nhất: {highestScore.FnScore:F2} ({highestScore.SubjectName})",
                        normalFont, blackBrush, new PointF(leftMargin + 20, yPos));
                    yPos += 25;

                    g.DrawString($"Số Môn Đạt: {passedCount}/{gradedScores.Count}",
                        normalFont, blackBrush, new PointF(leftMargin + 20, yPos));
                    yPos += 35;
                }

                g.DrawLine(borderPen, leftMargin, yPos, rightMargin, yPos);
                yPos += 30;

                g.DrawString("BẢNG ĐIỂM CHI TIẾT", headerFont, blackBrush,
                    new PointF(leftMargin, yPos));
                yPos += 30;

                float col1 = leftMargin;
                float col2 = leftMargin + 200;
                float col3 = leftMargin + 300;
                float col4 = leftMargin + 380;
                float col5 = leftMargin + 460;
                float col6 = leftMargin + 540;

                g.FillRectangle(new SolidBrush(Color.FromArgb(52, 152, 219)),
                    col1, yPos, rightMargin - leftMargin, 30);

                g.DrawString("Môn Học", new Font("Segoe UI", 10, FontStyle.Bold),
                    Brushes.White, new PointF(col1 + 5, yPos + 7));
                g.DrawString("QT", new Font("Segoe UI", 10, FontStyle.Bold),
                    Brushes.White, new PointF(col2 + 5, yPos + 7));
                g.DrawString("GK", new Font("Segoe UI", 10, FontStyle.Bold),
                    Brushes.White, new PointF(col3 + 5, yPos + 7));
                g.DrawString("CK", new Font("Segoe UI", 10, FontStyle.Bold),
                    Brushes.White, new PointF(col4 + 5, yPos + 7));
                g.DrawString("Tổng", new Font("Segoe UI", 10, FontStyle.Bold),
                    Brushes.White, new PointF(col5 + 5, yPos + 7));

                yPos += 30;

                bool alternateRow = false;
                foreach (var score in _printScores)
                {
                    if (yPos > e.PageBounds.Height - 100)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    if (alternateRow)
                        g.FillRectangle(new SolidBrush(Color.FromArgb(245, 247, 250)),
                            col1, yPos, rightMargin - leftMargin, 25);

                    g.DrawString(score.SubjectName, normalFont, blackBrush,
                        new PointF(col1 + 5, yPos + 3));
                    g.DrawString($"{score.QtScore:F2}", normalFont, blackBrush,
                        new PointF(col2 + 5, yPos + 3));
                    g.DrawString($"{score.GkScore:F2}", normalFont, blackBrush,
                        new PointF(col3 + 5, yPos + 3));
                    g.DrawString($"{score.CkScore:F2}", normalFont, blackBrush,
                        new PointF(col4 + 5, yPos + 3));
                    g.DrawString($"{score.FnScore ?? 0:F2}", new Font("Segoe UI", 10, FontStyle.Bold),
    new SolidBrush(GetColorForScore(score.FnScore ?? 0)),
    new PointF(col5 + 5, yPos + 3));


                    yPos += 25;
                    alternateRow = !alternateRow;
                }

                g.DrawRectangle(borderPen, col1, yPos - (_printScores.Count * 25) - 30,
                    rightMargin - leftMargin, (_printScores.Count * 25) + 30);

                yPos += 30;

                g.DrawLine(borderPen, leftMargin, yPos, rightMargin, yPos);
                yPos += 20;

                g.DrawString("Hệ Thống Quản Lý Điểm Học Sinh", smallFont, grayBrush,
                    new PointF(leftMargin, yPos));
                g.DrawString($"Trang 1", smallFont, grayBrush,
                    new PointF(rightMargin - 100, yPos));

                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo bản in: {ex.Message}",
                    "Lỗi In", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.HasMorePages = false;
            }
        }

        private Color GetColorForScore(decimal score)
        {
            if (score >= 9.0m) return Color.FromArgb(39, 174, 96);
            if (score >= 8.0m) return Color.FromArgb(52, 152, 219);
            if (score >= 7.0m) return Color.FromArgb(26, 188, 156);
            if (score >= 6.0m) return Color.FromArgb(243, 156, 18);
            if (score >= 5.0m) return Color.FromArgb(230, 126, 34);
            return Color.FromArgb(231, 76, 60);
        }

        private string TruncateSubjectName(string subjectName)
        {
            if (subjectName.Length <= 12)
                return subjectName;

            return subjectName.Substring(0, 10) + "..";
        }

        private void ExportToCSV(string filePath)
        {
            var scores = dgvScores.DataSource as List<StudentScoreDTO>;
            if (scores == null || !scores.Any())
            {
                throw new InvalidOperationException("Không có dữ liệu để xuất");
            }

            using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine("Môn Học,Điểm QT,Điểm GK,Điểm CK,Điểm Tổng");

                foreach (var score in scores)
                {
                    writer.WriteLine($"{score.SubjectName}," +
                                   $"{score.QtScore:F2}," +
                                   $"{score.GkScore:F2}," +
                                   $"{score.CkScore:F2}," +
                                   $"{score.FnScore:F2}");
                }

                writer.WriteLine();
                writer.WriteLine("TỔNG KẾT");

                var gradedScores = scores.Where(s => s.FnScore > 0).ToList();
                if (gradedScores.Any())
                {
                    writer.WriteLine($"Điểm Trung Bình,{gradedScores.Average(s => s.FnScore):F2}");
                    writer.WriteLine($"Số Môn Đạt,{gradedScores.Count(s => s.FnScore >= 5.0m)}");
                    writer.WriteLine($"Tổng Số Môn,{gradedScores.Count}");
                }
                else
                {
                    writer.WriteLine($"Điểm Trung Bình,Chưa có");
                    writer.WriteLine($"Số Môn Đạt,0");
                    writer.WriteLine($"Tổng Số Môn,0");
                }

                writer.WriteLine($"Tạo lúc,{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            }
        }
    }
}