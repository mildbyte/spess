using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess.Exchange
{
    class Account
    {
        Owner owner;
        Exchange exchange;

        Inventory escrowGoods;
        Inventory storedGoods;

        int escrowMoney;
    }
}
