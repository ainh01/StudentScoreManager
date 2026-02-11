using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentScoreManager.Models.DTOs
{
    public class SubjectAverageDTO
    {
        public string SubjectName { get; set; }
        public decimal? AverageScore { get; set; }
    }
}

