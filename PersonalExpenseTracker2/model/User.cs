using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalExpenseTracker2.model
{
	public class User
	{
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public List<string> Tags { get; set; } = new List<string>();
        public int TotalBalance { get; set; }

        public List<ManageDebt> ManageDebts { get; set; } = new List<ManageDebt>();

    }

}
