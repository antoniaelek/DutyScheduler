﻿using System;

namespace DutyScheduler.Models
{
    public class Day : IDay
    {
        public DateTime Date { get; }
        public string Name { get; }
        public string Type { get; } = "ordinary";
        public bool IsReplaceable { get; set; }
        public string Scheduled { get; set; }

        public Day(DateTime date)
        {
            Date = date;
            Name = null;
        }

        public override string ToString()
        {
            return Date.ToString("d.M.yyyy");
        }
    }
}