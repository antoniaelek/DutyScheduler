using DutyScheduler.Helpers;
using JayMuntzCom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DutyScheduler.Models
{
    public class Month
    {
        public ICollection<Holiday> Holidays { get; }
        public ICollection<SpecialDay> SpecialDays { get; }
        public ICollection<NonWorkingDay> NonWorkingDays { get; }

        public DateTime First { get; }
        public DateTime Last { get; }

        public Month(DateTime day)
        {
            Holidays = new List<Holiday>();
            SpecialDays = new List<SpecialDay>();
            NonWorkingDays = new List<NonWorkingDay>();

            First = new DateTime(day.Year, day.Month, 1);
            Last = new DateTime(day.Year, day.Month, DateTime.DaysInMonth(day.Year, day.Month));

            GetMonth(new DateTime(day.Year, day.Month, 1));
        }

        public Month() : this(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
        {
        }

        private void GetMonth(DateTime day)
        {
            // add holidays and days before holidays
            var holidays = new HolidayCalculator(day, Utils.ReadConfig("Holidays", "Path"));
            foreach (HolidayCalculator.Holiday h in holidays.OrderedHolidays)
            {
                if (h.Date.Month == day.Month) Holidays.Add(new Holiday(h.Name, h.Date));
                var yesterday = h.Date.AddDays(-1);
                if (yesterday.Month == day.Month &&
                    Holidays.FirstOrDefault(hl => hl.Date == yesterday) == default(Holiday))
                    SpecialDays.Add(new SpecialDay("Day before " + h.Name, yesterday.Date));
            }

            // if the last day in month is day before holiday
            var firstInNextMonth = Last.AddDays(1);
            var hol = Holidays.FirstOrDefault(h => h.Date == firstInNextMonth);
            if (hol != null)
                SpecialDays.Add(new SpecialDay("Day before " + hol.Name, Last.Date));

            // get all fridays, if they're not holidays
            var startDate = First;
            while (startDate.DayOfWeek != DayOfWeek.Friday)
                startDate = startDate.AddDays(1);

            while (startDate < Last)
            {
                if (Holidays.FirstOrDefault(h => h.Date == startDate) == default(Holiday))
                    SpecialDays.Add(new SpecialDay("Friday",startDate));
                startDate = startDate.AddDays(7);
            }

            // get all weekends, if they're not holidays
            startDate = First;
            while (startDate.DayOfWeek != DayOfWeek.Saturday)
                startDate = startDate.AddDays(1);

            while (startDate < Last)
            {
                if (Holidays.FirstOrDefault(h => h.Date == startDate) == default(Holiday))
                    NonWorkingDays.Add(new NonWorkingDay(startDate));
                startDate = startDate.AddDays(1);
                if (Holidays.FirstOrDefault(h => h.Date == startDate) == default(Holiday))
                    NonWorkingDays.Add(new NonWorkingDay(startDate));
                startDate = startDate.AddDays(6);
            }
        }
    }
}
