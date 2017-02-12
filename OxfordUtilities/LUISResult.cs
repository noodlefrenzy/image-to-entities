using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxfordUtilities
{
    public class LUISEntity
    {
        public LUISEntity()
        {
            this.EntityValues = new List<string>();
            this.EntityScores = new List<double>();
        }

        public string EntityName { get; set; }
        public IList<string> EntityValues { get; set; }
        public IList<double> EntityScores { get; set; }

        public string FullValue()
        {
            return string.Join(" ", this.EntityValues);
        }
    }

    public class LUISResult
    {
        public LUISResult()
        {
            this.Entities = new Dictionary<string, LUISEntity>();
        }

        public string Intent { get; set; }
        public double IntentScore { get; set; }
        public IDictionary<string, LUISEntity> Entities { get; set; }
    }
}
