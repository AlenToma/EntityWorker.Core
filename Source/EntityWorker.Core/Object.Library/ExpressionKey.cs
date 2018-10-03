using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EntityWorker.Core.Object.Library
{
    internal class ExpressionKey
    {
        public System.Linq.Expressions.Expression Expression { get; set; }

        public List<System.Linq.Expressions.Expression> OrderBy { get; set; } = new List<System.Linq.Expressions.Expression>();
    }
}
