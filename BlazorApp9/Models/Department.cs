using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp9.Models
{
    /// <summary>
    /// 社員情報
    /// </summary>
    [Table("departments")]
    public partial class Department
    {
        /// <summary>
        /// 社員番号
        /// </summary>
        [Key]
        [Column("department_code")]
        public string DepartmentCode { get; set; } = null!;

        /// <summary>
        /// 名前
        /// </summary>
        [Column("department_name")]
        public string DepartmentName { get; set; } = "";
    }

}

