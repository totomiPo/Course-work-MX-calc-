using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CalcMatrix.Data;

public partial class ExceptionCourse
{
    [Key]
    public int IdExc { get; set; }
    public string Message { get; set; }

    public string TargetSite { get; set; }

    public DateTime? DateTimeExc { get; set; }
}
