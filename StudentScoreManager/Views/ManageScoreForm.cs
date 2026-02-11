using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentScoreManager.Controllers;
using StudentScoreManager.Models.DTOs;
using StudentScoreManager.Utils;

namespace StudentScoreManager.Views
{
    public partial class ManageScoreForm : Form
    {
        private readonly ClassController _classController;
        private readonly SubjectController _subjectController;
        private readonly ScoreController _scoreController;

        private int _selectedClassId = 0;
        private int _selectedSubjectId = 0;
        private string _selectedSchoolYear = string.Empty;
        private int _selectedSemester = 0;

        private BindingSource _scoresBindingSource;

        private string _currentSortColumn = string.Empty;
        private SortOrder _currentSortOrder = SortOrder.None;

        public ManageScoreForm()
        {
            InitializeComponent();

            if (!SessionManager.HasRole(2))
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này.",
                    "Từ Chối Truy Cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            _classController = new ClassController();
            _subjectController = new SubjectController();
            _scoreController = new ScoreController();

            _scoresBindingSource = new BindingSource();

            this.Load += ManageScoreForm_Load;
        }

        private Label lblTitle;
        private GroupBox grpFilters;
        private Label lblSchoolYear;
        private ComboBox cboSchoolYear;
        private Label lblClass;
        private ComboBox cboClass;
        private Label lblSubject;
        private ComboBox cboSubject;
        private Label lblSemester;
        private ComboBox cboSemester;
        private Button btnLoad;
        private Button btnAnalyze;
        private Button btnRefresh;
        private DataGridView dgvScores;
        private Label lblStatus;
        private Panel pnlGrid;

        private void InitializeComponent()
        {
            this.ClientSize = new Size(900, 600);
            this.Text = "Quản Lý Điểm - Phầm Mềm Quản Lý Điểm";
            this.StartPosition = FormStartPosition.CenterScreen;

            grpFilters = new GroupBox
            {
                Text = "Chọn Lớp và Môn học",
                Location = new Point(20, 60),
                Size = new Size(860, 120),
                Font = new Font("Segoe UI", 10F)
            };

            cboSchoolYear = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 30),
                Size = new Size(150, 25)
            };
            cboSchoolYear.SelectedIndexChanged += CboSchoolYear_SelectedIndexChanged;

