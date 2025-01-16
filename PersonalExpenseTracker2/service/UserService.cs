
using PersonalExpenseTracker2.model;
using System.Text.Json;

namespace PersonalExpenseTracker2.service
{
    public class UserService
    {
        private readonly string _transactionFilePath;
        private List<User> _users;
        private readonly Store _store;

        public UserService(Store store)
        {
            string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _transactionFilePath = Path.Combine(desktopDirectory, "transaction.json");
            _store = store;
            LoadUsersFromJson();
        }


        //      public UserService() {

        //	string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //	expensePath = Path.Combine(desktopDirectory, "users.json");
        //	LoadUser();
        //}

        private void EnsureTransactionFileExists()
        {
            if (!File.Exists(_transactionFilePath))
            {
                File.WriteAllText(_transactionFilePath, "[]");
            }
        }

        // Load users and expenses from the JSON file
        private void LoadUsersFromJson()
        {
            if (File.Exists(_transactionFilePath))
            {
                var jsonData = File.ReadAllText(_transactionFilePath);
                _users = JsonSerializer.Deserialize<List<User>>(jsonData) ?? new List<User>();
                foreach (var user in _users)
                {
                    if (user.Transactions == null)
                    {
                        user.Transactions = new List<Transaction>();
                    }
                    else if (user.ManageDebts == null)
                    {
                        user.ManageDebts = new List<ManageDebt>();
                    }
                    else if (user.Tags == null)
                    {

                        user.Tags = new List<String>();
                    }
                    

                }
            }
            else
            {
                _users = new List<User>();
                //Create new file if json doesnot exists
                SaveUsersToJson(); 
            }
        }

        // Save users and transactions to the json file
        public void SaveUsersToJson()
        {
            var jsonData = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_transactionFilePath, jsonData);
        }

