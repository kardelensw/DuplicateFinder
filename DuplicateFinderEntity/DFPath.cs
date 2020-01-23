using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderDAL
{
    public class DFPath
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public string Path { get; set; }
        public DateTime ScanStarted { get; set; }
        public DateTime? ScanFinished { get; set; }
        public int FileCount { get; set; }
    }
}
