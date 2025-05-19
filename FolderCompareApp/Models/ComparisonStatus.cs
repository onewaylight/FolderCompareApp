using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCompareApp.Models
{
    /// <summary>
    /// defines the status of a file comparison
    /// </summary>
    public enum ComparisonStatus
    {
        Identical,
        Different,
        LeftOnly,
        RightOnly
    }
}
