using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class QuestionAnswer
    {
        public int Id {  get; set; }
        public int TabId { get; set; }
        public string Question {  get; set; }
        public string Answer { get; set; }
        [ForeignKey("TabId")]
        virtual public TabText TabText { get; set; }
    }
}
