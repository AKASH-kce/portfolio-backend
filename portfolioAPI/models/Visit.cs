using System;

namespace portfolioAPI.Models
{
    public class Visit
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string IP { get; set; }
        public string Location { get; set; }
        public string UserAgent { get; set; }
        public string Referer { get; set; }
        public string Url { get; set; }
    }
} 