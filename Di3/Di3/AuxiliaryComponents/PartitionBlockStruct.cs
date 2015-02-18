﻿using System;

namespace Polimi.DEIB.VahidJalili.DI3
{
    internal struct PartitionBlock<C>
        where C : IComparable<C>, IFormattable
    {
        public BlockKey<C> left { set; get; }
        public BlockKey<C> right { set; get; }
    }
}