            cboClass = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 65),
                Size = new Size(200, 25),
                Enabled = false
            };
            cboClass.SelectedIndexChanged += CboClass_SelectedIndexChanged;

            cboSubject = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(470, 30),
                Size = new Size(200, 25),
                Enabled = false
            };
            cboSubject.SelectedIndexChanged += CboSubject_SelectedIndexChanged;

            cboSemester = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(470, 65),
                Size = new Size(100, 25)
            };
            cboSemester.Items.AddRange(new object[] { "1", "2" });
            cboSemester.SelectedIndex = 0;
            cboSemester.SelectedIndexChanged += CboSemester_SelectedIndexChanged;

            btnLoad = new Button
            {
                Text = "Tải Điểm",
                Location = new Point(700, 30),
                Size = new Size(120, 60),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false
            };
            btnLoad.Click += BtnLoad_Click;

            btnAnalyze = new Button
            {
                Text = "📊 Phân Tích",
                Location = new Point(20, 190),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Enabled = false
            };
            btnAnalyze.Click += BtnAnalyze_Click;

            btnRefresh = new Button
            {
                Text = "🔄 Làm Mới",
                Location = new Point(130, 190),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Enabled = false
            };
            btnRefresh.Click += BtnRefresh_Click;

            dgvScores = new DataGridView
            {
                Location = new Point(20, 230),
                Size = new Size(860, 310),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                RowHeadersVisible = false,
                AutoGenerateColumns = false
            };

            ConfigureGridColumns();

            dgvScores.CellDoubleClick += DgvScores_CellDoubleClick;
            dgvScores.ColumnHeaderMouseClick += DgvScores_ColumnHeaderMouseClick;

            lblStatus = new Label
            {
                Location = new Point(20, 550),
                Size = new Size(860, 20),
                Text = "Chọn bộ lọc và bấm 'Tải Điểm' để bắt đầu.",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9F)
            };

            grpFilters.Controls.AddRange(new Control[] {
                new Label { Text = "Năm Học:", Location = new Point(20, 32), Size = new Size(90, 20) },
                cboSchoolYear,
                new Label { Text = "Lớp:", Location = new Point(20, 67), Size = new Size(90, 20) },
                cboClass,
                new Label { Text = "Môn Học:", Location = new Point(350, 32), Size = new Size(110, 20) },
                cboSubject,
                new Label { Text = "Học Kì:", Location = new Point(350, 67), Size = new Size(110, 20) },
                cboSemester,
                btnLoad
            });

            this.Controls.AddRange(new Control[] {
                new Label {
                    Text = "Quản Lý Điểm",
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    Location = new Point(20, 20),
                    Size = new Size(300, 30),
                    ForeColor = Color.FromArgb(41, 128, 185)
                },
                grpFilters,
                btnAnalyze,
                btnRefresh,
                dgvScores,
                lblStatus
            });
        }

        private void ConfigureGridColumns()
        {
            dgvScores.Columns.Clear();

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStudentId",
                DataPropertyName = "StudentId",
                Visible = false
            });

            var colRowNum = new DataGridViewTextBoxColumn
            {
                Name = "colRowNum",
                HeaderText = "#",
                Width = 50,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    BackColor = Color.FromArgb(240, 240, 240)
                }
            };
            dgvScores.Columns.Add(colRowNum);

            dgvScores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStudentName",
                HeaderText = "Tên Học Sinh",
                DataPropertyName = "StudentName",
                Width = 300,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular)
                }
            });

            var colFinalScore = new DataGridViewTextBoxColumn
            {
                Name = "colFinalScore",
                HeaderText = "Điểm Tổng Kết",
                DataPropertyName = "FnScore",
                Width = 150,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Format = "N2",
                    NullValue = "Chưa Nhập",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            };
            dgvScores.Columns.Add(colFinalScore);

            var colPerformance = new DataGridViewTextBoxColumn
            {
                Name = "colPerformance",
                HeaderText = "Học Lực",
                Width = 150,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dgvScores.Columns.Add(colPerformance);

            dgvScores.CellFormatting += DgvScores_CellFormatting;
        }

        private void DgvScores_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || dgvScores.Rows.Count == 0)
                return;

            var clickedColumn = dgvScores.Columns[e.ColumnIndex];

            if (_currentSortColumn == clickedColumn.Name)
            {
                _currentSortOrder = _currentSortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _currentSortColumn = clickedColumn.Name;
                _currentSortOrder = SortOrder.Ascending;
            }

            SortGridData(_currentSortColumn, _currentSortOrder);

            UpdateColumnHeaderSortIndicators();
        }

        private void SortGridData(string columnName, SortOrder sortOrder)
        {
            try
            {
                var dataSource = _scoresBindingSource.DataSource as List<ScoreSummaryDTO>;
                if (dataSource == null || !dataSource.Any())
                    return;

                List<ScoreSummaryDTO> sortedList;

                switch (columnName)
                {
                    case "colRowNum":
                    case "colStudentId":
                        sortedList = sortOrder == SortOrder.Ascending
                            ? dataSource.OrderBy(x => x.StudentId).ToList()
                            : dataSource.OrderByDescending(x => x.StudentId).ToList();
                        break;

                    case "colStudentName":
                        sortedList = sortOrder == SortOrder.Ascending
                            ? dataSource.OrderBy(x => x.StudentName).ToList()
                            : dataSource.OrderByDescending(x => x.StudentName).ToList();
                        break;

                    case "colFinalScore":
                        sortedList = sortOrder == SortOrder.Ascending
                            ? dataSource.OrderBy(x => x.FnScore ?? decimal.MinValue).ToList()
                            : dataSource.OrderByDescending(x => x.FnScore ?? decimal.MinValue).ToList();
                        break;

                    case "colPerformance":
                        sortedList = sortOrder == SortOrder.Ascending
                            ? dataSource.OrderBy(x => x.FnScore ?? decimal.MinValue).ToList()
                            : dataSource.OrderByDescending(x => x.FnScore ?? decimal.MinValue).ToList();
                        break;

                    default:
                        return;
                }

                _scoresBindingSource.DataSource = sortedList;
                dgvScores.Refresh();

                AddRowNumbers();

                lblStatus.Text = $"Xếp theo {dgvScores.Columns[columnName].HeaderText} ({(sortOrder == SortOrder.Ascending ? "Tăng dần" : "Giảm dần")})";
            }
            catch (Exception ex)
            {
                ShowError($"Không xếp được: {ex.Message}");
            }
        }

        private void UpdateColumnHeaderSortIndicators()
        {
            foreach (DataGridViewColumn col in dgvScores.Columns)
            {
                if (col.Name == _currentSortColumn)
                {
                    string baseHeaderText = col.HeaderText.Replace(" ▲", "").Replace(" ▼", "");
                    col.HeaderText = baseHeaderText + (_currentSortOrder == SortOrder.Ascending ? " ▲" : " ▼");

                    col.HeaderCell.SortGlyphDirection = _currentSortOrder;
                }
                else
                {
                    col.HeaderText = col.HeaderText.Replace(" ▲", "").Replace(" ▼", "");
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
        }

        private void ManageScoreForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = $"Quản lý điểm - {SessionManager.DisplayName} ({SessionManager.RoleName})";

                LoadSchoolYears();

                lblStatus.Text = $"Xin chào, {SessionManager.DisplayName}!";
            }
            catch (Exception ex)
            {
                ShowError($"Không tạo form được: {ex.Message}");
            }
        }

        private void LoadSchoolYears()
        {
            try
            {
                var schoolYears = _classController.GetAllSchoolYears();

                if (schoolYears == null || !schoolYears.Any())
                {
                    MessageBox.Show("Không tìm thấy năm học nào. Hãy liên hệ Admin.",
                        "Không Có Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                cboSchoolYear.Items.Clear();
                cboSchoolYear.Items.AddRange(schoolYears.ToArray());

                cboSchoolYear.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Không đọc được Năm Học: {ex.Message}");
            }
        }

        private void CboSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSemester.SelectedItem == null)
                return;

            cboClass.DataSource = null;
            cboClass.Enabled = false;
            cboSubject.DataSource = null;
            cboSubject.Enabled = false;
            btnLoad.Enabled = false;
            btnAnalyze.Enabled = false;
            btnRefresh.Enabled = false;

            ClearGrid();

            try
            {
                if (string.IsNullOrEmpty(_selectedSchoolYear))
                {
                    lblStatus.Text = "Hãy chọn Năm Học.";
                    return;
                }

                int semester = int.Parse(cboSemester.SelectedItem.ToString());

                var classes = _classController.GetClassesForCurrentUser(_selectedSchoolYear, semester);

                if (classes == null || !classes.Any())
                {
                    lblStatus.Text = $"Không tìm được Lớp học nào trong Học Kì {semester}.";
                    return;
                }

                cboClass.DisplayMember = "Name";
                cboClass.ValueMember = "Id";
                cboClass.DataSource = classes;
                cboClass.Enabled = true;

                lblStatus.Text = $"Học Kì {semester}: {classes.Count} Lớp.";
            }
            catch (Exception ex)
            {
                ShowError($"Không đọc được thông tin Lớp Học theo Học Kì: {ex.Message}");
            }
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
            btnLoad.Enabled = false;
            btnAnalyze.Enabled = false;
            btnRefresh.Enabled = false;

            ClearGrid();

            try
            {
                if (cboSemester.SelectedItem == null)
                {
                    lblStatus.Text = "Hãy chọn Học Kì.";
                    return;
                }

                int semester = int.Parse(cboSemester.SelectedItem.ToString());

                var classes = _classController.GetClassesForCurrentUser(_selectedSchoolYear, semester);

                if (classes == null || !classes.Any())
                {
                    lblStatus.Text = "Không tồn tại lớp học nào trong năm được chọn.";
                    return;
                }

                cboClass.DisplayMember = "Name";
                cboClass.ValueMember = "Id";
                cboClass.DataSource = classes;
                cboClass.Enabled = true;

                lblStatus.Text = $"Có {classes.Count} Lớp trong Năm học {_selectedSchoolYear}.";
            }
            catch (Exception ex)
            {
                ShowError($"Không đọc được thông tin lớp đã chọn: {ex.Message}");
            }
        }

        private void CboClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboClass.SelectedValue == null)
                {
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
                    return;
                }

                _selectedClassId = classId;

                cboSubject.DataSource = null;
                cboSubject.Enabled = false;
                btnLoad.Enabled = false;
                btnAnalyze.Enabled = false;
                btnRefresh.Enabled = false;

                ClearGrid();

                if (cboSemester.SelectedItem == null)
                {
                    lblStatus.Text = "Chưa chọn Học Kì.";
                    return;
                }

                int semester = int.Parse(cboSemester.SelectedItem.ToString());

                if (string.IsNullOrEmpty(_selectedSchoolYear))
                {
                    lblStatus.Text = "Chưa chọn Năm Học.";
                    return;
                }

                var subjects = _subjectController.GetSubjectsForCurrentUser(
                    _selectedClassId,
                    _selectedSchoolYear,
                    semester);

                if (subjects == null || !subjects.Any())
                {
                    lblStatus.Text = "⚠️ Không tồn tại Môn Học nào trong Lớp được chọn.";
                    return;
                }

                cboSubject.DisplayMember = "Name";
                cboSubject.ValueMember = "Id";
                cboSubject.DataSource = subjects;
                cboSubject.Enabled = true;

                lblStatus.Text = $"✅ Có {subjects.Count} môn học cho lớp đã chọn.";
            }
            catch (Exception ex)
            {
                ShowError($"Không tải được môn học: {ex.Message}");
            }
        }

        private void CboSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSubject.SelectedValue == null)
                return;

            if (!(cboSubject.SelectedValue is int subjectId))
                return;

            _selectedSubjectId = subjectId;

            btnLoad.Enabled = true;
            lblStatus.Text = "Bấm 'Tải Điểm' để xem điểm học sinh.";
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (_selectedClassId == 0 || _selectedSubjectId == 0 || string.IsNullOrEmpty(_selectedSchoolYear))
            {
                MessageBox.Show("Vui lòng chọn đầy đủ bộ lọc trước khi tải điểm.",
                    "Chưa Chọn Đủ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selectedSemester = int.Parse(cboSemester.SelectedItem.ToString());

            LoadScoreData();
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvScores.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để phân tích. Vui lòng tải điểm trước.",
                        "Không Có Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var scores = new List<decimal>();
                int gradedCount = 0;
                int notGradedCount = 0;
                int excellentCount = 0;
                int goodCount = 0;
                int averageCount = 0;
                int failingCount = 0;

                foreach (DataGridViewRow row in dgvScores.Rows)
                {
                    var scoreCell = row.Cells["colFinalScore"];
                    if (scoreCell.Value != null && scoreCell.Value != DBNull.Value)
                    {
                        decimal score = Convert.ToDecimal(scoreCell.Value);
                        scores.Add(score);
                        gradedCount++;

                        if (score >= 9.0m) excellentCount++;
                        else if (score >= 8.0m) goodCount++;
                        else if (score >= 5.0m) averageCount++;
                        else failingCount++;
                    }
                    else
                    {
                        notGradedCount++;
                    }
                }

                if (gradedCount == 0)
                {
                    MessageBox.Show("Không tìm thấy học sinh nào đã có điểm.",
                        "Phân Tích", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                decimal avgScore = scores.Average();
                decimal maxScore = scores.Max();
                decimal minScore = scores.Min();
                decimal passRate = (decimal)(gradedCount - failingCount) / gradedCount * 100;

                string analysisMessage = $"📊 PHÂN TÍCH NHANH\n\n" +
                    $"Số Học Sinh: {dgvScores.Rows.Count}\n" +
                    $"Đã Nhập: {gradedCount} | Chưa Nhập: {notGradedCount}\n\n" +
                    $"━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                    $"📈 Thống Kê:\n" +
                    $"   • Điểm Trung Bình: {avgScore:N2}\n" +
                    $"   • Điểm Cao Nhất: {maxScore:N2}\n" +
                    $"   • Điểm Thấp Nhất: {minScore:N2}\n" +
                    $"   • Tỉ Lệ Đạt: {passRate:N1}%\n\n" +
                    $"🎯 Phổ Điểm:\n" +
                    $"   • Xuất Sắc (≥9.0): {excellentCount}\n" +
                    $"   • Giỏi (≥8.0): {goodCount}\n" +
                    $"   • Trung Bình (5.0-7.9): {averageCount}\n" +
                    $"   • Kém (<5.0): {failingCount}";

                MessageBox.Show(analysisMessage, "Phân Tích Nhanh",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblStatus.Text = $"Phân tích hoàn tất: TB={avgScore:N2}, Tỉ Lệ Đạt={passRate:N1}%";
            }
            catch (Exception ex)
            {
                ShowError($"Không phân tích được điểm: {ex.Message}");
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (_selectedClassId == 0 || _selectedSubjectId == 0 || string.IsNullOrEmpty(_selectedSchoolYear))
            {
                MessageBox.Show("Vui lòng chọn đầy đủ bộ lọc trước khi làm mới.",
                    "Chưa Chọn Đủ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadScoreData();
            lblStatus.Text = "Dữ liệu đã được làm mới thành công.";
        }

        private void LoadScoreData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Đang tải điểm...";
                btnLoad.Enabled = false;

                var scoreSummary = _scoreController.GetScoreSummary(
                    _selectedClassId,
                    _selectedSubjectId,
                    _selectedSchoolYear,
                    _selectedSemester);

                if (scoreSummary == null || !scoreSummary.Any())
                {
                    ClearGrid();
                    lblStatus.Text = "Không tìm thấy học sinh nào trong lớp này.";
                    MessageBox.Show("Không có học sinh nào được ghi danh vào lớp đã chọn.",
                        "Không Có Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnAnalyze.Enabled = false;
                    btnRefresh.Enabled = false;
                    return;
                }

                _currentSortColumn = string.Empty;
                _currentSortOrder = SortOrder.None;
                UpdateColumnHeaderSortIndicators();

                _scoresBindingSource.DataSource = scoreSummary;
                dgvScores.DataSource = _scoresBindingSource;

                AddRowNumbers();

                int gradedCount = scoreSummary.Count(s => s.FnScore.HasValue);
                lblStatus.Text = $"Đã tải {scoreSummary.Count} học sinh. {gradedCount} có điểm, {scoreSummary.Count - gradedCount} chưa có. Bấm tiêu đề cột để sắp xếp.";

                btnAnalyze.Enabled = true;
                btnRefresh.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowError($"Không tải được điểm: {ex.Message}");
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnLoad.Enabled = true;
            }
        }

        private void AddRowNumbers()
        {
            for (int i = 0; i < dgvScores.Rows.Count; i++)
            {
                dgvScores.Rows[i].Cells["colRowNum"].Value = (i + 1).ToString();
            }
        }

        private void ClearGrid()
        {
            _scoresBindingSource.DataSource = null;
            dgvScores.DataSource = null;

            _currentSortColumn = string.Empty;
            _currentSortOrder = SortOrder.None;
            UpdateColumnHeaderSortIndicators();
        }

        private void DgvScores_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvScores.Columns[e.ColumnIndex].Name == "colPerformance" ||
                dgvScores.Columns[e.ColumnIndex].Name == "colFinalScore")
            {
                var scoreCell = dgvScores.Rows[e.RowIndex].Cells["colFinalScore"];

                if (scoreCell.Value == null || scoreCell.Value == DBNull.Value)
                {
                    if (dgvScores.Columns[e.ColumnIndex].Name == "colPerformance")
                    {
                        e.Value = "Chưa Nhập";
                        e.CellStyle.ForeColor = Color.Gray;
                        e.CellStyle.BackColor = Color.FromArgb(245, 245, 245);
                    }
                    return;
                }

                decimal score = Convert.ToDecimal(scoreCell.Value);

                string performance;
                Color backColor;
                Color foreColor = Color.White;

                if (score >= 9.0m)
                {
                    performance = "Xuất Sắc";
                    backColor = Color.FromArgb(39, 174, 96);
                }
                else if (score >= 8.0m)
                {
                    performance = "Giỏi";
                    backColor = Color.FromArgb(52, 152, 219);
                }
                else if (score >= 7.0m)
                {
                    performance = "Khá";
                    backColor = Color.FromArgb(26, 188, 156);
                }
                else if (score >= 6.0m)
                {
                    performance = "Trung Bình";
                    backColor = Color.FromArgb(241, 196, 15);
                    foreColor = Color.Black;
                }
                else if (score >= 5.0m)
                {
                    performance = "Dưới Trung Bình";
                    backColor = Color.FromArgb(230, 126, 34);
                }
                else
                {
                    performance = "Kém";
                    backColor = Color.FromArgb(231, 76, 60);
                }

                if (dgvScores.Columns[e.ColumnIndex].Name == "colPerformance")
                {
                    e.Value = performance;
                    e.CellStyle.BackColor = backColor;
                    e.CellStyle.ForeColor = foreColor;
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.BackColor = ControlPaint.Light(backColor, 0.8f);

                    if (score < 5.0m)
                        e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                    else
                        e.CellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void DgvScores_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            try
            {
                var studentIdCell = dgvScores.Rows[e.RowIndex].Cells["colStudentId"];

                if (studentIdCell.Value == null)
                    return;

                int studentId = Convert.ToInt32(studentIdCell.Value);
                string studentName = dgvScores.Rows[e.RowIndex].Cells["colStudentName"].Value.ToString();

                using (var detailForm = new ScoreDetailForm(
                    studentId,
                    _selectedSubjectId,
                    _selectedSchoolYear,
                    _selectedSemester,
                    studentName))
                {
                    var result = detailForm.ShowDialog(this);

                    if (result == DialogResult.OK)
                    {
                        LoadScoreData();

                        if (e.RowIndex < dgvScores.Rows.Count)
                            dgvScores.Rows[e.RowIndex].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Không mở được chi tiết điểm: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Đã xảy ra lỗi. Vui lòng thử lại.";
        }
    }
}
