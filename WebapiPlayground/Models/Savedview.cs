using System.ComponentModel.DataAnnotations;

namespace WebapiPlayground.Models
{
    public partial class Savedview
    {
        [Key]
        public int Id { get; set; }

        public string? Viewtype { get; set; }

        public string? Saveobj { get; set; }

        public bool? Isglobal { get; set; }

        public DateTime? Timestamp { get; set; }
    }
}
