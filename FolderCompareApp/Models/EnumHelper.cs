using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCompareApp.Models
{
    public static class EnumHelper
    {
        public static IEnumerable<ComparisonStatus> GetComparisonStatusValues
        {
            get
            {
                // Add a null value for "All" filter option
                yield return ComparisonStatus.Identical;
                yield return ComparisonStatus.Different;
                yield return ComparisonStatus.LeftOnly;
                yield return ComparisonStatus.RightOnly;
            }
        }
    }
}
