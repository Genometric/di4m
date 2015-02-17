﻿using System;
using Polimi.DEIB.VahidJalili.IGenomics;
using CSharpTest.Net.Collections;

namespace Polimi.DEIB.VahidJalili.DI3
{
    internal class RangeIndex<C, I, M>
        where C : IComparable<C>, IFormattable
        where I : IInterval<C, M>
        where M : IMetaData, new()
    {
        internal RangeIndex(BPlusTree<C,B> di3)
        {

        }

        /// <summary>
        /// Sets and gets the _di3_1R data structure
        /// to be manipulated. This data structure
        /// is in common between all classes of 
        /// namespace.
        /// </summary>
        private BPlusTree<C, B> _di3 { set; get; }
    }
}
