using EdmundsVehicles.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReloadEdmundVehicles
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new EdmundsRepository();
            repo.Reload().Wait();
        }
    }
}
