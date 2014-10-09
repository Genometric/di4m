﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Di3B
{
    public class FunctionOutput<O>
    {
        //public Dictionary<string, Chromosome> Chrs { set; get; }
        public Dictionary<string, Dictionary<char, List<O>>> Chrs { set; get; }

        public FunctionOutput()
        {
            //Chrs = new Dictionary<string, Chromosome>();
            Chrs = new Dictionary<string, Dictionary<char, List<O>>>();
        }
        /*
        public class Chromosome
        {
            public Chromosome()
            {
                //outputPS = new List<O>();
                //outputNS = new List<O>();
                outputUS = new List<O>();
            }
            public List<O> outputPS { set; get; }
            public List<O> outputNS { set; get; }
            public List<O> outputUS { set; get; }
        }*/

        public void addChromosome(string chr)
        {
            //Chrs.Add(chr, new Chromosome());
            Chrs.Add(chr, new Dictionary<char, List<O>>());
        }
    }
}
