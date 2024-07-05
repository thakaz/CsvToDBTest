using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp9.Models
{
    /// <summary>
    /// 社員情報
    /// </summary>
    [Table("employees")]
    public partial class Employee
    {
        /// <summary>
        /// 社員番号
        /// </summary>
        [Key]
        [Column("employee_code")]
        public string EmployeeCode { get; set; } = null!;

        /// <summary>
        /// 名前
        /// </summary>
        [Column("employee_name")]
        public string EmployeeName { get; set; } = "";

        /// <summary>
        /// 年齢
        /// </summary>
        [Column("employee_age")]
        public int Age{ get; set; }
    }
}
