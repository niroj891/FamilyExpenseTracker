
using System.ComponentModel.DataAnnotations;
namespace PersonalExpenseTracker2.model
{
    public class ManageDebt
    {
        public ManageDebt()
        {

        }

        public int Id { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Must fill the source of debt")]
        public string Source { get; set; }  

        public DateTime? Date { get; set; }
        public bool IsPaid { get; set; } = false;
    }
}
