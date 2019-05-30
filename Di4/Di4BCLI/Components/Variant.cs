﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polimi.DEIB.VahidJalili.IGenomics;

namespace Polimi.DEIB.VahidJalili.DI4.CLI
{
    public class Variant : IInterval<int, VariantData>, IFormattable
    {
        public uint hashKey { set; get; }

        public int left { set; get; }

        public IVCF metadata { set; get; }

        public int right { set; get; }

        VariantData IInterval<int, VariantData>.metadata { set; get; }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
    }
}
