using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderDAL
{
    public class DFHash
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal PathId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string SHA1 { get; set; }
    }
}
