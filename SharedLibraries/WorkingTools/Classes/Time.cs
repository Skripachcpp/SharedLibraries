using System;

namespace WorkingTools.Classes
{
    /// <summary>
    /// Время
    /// </summary>
    public class Time
    {
        public Time() { }
        public Time(int hours, int minutes) { Hours = hours; Minutes = minutes; }

        private int _hours;
        public int Hours { get => _hours; set => _hours = value > 24 ? 24 : value < 0 ? 0 : value; }

        private int _minutes;
        public int Minutes { get => _minutes; set => _minutes = value > 60 ? 60 : value < 0 ? 0 : value; }

        public DateTime ToDateTime() => new DateTime(0, 0, 0, Hours, Minutes, 0);
        public DateTimeOffset ToDateTimeOffset(TimeSpan ofset) => new DateTimeOffset(0, 0, 0, Hours, Minutes, 0, ofset);
        public override string ToString() => $"{(Hours < 9 ? $"0{Hours}" : Hours.ToString())}:{(Minutes < 9 ? $"0{Minutes}" : Minutes.ToString())}";
    }
}
