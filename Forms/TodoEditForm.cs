using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TodoManager.Models;

namespace TodoManager.Forms
{
    public class TodoEditForm : Form
    {
        // ── Controls ─────────────────────────────────────────────────────────
        private TextBox txtTitle;
        private TextBox txtDescription;
        private ComboBox cmbPriority;
        private ComboBox cmbCategory;
        private TextBox txtNewCategory;
        private DateTimePicker dtpDueDate;
        private CheckBox chkHasDueDate;
        private CheckBox chkCompleted;
        private Button btnSave;
        private Button btnCancel;

        public TodoItem Result { get; private set; }
        private readonly TodoItem _editing;
        private readonly List<string> _categories;

        public TodoEditForm(TodoItem existing = null, List<string> categories = null)
        {
            _editing    = existing;
            _categories = categories ?? new List<string> { "一般", "工作", "學習", "生活", "購物" };
            InitializeComponent();
            if (_editing != null) LoadData();
        }

        private void InitializeComponent()
        {
            Text            = _editing == null ? "➕ 新增待辦事項" : "✏️ 編輯待辦事項";
            Size            = new Size(460, 420);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.FromArgb(245, 247, 250);
            Font            = new Font("Microsoft JhengHei", 9.5f);

            int lx = 20, cx = 110, w = 300, y = 18, gap = 42;

            // Title
            AddLabel("標題 *", lx, y);
            txtTitle = new TextBox { Location = new Point(cx, y - 2), Width = w, Font = Font };
            Controls.Add(txtTitle);
            y += gap;

            // Description
            AddLabel("描述", lx, y);
            txtDescription = new TextBox
            {
                Location   = new Point(cx, y - 2),
                Width      = w,
                Height     = 60,
                Multiline  = true,
                ScrollBars = ScrollBars.Vertical,
                Font       = Font
            };
            Controls.Add(txtDescription);
            y += gap + 22;

            // Priority
            AddLabel("優先等級", lx, y);
            cmbPriority = new ComboBox
            {
                Location      = new Point(cx, y - 2),
                Width         = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = Font
            };
            cmbPriority.Items.AddRange(new[] { "🟢 低", "🟡 中", "🔴 高" });
            cmbPriority.SelectedIndex = 1;
            Controls.Add(cmbPriority);
            y += gap;

            // Category
            AddLabel("分類", lx, y);
            cmbCategory = new ComboBox
            {
                Location      = new Point(cx, y - 2),
                Width         = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = Font
            };
            foreach (var cat in _categories)
                if (cat != "全部") cmbCategory.Items.Add(cat);
            if (!_categories.Contains("一般")) cmbCategory.Items.Insert(0, "一般");
            cmbCategory.SelectedIndex = 0;
            Controls.Add(cmbCategory);

            var lblNew = new Label { Text = "新分類:", Location = new Point(270, y), AutoSize = true, Font = Font };
            Controls.Add(lblNew);
            txtNewCategory = new TextBox { Location = new Point(320, y - 2), Width = 90, Font = Font };
            Controls.Add(txtNewCategory);
            y += gap;

            // Due Date
            chkHasDueDate = new CheckBox
            {
                Text     = "設定截止日期",
                Location = new Point(lx, y),
                AutoSize = true,
                Font     = Font
            };
            chkHasDueDate.CheckedChanged += (s, e) => dtpDueDate.Enabled = chkHasDueDate.Checked;
            Controls.Add(chkHasDueDate);

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(cx + 30, y - 2),
                Width    = 180,
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today.AddDays(7),
                Enabled  = false,
                Font     = Font
            };
            Controls.Add(dtpDueDate);
            y += gap;

            // Completed (only when editing)
            if (_editing != null)
            {
                chkCompleted = new CheckBox
                {
                    Text     = "標記為已完成",
                    Location = new Point(lx, y),
                    AutoSize = true,
                    Font     = Font
                };
                Controls.Add(chkCompleted);
                y += gap;
            }

            // Buttons
            btnSave = new Button
            {
                Text      = "💾 儲存",
                Location  = new Point(cx, y),
                Width     = 100,
                Height    = 34,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Microsoft JhengHei", 9.5f, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text      = "取消",
                Location  = new Point(cx + 115, y),
                Width     = 80,
                Height    = 34,
                BackColor = Color.FromArgb(229, 231, 235),
                FlatStyle = FlatStyle.Flat,
                Font      = Font
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text      = text,
                Location  = new Point(x, y + 2),
                AutoSize  = true,
                Font      = Font,
                ForeColor = Color.FromArgb(55, 65, 81)
            });
        }

        private void LoadData()
        {
            txtTitle.Text       = _editing.Title;
            txtDescription.Text = _editing.Description;
            cmbPriority.SelectedIndex = (int)_editing.Priority;

            if (cmbCategory.Items.Contains(_editing.Category))
                cmbCategory.SelectedItem = _editing.Category;

            if (_editing.DueDate.HasValue)
            {
                chkHasDueDate.Checked = true;
                dtpDueDate.Value      = _editing.DueDate.Value;
                dtpDueDate.Enabled    = true;
            }

            if (chkCompleted != null)
                chkCompleted.Checked = _editing.IsCompleted;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("請輸入標題！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            string category = !string.IsNullOrWhiteSpace(txtNewCategory.Text)
                ? txtNewCategory.Text.Trim()
                : cmbCategory.SelectedItem?.ToString() ?? "一般";

            Result = _editing ?? new TodoItem();
            Result.Title       = txtTitle.Text.Trim();
            Result.Description = txtDescription.Text.Trim();
            Result.Priority    = (Priority)cmbPriority.SelectedIndex;
            Result.Category    = category;
            Result.DueDate     = chkHasDueDate.Checked ? dtpDueDate.Value.Date : (DateTime?)null;

            if (chkCompleted != null)
            {
                bool wasCompleted  = Result.IsCompleted;
                Result.IsCompleted = chkCompleted.Checked;
                if (!wasCompleted && Result.IsCompleted)
                    Result.CompletedAt = DateTime.Now;
                else if (!Result.IsCompleted)
                    Result.CompletedAt = null;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
