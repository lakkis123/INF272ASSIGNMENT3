using System;
using System.Runtime.Serialization;

namespace INF272ASSIGNMENT3.Models
{
    [DataContract]
    public class DataPoint
    {
        [DataMember]
        public double Count { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public int Month { get; set; }

        public DataPoint(double count, string label, int month)
        {
            Count = count;
            Label = label;
            Month = month;
        }
    }
}
