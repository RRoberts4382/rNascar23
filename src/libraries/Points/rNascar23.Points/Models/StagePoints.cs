﻿using System;
using System.Collections.Generic;
using System.Text;

namespace rNascar23.Points.Models
{
    public class StagePoints
    {
        public int position { get; set; }
        public string vehicle_number { get; set; }
        public int driver_id { get; set; }
        public string full_name { get; set; }
        public int stage_points { get; set; }
    }
}