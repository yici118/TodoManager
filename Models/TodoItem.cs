using System;

namespace TodoManager.Models
{
    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TodoItem()
        {
            CreatedAt = DateTime.Now;
            IsCompleted = false;
            Priority = Priority.Medium;
            Category = "一般";
        }

        public string PriorityText
        {
            get
            {
                switch (Priority)
                {
                    case Priority.High: return "🔴 高";
                    case Priority.Medium: return "🟡 中";
                    case Priority.Low: return "🟢 低";
                    default: return "🟡 中";
                }
            }
        }

        public string StatusText => IsCompleted ? "✅ 已完成" : IsOverdue ? "⚠️ 已逾期" : "⏳ 進行中";

        public bool IsOverdue => !IsCompleted && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;
    }
}
