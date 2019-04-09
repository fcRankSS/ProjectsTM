﻿using System;
using System.Collections.Generic;

namespace TaskManagement
{
    public class Period
    {
        public Period() { }

        public Period(CallenderDay from, CallenderDay to)
        {
            this.From = from;
            this.To = to;
        }

        public CallenderDay From { set; get; }
        public CallenderDay To { set; get; }

        public bool Contains(CallenderDay day)
        {
            return From.LesserThan(day) && day.LesserThan(To);
        }

        public Period ApplyOffset(int offset, Callender callender)
        {
            var from = callender.ApplyOffset(From, offset);
            var to = callender.ApplyOffset(To, offset);
            if (from == null || to == null) return this;
            return new Period(from, to);
        }

        public Period Clone()
        {
            return new Period(From, To);
        }

        public override bool Equals(object obj)
        {
            return obj is Period period &&
                   EqualityComparer<CallenderDay>.Default.Equals(From, period.From) &&
                   EqualityComparer<CallenderDay>.Default.Equals(To, period.To);
        }

        public override int GetHashCode()
        {
            var hashCode = -1781160927;
            hashCode = hashCode * -1521134295 + EqualityComparer<CallenderDay>.Default.GetHashCode(From);
            hashCode = hashCode * -1521134295 + EqualityComparer<CallenderDay>.Default.GetHashCode(To);
            return hashCode;
        }

        internal bool HasInterSection(Period period)
        {
            if (period.Contains(From)) return true;
            if (period.Contains(To)) return true;
            return false;
        }
    }
}