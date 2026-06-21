using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TodoManager.Data;
using TodoManager.Models;

namespace TodoManager.Forms
{
    public class MainForm : Form
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        private ListView lvTodos;
        private TextBox  txtSearch;
        private ComboBox cmbFilter;
        private ComboBox cmbCategory;
        private Label    lblTotal, lblDone, lblOverdue, lblRate;
        private ToolStripStatusLabel statusLabel;
        private List<TodoItem> _allItems = new List<TodoItem>();

        public MainForm()
        {
            InitializeComponent();
            RefreshAll();
        }

        private void InitializeComponent()
        {
            Text          = "📝 待辦事項管理系統";
            Size          = new Size(980, 680);
            MinimumSize   = new Size(800, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Microsoft JhengHei", 9.5f);
            BackColor     = Color.FromArgb(241, 245, 249);

            // ── 主容器 ──────────────────────────────────────────────
            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 5,
                ColumnCount = 1,
                BackColor   = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // header
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));   // stats
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));   // toolbar
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // clear btn
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // list

            layout.Controls.Add(BuildHeader(),  0, 0);
            layout.Controls.Add(BuildStats(),   0, 1);
            layout.Controls.Add(BuildToolbar(), 0, 2);
            layout.Controls.Add(BuildClearBar(),0, 3);
            layout.Controls.Add(BuildList(),    0, 4);

            Controls.Add(layout);

            // ── Status bar ──────────────────────────────────────────
            var statusBar = new StatusStrip { BackColor = Color.FromArgb(37, 99, 235) };
            statusLabel = new ToolStripStatusLabel
            {
                Text      = "就緒",
                ForeColor = Color.White,
                Font      = new Font("Microsoft JhengHei", 9f)
            };
            statusBar.Items.Add(statusLabel);
            Controls.Add(statusBar);
        }

        // ── Header ──────────────────────────────────────────────────────────
        private Panel BuildHeader()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(37, 99, 235) };
            p.Controls.Add(new Label
            {
                Text      = "📝 待辦事項管理系統",
                ForeColor = Color.White,
                Font      = new Font("Microsoft JhengHei", 15f, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(16, 10)
            });
            p.Controls.Add(new Label
            {
                Text      = DateTime.Now.ToString("yyyy年MM月dd日"),
                ForeColor = Color.FromArgb(191, 219, 254),
                Font      = new Font("Microsoft JhengHei", 9f),
                AutoSize  = true,
                Location  = new Point(18, 36)
            });
            return p;
        }

        // ── Stats ───────────────────────────────────────────────────────────
        private Panel BuildStats()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10, 10, 10, 10) };

            var cards = new (string title, Color color, int x)[]
            {
                ("📋 全部事項", Color.FromArgb(59,130,246),  10),
                ("✅ 已完成",   Color.FromArgb(16,185,129), 190),
                ("⚠️ 已逾期",   Color.FromArgb(239,68,68),  370),
                ("📊 完成率",    Color.FromArgb(139,92,246), 550),
            };

            var nums = new Label[4];
            for (int i = 0; i < cards.Length; i++)
            {
                var (title, color, x) = cards[i];
                var card = new Panel { Location = new Point(x, 8), Size = new Size(162, 64), BackColor = color };
                card.Controls.Add(new Label { Text = title, ForeColor = Color.White, Font = new Font("Microsoft JhengHei", 8f), Location = new Point(8, 4), AutoSize = true });
                nums[i] = new Label { Text = "0", ForeColor = Color.White, Font = new Font("Microsoft JhengHei", 18f, FontStyle.Bold), Location = new Point(8, 22), AutoSize = true };
                card.Controls.Add(nums[i]);
                p.Controls.Add(card);
            }
            lblTotal = nums[0]; lblDone = nums[1]; lblOverdue = nums[2]; lblRate = nums[3];
            return p;
        }

        // ── Toolbar ─────────────────────────────────────────────────────────
        private Panel BuildToolbar()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };

            var lblS = new Label { Text = "🔍", Location = new Point(10, 13), AutoSize = true, Font = Font };
            txtSearch = new TextBox { Location = new Point(32, 10), Width = 170, Font = Font };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            var lblF = new Label { Text = "狀態:", Location = new Point(215, 13), AutoSize = true, Font = Font };
            cmbFilter = new ComboBox { Location = new Point(250, 10), Width = 105, DropDownStyle = ComboBoxStyle.DropDownList, Font = Font };
            cmbFilter.Items.AddRange(new[] { "全部", "進行中", "已完成", "已逾期" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblC = new Label { Text = "分類:", Location = new Point(368, 13), AutoSize = true, Font = Font };
            cmbCategory = new ComboBox { Location = new Point(402, 10), Width = 105, DropDownStyle = ComboBoxStyle.DropDownList, Font = Font };
            cmbCategory.Items.Add("全部");
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => ApplyFilter();

            var btnAdd    = Btn("➕ 新增",    Color.FromArgb(59,130,246),  520, BtnAdd_Click);
            var btnEdit   = Btn("✏️ 編輯",    Color.FromArgb(16,185,129),  610, BtnEdit_Click);
            var btnDel    = Btn("🗑️ 刪除",    Color.FromArgb(239,68,68),   700, BtnDelete_Click);
            var btnToggle = Btn("✅ 切換完成", Color.FromArgb(139,92,246),  790, BtnToggle_Click);

            p.Controls.AddRange(new Control[] { lblS, txtSearch, lblF, cmbFilter, lblC, cmbCategory, btnAdd, btnEdit, btnDel, btnToggle });
            return p;
        }

        private Panel BuildClearBar()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };
            var btn = new Button
            {
                Text      = "🧹 清除所有已完成事項",
                Location  = new Point(10, 4),
                Width     = 185,
                Height    = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(249, 250, 251),
                ForeColor = Color.FromArgb(107, 114, 128),
                Font      = new Font("Microsoft JhengHei", 8.5f)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btn.Click += BtnClearDone_Click;
            p.Controls.Add(btn);
            return p;
        }

        private ListView BuildList()
        {
            lvTodos = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                MultiSelect   = false,
                Font          = new Font("Microsoft JhengHei", 9.5f),
                BackColor     = Color.White,
                BorderStyle   = BorderStyle.None
            };
            lvTodos.Columns.Add("標題",    260);
            lvTodos.Columns.Add("優先等級",  80);
            lvTodos.Columns.Add("分類",      80);
            lvTodos.Columns.Add("截止日期", 100);
            lvTodos.Columns.Add("狀態",      90);
            lvTodos.Columns.Add("建立時間", 150);
            lvTodos.DoubleClick += BtnEdit_Click;
            return lvTodos;
        }

        private Button Btn(string text, Color color, int x, EventHandler click)
        {
            var b = new Button
            {
                Text      = text,
                Location  = new Point(x, 7),
                Width     = 85,
                Height    = 30,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Microsoft JhengHei", 8.5f, FontStyle.Bold)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            return b;
        }

        // ── Data ────────────────────────────────────────────────────────────
        private void RefreshAll()
        {
            _allItems = _db.GetAllTodos();

            var cats    = _db.GetCategories();
            var prevCat = cmbCategory.SelectedItem?.ToString() ?? "全部";
            cmbCategory.Items.Clear();
            foreach (var c in cats) cmbCategory.Items.Add(c);
            cmbCategory.SelectedItem = cmbCategory.Items.Contains(prevCat) ? prevCat : "全部";

            ApplyFilter();
            UpdateStats();
        }

        private void ApplyFilter()
        {
            string search   = txtSearch.Text.Trim().ToLower();
            string status   = cmbFilter.SelectedItem?.ToString() ?? "全部";
            string category = cmbCategory.SelectedItem?.ToString() ?? "全部";

            var filtered = _allItems.Where(t =>
            {
                bool ms = string.IsNullOrEmpty(search) || t.Title.ToLower().Contains(search) || (t.Description ?? "").ToLower().Contains(search);
                bool mf = status == "全部" || (status == "進行中" && !t.IsCompleted) || (status == "已完成" && t.IsCompleted) || (status == "已逾期" && t.IsOverdue);
                bool mc = category == "全部" || t.Category == category;
                return ms && mf && mc;
            }).ToList();

            lvTodos.BeginUpdate();
            lvTodos.Items.Clear();
            foreach (var t in filtered)
            {
                var lvi = new ListViewItem(t.Title)
                {
                    Tag       = t,
                    ForeColor = t.IsCompleted ? Color.FromArgb(156,163,175) : t.IsOverdue ? Color.FromArgb(239,68,68) : Color.FromArgb(17,24,39),
                    BackColor = t.IsCompleted ? Color.FromArgb(249,250,251) : t.IsOverdue ? Color.FromArgb(254,242,242) : Color.White
                };
                lvi.SubItems.Add(t.PriorityText);
                lvi.SubItems.Add(t.Category);
                lvi.SubItems.Add(t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy/MM/dd") : "—");
                lvi.SubItems.Add(t.StatusText);
                lvi.SubItems.Add(t.CreatedAt.ToString("yyyy/MM/dd HH:mm"));
                lvTodos.Items.Add(lvi);
            }
            lvTodos.EndUpdate();
            statusLabel.Text = $"顯示 {filtered.Count} / {_allItems.Count} 筆事項";
        }

        private void UpdateStats()
        {
            var (total, completed, overdue) = _db.GetStats();
            lblTotal.Text   = total.ToString();
            lblDone.Text    = completed.ToString();
            lblOverdue.Text = overdue.ToString();
            lblRate.Text    = total > 0 ? $"{completed * 100 / total}%" : "0%";
        }

        private TodoItem SelectedItem => lvTodos.SelectedItems.Count > 0 ? lvTodos.SelectedItems[0].Tag as TodoItem : null;

        // ── Handlers ────────────────────────────────────────────────────────
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var cats = _db.GetCategories().Where(c => c != "全部").ToList();
            using var dlg = new TodoEditForm(null, cats);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _db.AddTodo(dlg.Result);
                RefreshAll();
                statusLabel.Text = "✅ 已新增：" + dlg.Result.Title;
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var item = SelectedItem;
            if (item == null) { MessageBox.Show("請先選取一筆事項。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var cats = _db.GetCategories().Where(c => c != "全部").ToList();
            using var dlg = new TodoEditForm(item, cats);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _db.UpdateTodo(dlg.Result);
                RefreshAll();
                statusLabel.Text = "✏️ 已更新：" + dlg.Result.Title;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var item = SelectedItem;
            if (item == null) { MessageBox.Show("請先選取一筆事項。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"確定要刪除「{item.Title}」嗎？", "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _db.DeleteTodo(item.Id);
                RefreshAll();
                statusLabel.Text = "🗑️ 已刪除：" + item.Title;
            }
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            var item = SelectedItem;
            if (item == null) { MessageBox.Show("請先選取一筆事項。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            item.IsCompleted = !item.IsCompleted;
            item.CompletedAt = item.IsCompleted ? DateTime.Now : (DateTime?)null;
            _db.UpdateTodo(item);
            RefreshAll();
            statusLabel.Text = item.IsCompleted ? $"✅ 已完成：{item.Title}" : $"⏳ 恢復進行中：{item.Title}";
        }

        private void BtnClearDone_Click(object sender, EventArgs e)
        {
            int count = _allItems.Count(t => t.IsCompleted);
            if (count == 0) { MessageBox.Show("目前沒有已完成的事項。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"確定要刪除全部 {count} 筆已完成事項嗎？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _db.DeleteCompleted();
                RefreshAll();
                statusLabel.Text = $"🧹 已清除 {count} 筆完成事項";
            }
        }
    }
}
