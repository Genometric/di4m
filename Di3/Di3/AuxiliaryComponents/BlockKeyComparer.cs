﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DI3
{
    internal class BlockKeyComparer<C> : IComparer<BlockKey<C>>
        where C : IComparable<C>, IFormattable
    {
        public int Compare(BlockKey<C> x, BlockKey<C> y)
        {
            if (x.leftEnd.CompareTo(y.rightEnd) == 1)   //  x.LeftEnd > y.currentBlockLeftEnd
                return 1;

            if (x.leftEnd.CompareTo(y.leftEnd) == -1 && //  x.LeftEnd < y.LeftEnd
                x.rightEnd.CompareTo(y.leftEnd) == -1)  // x.rightEnd < y.LeftEnd
                return -1;

            return 0;
        }
    }
}
