using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using TodoManager.Models;

namespace TodoManager.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TodoManager");
            Directory.CreateDirectory(appData);
            string dbPath = Path.Combine(appData, "todos.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = @"
                CREATE TABLE IF NOT EXISTS Todos (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title       TEXT    NOT NULL,
                    Description TEXT,
                    Priority    INTEGER NOT NULL DEFAULT 1,
                    Category    TEXT    NOT NULL DEFAULT '一般',
                    CreatedAt   TEXT    NOT NULL,
                    DueDate     TEXT,
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    CompletedAt TEXT
                );";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public int AddTodo(TodoItem item)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = @"
                INSERT INTO Todos (Title, Description, Priority, Category, CreatedAt, DueDate, IsCompleted, CompletedAt)
                VALUES (@Title, @Description, @Priority, @Category, @CreatedAt, @DueDate, @IsCompleted, @CompletedAt);
                SELECT last_insert_rowid();";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? "");
            cmd.Parameters.AddWithValue("@Priority", (int)item.Priority);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@CreatedAt", item.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("@DueDate", item.DueDate?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsCompleted", item.IsCompleted ? 1 : 0);
            cmd.Parameters.AddWithValue("@CompletedAt", item.CompletedAt?.ToString("o") ?? (object)DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<TodoItem> GetAllTodos()
        {
            var list = new List<TodoItem>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = "SELECT * FROM Todos ORDER BY IsCompleted ASC, Priority DESC, CreatedAt DESC;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapReader(reader));
            return list;
        }

        public List<string> GetCategories()
        {
            var cats = new List<string> { "全部" };
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = "SELECT DISTINCT Category FROM Todos ORDER BY Category;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string cat = reader.GetString(0);
                if (!cats.Contains(cat)) cats.Add(cat);
            }
            return cats;
        }

        public void UpdateTodo(TodoItem item)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = @"
                UPDATE Todos SET
                    Title       = @Title,
                    Description = @Description,
                    Priority    = @Priority,
                    Category    = @Category,
                    DueDate     = @DueDate,
                    IsCompleted = @IsCompleted,
                    CompletedAt = @CompletedAt
                WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? "");
            cmd.Parameters.AddWithValue("@Priority", (int)item.Priority);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@DueDate", item.DueDate?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsCompleted", item.IsCompleted ? 1 : 0);
            cmd.Parameters.AddWithValue("@CompletedAt", item.CompletedAt?.ToString("o") ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTodo(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Todos WHERE Id = @Id;", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteCompleted()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Todos WHERE IsCompleted = 1;", conn);
            cmd.ExecuteNonQuery();
        }

        public (int total, int completed, int overdue) GetStats()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string sql = @"
                SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN IsCompleted = 1 THEN 1 ELSE 0 END) AS Completed,
                    SUM(CASE WHEN IsCompleted = 0 AND DueDate IS NOT NULL
                                  AND DATE(DueDate) < DATE('now') THEN 1 ELSE 0 END) AS Overdue
                FROM Todos;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int total   = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                int done    = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                int overdue = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                return (total, done, overdue);
            }
            return (0, 0, 0);
        }

        private TodoItem MapReader(SqliteDataReader r)
        {
            return new TodoItem
            {
                Id          = r.GetInt32(r.GetOrdinal("Id")),
                Title       = r.GetString(r.GetOrdinal("Title")),
                Description = r["Description"] as string ?? "",
                Priority    = (Priority)r.GetInt32(r.GetOrdinal("Priority")),
                Category    = r.GetString(r.GetOrdinal("Category")),
                CreatedAt   = DateTime.Parse(r.GetString(r.GetOrdinal("CreatedAt"))),
                DueDate     = r["DueDate"] is DBNull ? (DateTime?)null : DateTime.Parse((string)r["DueDate"]),
                IsCompleted = r.GetInt32(r.GetOrdinal("IsCompleted")) == 1,
                CompletedAt = r["CompletedAt"] is DBNull ? (DateTime?)null : DateTime.Parse((string)r["CompletedAt"])
            };
        }
    }
}