        // Authenticate the user and return user details if login is successful
        public User? Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                _store.CurrentUser = user;
            }

            
            return user; 
        }

        // Get Transaction list  for the currently logged-in user
        public List<Transaction> GetUserExpenses()
        {
            var user = _store.CurrentUser;
            return user?.Transactions ?? new List<Transaction>();
        }

        // Add a new expense for the currently logged-in user and return true/false
        public bool AddTransactionForUser(Transaction transaction)
        {
            var user = _store.CurrentUser;

            if (user == null)
            {
                throw new Exception("User is not logged in");
            }

            switch (transaction.TransactionType)
            {
                case "Debit":
                    // Check if the total balance is sufficient to make the transaction if not it will return false.
                    if (user.TotalBalance < transaction.Amount)
                    {
                        return false; 
                    }
                    // Deduct the amount from total balance
                    user.TotalBalance -= transaction.Amount; 
                    break;

                case "Credit":
                case "Debt":
                    // Add the amount of debt to the total balance
                    user.TotalBalance += transaction.Amount; 
                    break;

                default:
                    throw new ArgumentException("Invalid Transaction type");
            }

            // Assign a unique ID to the transaction
            transaction.Id = user.Transactions.Any() ? user.Transactions.Max(e => e.Id) + 1 : 1;

            user.Transactions.Add(transaction); // Add the transaction to the user's list
            //save that data into json
            SaveUsersToJson(); 

            //Transaction added successfully 
            return true; 
        }

        // Get Transaction filter by tag for the currently logged-in user
        public List<Transaction> GetTransactionByTag(string tag)
        {
            var user = _store.CurrentUser;
            return user?.Transactions.Where(e => e.TransactionTag == tag).ToList() ?? new List<Transaction>();
        }

        public List<string> GetAllTags()
        {
            var user = _store.CurrentUser;
            List<string> list = user.Tags.ToList();
            return list;
        }

        public User? GetCurrentUser()
        {
            return _store.CurrentUser;
        }

        // Add a new user during first login
        public void AddUser(User newUser)
        {
            //If new user try to add new account it will throw exception
            if (_users.Any(u => u.Username == newUser.Username))
            {
                throw new Exception("User already exists");
            }

            newUser.Transactions = new List<Transaction>();
            _users.Add(newUser);
            SaveUsersToJson();
        }

        // Getting all users if needed 
        public List<User> GetAllUsers()
        {
            return _users;
        }

        public bool AddTagToUser(string tag)
        {
            var user = _store.CurrentUser;
            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            if (user.Tags.Contains(tag))
            {
                return false; // Tag already exists
            }

            user.Tags.Add(tag);
            SaveUsersToJson(); // Persist the updated tags
            return true;
        }

        public List<ManageDebt> GetAllDebtsForUser()
        {
            var user = _store.CurrentUser;
            return user?.ManageDebts ?? new List<ManageDebt>();
        }

        public bool AddDebtForUser(ManageDebt manageDebt)
        {
            var user = _store.CurrentUser;

            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            try
            {
                // Check for sufficient balance before adding the debt
                //if (user.TotalBalance < debt.Amount)
                //{
                //    throw new Exception("Insufficient balance to add debt");
                //}
                // Assign a unique ID to the debt using Guid for uniqueness across sessions
                //debt.Id = user.Debts.Any() ? user.Debts.Max(d => d.Id) + 1 : 1;
                // Add the debt to the user's list
                List<ManageDebt> ManageDebts = user.ManageDebts;
                
                user.ManageDebts.Add(manageDebt);

                // Update the user's balance based on the debt amount
                user.TotalBalance += manageDebt.Amount;

                // Persist the changes to the data storage
                SaveUsersToJson(); // Ensure this persists data correctly

                return true; // Debt added successfully
            }
            catch (Exception e)
            {
                // Log error for debugging and troubleshooting
                // Ideally, use a logger here instead of Console.WriteLine
                Console.WriteLine($"Error adding debt: {e.Message}");
                return false;
            }
        }

        public bool DebtIsPaid(int debtId)
        {
            var user = _store.CurrentUser;

            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            var debt = user.ManageDebts.FirstOrDefault(d => d.Id == debtId);
            if (debt == null)
            {
                throw new Exception("Debt not found");
            }

            if (debt.IsPaid)
            {
                return false; // Debt is already marked as paid
            }

            // Mark the debt as paid
            debt.IsPaid = true;

            // Deduct the debt amount from the user's balance after clearing the debt
            user.TotalBalance -= debt.Amount;

            // Persist changes to the JSON file
            SaveUsersToJson(); // Save the updated list of debts and user balance to JSON

            return true; // Debt successfully marked as paid
        }

        // Method to calculate total inflow (credit)
        public double GetTotalInflow()
        {
            var user = _store.CurrentUser;
            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            return user.Transactions.Where(e => e.TransactionType == "Credit").Sum(e => e.Amount);
        }

        // Method to calculate total outflow (debit)
        public double GetTotalOutflow()
        {
            var user = _store.CurrentUser;
            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            return user.Transactions.Where(e => e.TransactionType == "Debit").Sum(e => e.Amount);
        }

        public int GetRemaingBalance()

        {
            return _store.CurrentUser.TotalBalance;
        }


        // Method to get the total number of inflows (credits)
        public int GetTotalNumberOfInflows()
        {
            var user = _store.CurrentUser;
            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            return user.Transactions.Count(e => e.TransactionType == "Credit");
        }

        // Method to get the total number of outflows (debits)
        public int GetTotalNumberOfOutflows()
        {
            var user = _store.CurrentUser;
            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            return user.Transactions.Count(e => e.TransactionType == "Debit");
        }

        // Method to calculate the total Transaction (inflows + outflows)
        public double GetTotalTransaction()
        {
            var user = _store.CurrentUser;
            if (user == null)
            {
                throw new Exception("No user is logged in");
            }

            return user.Transactions.Sum(e => e.Amount); // Sum of number of transaction of  both debit and credit transactions
        }


    }
}
