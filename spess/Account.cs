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

        public Owner Owner { get { return owner; } }
        public Exchange Exchange { get { return exchange; } }
        public Inventory EscrowGoods { get { return escrowGoods; } set { escrowGoods = value; } }
        public Inventory StoredGoods { get { return storedGoods; } set { storedGoods = value; } }
        public int EscrowMoney { get { return escrowMoney; } set {escrowMoney = value;} }

        public Account(Owner owner, Exchange exchange)
        {
            this.owner = owner; this.exchange = exchange;
            escrowGoods = new Inventory();
            storedGoods = new Inventory();
            escrowMoney = 0;
        }
    }
}
