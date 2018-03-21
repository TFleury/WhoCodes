using System.Collections.Generic;

namespace WhoCodes.ViewModels
{
    public class QueryResult<T>
    {
        public IEnumerable<T> Results { get; set; }

        public int Count { get; set; }
    }
}
