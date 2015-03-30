﻿using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using CSharpTest.Net.Synchronization;
using CSharpTest.Net.Threading;
using Polimi.DEIB.VahidJalili.DI3.BasicOperations.FirstOrderFunctions;
using Polimi.DEIB.VahidJalili.DI3.BasicOperations.IndexFunctions;
using Polimi.DEIB.VahidJalili.IGenomics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Polimi.DEIB.VahidJalili.DI3
{
    /// <summary>
    /// Dynamic _intervals inverted index (Polimi.DEIB.VahidJalili.DI3) 
    /// is an indexing system aimed at providing
    /// efficient means of processing the _intervals
    /// it indexes for common information retrieval 
    /// tasks.
    /// </summary>
    /// <typeparam name="C">Represents the c/domain
    /// type (e.g,. int, double, Time).</typeparam>
    /// <typeparam name="I">Represents generic type of the _interval.
    /// (e.g., time span, _interval on natural numbers)
    /// <para>For _intervals of possibly different types,
    /// it is recommended to define this generic type
    /// parameter in terms of Lowest Common Denominator.
    /// </para></typeparam>
    /// <typeparam name="M">Represents generic
    /// type of pointer to descriptive atI cooresponding
    /// to the _interval.</typeparam>
    public class Di3<C, I, M> : IDisposable
        where C : IComparable<C>, IFormattable
        where I : IInterval<C, M>
        where M : IMetaData, new()
    {
        /// <summary>
        /// Dynamic _intervals inverted index (Polimi.DEIB.VahidJalili.DI3) 
        /// is an indexing system aimed at providing
        /// efficient means of processing the _intervals
        /// it indexes for common information retrieval 
        /// tasks.
        /// </summary>
        /// <param name="CSerializer"></param>
        /// <param name="comparer"></param>
        private Di3(
            string FileName,
            CreatePolicy createPolicy,
            ISerializer<C> CSerializer,
            IComparer<C> comparer,
            int avgKeySize,
            int avgValueSize,
            bool THIS_IS_THE_MOST_USED_ONE)
        {
            //bSerializer = new BSerializer();
            //var options = new BPlusTree<C, B>.OptionsV2(CSerializer, bSerializer, comparer);


            ////counted.CalcBTreeOrder(avgKeySize, avgValueSize); //24);
            //options.CreateFile = createPolicy;
            ////counted.ExistingLogAction = ExistingLogAction.ReplayAndCommit;
            //options.StoragePerformance = StoragePerformance.Fastest;


            //////// Multi-Threading 2nd pass test; was commented-out visa versa
            //counted.CachePolicy = CachePolicy.All;
            //options.CachePolicy = CachePolicy.Recent;


            //counted.FileBlockSize = 512;

            /*counted.MaximumChildNodes = 8;
            counted.MinimumChildNodes = 2;

            counted.MaximumValueNodes = 8;
            counted.MinimumValueNodes = 2;
            */


            /// There three lines added for multi-threading.
            //counted.CallLevelLock = new ReaderWriterLocking();
            /////counted.LockingFactory = new LockFactory<SimpleReadWriteLocking>(); //Test 1
            /////counted.LockingFactory = new LockFactory<WriterOnlyLocking>(); //Test 2
            //counted.LockingFactory = new LockFactory<ReaderWriterLocking>();
            //counted.LockTimeout = 10000;




            //options.MaximumChildNodes = 256;// 100;
            //options.MinimumChildNodes = 2;//2;//10;

            //options.MaximumValueNodes = 256; // 100;
            //options.MinimumValueNodes = 2;//2;//10;

            //if (createPolicy != CreatePolicy.Never)
                //options.FileName = FileName;

            //_di3_1R = new BPlusTree<C, B>(options);
            //SingleIndex = new SingleIndex<C, I, M>(_di3_1R);

            //_di3_1R.DebugSetValidateOnCheckpoint(false);
        }


        public Di3(Di3Options<C> options)
        {
            _di3_1R = new BPlusTree<C, B>(Get1ROptions(options));
            _di3_2R = new BPlusTree<BlockKey<C>, BlockValue>(Get2ROptions(options));
            _di3_info = new BPlusTree<string, int>(GetinfoOptions(options));
            _indexesCardinality = new InfoIndex(_di3_info);
            INDEX = new SingleIndex<C, I, M>(_di3_1R);            

            /// Don't enable following commands.
            /// The consequences are: initialization becomes very slow,
            /// specially if the data size is big.
            //_di3_1R.EnableCount();
            //_di3_2R.EnableCount();
        }

        /// <summary>
        /// Di3 First resolution
        /// </summary>
        private BPlusTree<C, B> _di3_1R { set; get; }
        private BPlusTree<BlockKey<C>, BlockValue> _di3_2R { set; get; }
        private BPlusTree<string, int> _di3_info { set; get; }
        private BookmarkSerializer _bookmarkSerializer { set; get; }
        private LambdaItemSerializer _lambdaItemSerializer { set; get; }
        private LambdaArraySerializer _lambdaArraySerializer { set; get; }
        private BlockKeySerializer<C> _blockKeySerializer { set; get; }
        private BlockValueSerializer _blockValueSerializer { set; get; }
        private InfoIndex _indexesCardinality { set; get; }
        
        /// <summary>
        /// Cardinality Key for first resolution.
        /// </summary>
        private string cKey_1R { set { } get { return "1st"; } }

        /// <summary>
        /// Cardinality key for second resolution
        /// </summary>
        private string cKey_2R { set { } get { return "2nd"; } }
        

        /// <summary>
        /// Is an instance of SingleIndex class which 
        /// provides efficient means of inserting an 
        /// interval to Polimi.DEIB.VahidJalili.DI3; i.e., di3_1R indexding.
        /// </summary>
        private SingleIndex<C, I, M> INDEX { set; get; }

        /// <summary>
        /// Gets the number of blocks contained in Polimi.DEIB.VahidJalili.DI3.
        /// </summary>
        public int bookmarkCount { private set { } get { return _indexesCardinality.GetValue(cKey_1R); } }
        public int blockCount { private set { } get { return _indexesCardinality.GetValue(cKey_2R); } }

        private BPlusTree<C, B>.OptionsV2 Get1ROptions(Di3Options<C> options)
        {
            _lambdaItemSerializer = new LambdaItemSerializer();
            _lambdaArraySerializer = new LambdaArraySerializer(_lambdaItemSerializer);
            _bookmarkSerializer = new BookmarkSerializer(_lambdaArraySerializer);
            var rtv = new BPlusTree<C, B>.OptionsV2(options.CSerializer, _bookmarkSerializer, options.Comparer);
            rtv.ReadOnly = options.OpenReadOnly;

            if (options.MaximumChildNodes >= 4 &&
                options.MinimumChildNodes >= 2 &&
                options.MaximumValueNodes >= 4 &&
                options.MinimumValueNodes >= 2)
            {
                rtv.MaximumChildNodes = options.MaximumChildNodes;
                rtv.MinimumChildNodes = options.MinimumChildNodes;
                rtv.MaximumValueNodes = options.MaximumValueNodes;
                rtv.MinimumValueNodes = options.MinimumValueNodes;
            }

            if (options.AverageKeySize != 0 && options.AverageValueSize != 0)
                rtv.CalcBTreeOrder(options.AverageKeySize, options.AverageValueSize);

            if (options.FileBlockSize != 0)
                rtv.FileBlockSize = options.FileBlockSize;

            rtv.CachePolicy = options.CachePolicy;
            if (options.CreatePolicy != CreatePolicy.Never)
                rtv.FileName = options.FileName + ".idx1R";

            rtv.CreateFile = options.CreatePolicy;
            rtv.ExistingLogAction = options.ExistingLogAction;
            rtv.StoragePerformance = options.StoragePerformance;

            rtv.CallLevelLock = new ReaderWriterLocking();
            if (options.LockTimeout > 0) rtv.LockTimeout = options.LockTimeout;

            switch (options.Locking)
            {
                case LockMode.WriterOnlyLocking:
                    rtv.LockingFactory = new LockFactory<WriterOnlyLocking>();
                    break;

                case LockMode.ReaderWriterLocking:
                    rtv.LockingFactory = new LockFactory<ReaderWriterLocking>();
                    break;

                case LockMode.SimpleReadWriteLocking:
                    rtv.LockingFactory = new LockFactory<SimpleReadWriteLocking>();
                    break;

                case LockMode.IgnoreLocking:
                    rtv.LockingFactory = new IgnoreLockFactory();
                    break;
            }

            if (options.CacheMaximumHistory != 0 && options.CacheKeepAliveTimeOut != 0)
            {
                rtv.CacheKeepAliveMaximumHistory = options.CacheMaximumHistory;
                rtv.CacheKeepAliveMinimumHistory = options.CacheMinimumHistory;
                rtv.CacheKeepAliveTimeout = options.CacheKeepAliveTimeOut;
            }

            return rtv;
        }
        private BPlusTree<BlockKey<C>, BlockValue>.OptionsV2 Get2ROptions(Di3Options<C> options)
        {
            /// TODO
            /// Try to optimize these settings as much as possible.
            
            _blockKeySerializer = new BlockKeySerializer<C>(options.CSerializer);
            _blockValueSerializer = new BlockValueSerializer();

            var rtv = new BPlusTree<BlockKey<C>, BlockValue>.OptionsV2(_blockKeySerializer, _blockValueSerializer, new BlockKeyComparer<C>());
            rtv.ReadOnly = options.OpenReadOnly;

            rtv.MinimumChildNodes = 2;
            rtv.MaximumChildNodes = 256;
            rtv.MinimumValueNodes = 2;
            rtv.MaximumValueNodes = 256;

            rtv.FileBlockSize = 8192;

            rtv.StoragePerformance = StoragePerformance.Fastest;

            rtv.CachePolicy = options.CachePolicy;
            if (options.CreatePolicy != CreatePolicy.Never)
                rtv.FileName = options.FileName + ".idx2R";

            rtv.CreateFile = options.CreatePolicy;
            rtv.ExistingLogAction = options.ExistingLogAction;
            rtv.StoragePerformance = options.StoragePerformance;

            rtv.CallLevelLock = new ReaderWriterLocking();
            if (options.LockTimeout > 0) rtv.LockTimeout = options.LockTimeout;

            switch (options.Locking)
            {
                case LockMode.WriterOnlyLocking:
                    rtv.LockingFactory = new LockFactory<WriterOnlyLocking>();
                    break;

                case LockMode.ReaderWriterLocking:
                    rtv.LockingFactory = new LockFactory<ReaderWriterLocking>();
                    break;

                case LockMode.SimpleReadWriteLocking:
                    rtv.LockingFactory = new LockFactory<SimpleReadWriteLocking>();
                    break;

                case LockMode.IgnoreLocking:
                    rtv.LockingFactory = new IgnoreLockFactory();
                    break;
            }

            if (options.CacheMaximumHistory != 0 && options.CacheKeepAliveTimeOut != 0)
            {
                rtv.CacheKeepAliveMaximumHistory = options.CacheMaximumHistory;
                rtv.CacheKeepAliveMinimumHistory = options.CacheMinimumHistory;
                rtv.CacheKeepAliveTimeout = options.CacheKeepAliveTimeOut;
            }

            return rtv;
        }
        private BPlusTree<string, int>.OptionsV2 GetinfoOptions(Di3Options<C> options)
        {
            var rtv = new BPlusTree<string, int>.OptionsV2(PrimitiveSerializer.String, PrimitiveSerializer.Int32, new Comparers.StringComparer());
            rtv.FileBlockSize = 1024;
            rtv.CachePolicy = CachePolicy.All;
            rtv.StoragePerformance = StoragePerformance.Fastest;
            rtv.FileName = options.FileName + ".info";
            rtv.CreateFile = CreatePolicy.IfNeeded;
            return rtv;
        }



        public void Add(I interval)
        {   
            INDEX.Index(interval);
        }
        public void Add(List<I> intervals, IndexingMode mode)
        {
            Add(intervals, mode, Environment.ProcessorCount);
        }
        public void Add(List<I> intervals, IndexingMode mode, int threads)
        {
            int start = 0, stop = 0, range = (int)Math.Ceiling(intervals.Count / (double)threads);
            var addedBookmarks = new ConcurrentDictionary<int, int>();
            using (WorkQueue work = new WorkQueue(threads))
            {
                for (int i = 0; i < threads; i++)
                {
                    start = i * range;
                    stop = (i + 1) * range;
                    if (stop > intervals.Count) stop = intervals.Count;
                    work.Enqueue(new SingleIndex<C, I, M>(_di3_1R, intervals, start, stop, mode, addedBookmarks).Index);
                }

                work.Complete(true, -1);
            }

            int counted = 0;
            foreach (var item in addedBookmarks)
                counted += item.Value;
            _indexesCardinality.AddOrUpdate(cKey_1R, counted);
        }
        public void SecondPass()
        {
            INDEX.SecondPass();
        }


        public void SecondResolutionIndex()
        {
            SecondResolutionIndex(Environment.ProcessorCount);
        }
        public void SecondResolutionIndex(int nThreads)
        {
            // change first resolution options here to be readonly and readonly lock.

            Partition<C>[] partitions = Fragment_1R(nThreads);
            var addedBookmarks = new ConcurrentDictionary<C, int>();
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(
                        new SingleIndex2R<C, I, M>(
                            _di3_1R,
                            _di3_2R,
                            partitions[i].left,
                            partitions[i].right,
                            addedBookmarks).Index);

                work.Complete(true, -1);
            }

            int counted = 0;
            foreach (var item in addedBookmarks)
                counted += item.Value;
            _indexesCardinality.AddOrUpdate(cKey_2R, counted);
        }


        public void Cover<O>(ICSOutput<C, I, M, O> outputStrategy, int minAccumulation, int maxAccumulation)
        {
            Cover<O>(outputStrategy, minAccumulation, maxAccumulation, Environment.ProcessorCount);
        }
        public void Cover<O>(ICSOutput<C, I, M, O> outputStrategy, int minAccumulation, int maxAccumulation, int nThreads)
        {
            Object lockOnMe = new Object();
            PartitionBlock<C>[] partitions = Fragment_2R(nThreads);
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(
                        new CoverSummit<C, I, M, O>(
                            lockOnMe,
                            _di3_1R,
                            _di3_2R,
                            outputStrategy,
                            partitions[i].left,
                            partitions[i].right,
                            minAccumulation,
                            maxAccumulation).Cover);

                work.Complete(true, -1);
            }
        }
        public void Summit<O>(ICSOutput<C, I, M, O> outputStrategy, int minAccumulation, int maxAccumulation)
        {
            Summit<O>(outputStrategy, minAccumulation, maxAccumulation, Environment.ProcessorCount);
        }
        public void Summit<O>(ICSOutput<C, I, M, O> outputStrategy, int minAccumulation, int maxAccumulation, int nThreads)
        {
            Object lockOnMe = new Object();
            PartitionBlock<C>[] partitions = Fragment_2R(nThreads);
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(
                        new CoverSummit<C, I, M, O>(
                            lockOnMe,
                            _di3_1R,
                            _di3_2R,
                            outputStrategy,
                            partitions[i].left,
                            partitions[i].right,
                            minAccumulation,
                            maxAccumulation).Summit);

                work.Complete(true, -1);
            }
        }

        public void Map<O>(ref ICSOutput<C, I, M, O> outputStrategy, List<I> references)
        {
            Map<O>(ref outputStrategy, references, Environment.ProcessorCount, default(C), default(C));
        }
        public void Map<O>(ref ICSOutput<C, I, M, O> outputStrategy, List<I> references, C UDF, C DDF)
        {
            Map<O>(ref outputStrategy, references, Environment.ProcessorCount, UDF, DDF);
        }
        public void Map<O>(ref ICSOutput<C, I, M, O> outputStrategy, List<I> references, int nThreads)
        {
            Map<O>(ref outputStrategy, references, nThreads, default(C), default(C));
        }
        public void Map<O>(ref ICSOutput<C, I, M, O> outputStrategy, List<I> references, int nThreads, C UDF, C DDF)
        {
            Object lockOnMe = new Object();
            int start = 0, stop = 0, range = (int)Math.Ceiling(references.Count / (double)nThreads);
            
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                {
                    start = i * range;
                    stop = (i + 1) * range;
                    if (stop > references.Count) stop = references.Count;
                    if (start < stop) work.Enqueue(new Map<C, I, M, O>(lockOnMe, _di3_1R, outputStrategy, references, start, stop, UDF, DDF).Run);
                    else break;
                }

                work.Complete(true, -1);
            }
        }

        public List<AccEntry<C>> AccumulationHistogram()
        {
            return AccumulationHistogram(Environment.ProcessorCount);
        }
        public List<AccEntry<C>> AccumulationHistogram(int nThreads)
        {
            Object lockOnMe = new Object();
            var results = new List<AccEntry<C>>();

            var partitions = Fragment_1R(nThreads);
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(new AccumulationStats<C, I, M>(_di3_1R, partitions[i].left, partitions[i].right, results, lockOnMe).AccHistogram);
                work.Complete(true, -1);
            }

            /// Partitions define dichotomies, hence iterating over ranges it would not be 
            /// possible to see inter-dichotomies gaps. Therefore, there will be #nThreads 
            /// gaps that are not reported in output. The following loop addresses the point.
            for (int i = 0; i < nThreads - 1; i++)
                results.Add(new AccEntry<C>(partitions[i].right, partitions[i + 1].left, 0));

            return results;
        }

        public SortedDictionary<int, int> AccumulationDistribution()
        {
            return AccumulationDistribution(Environment.ProcessorCount);
        }
        public SortedDictionary<int, int> AccumulationDistribution(int nThreads)
        {
            Object lockOnMe = new Object();
            var results = new SortedDictionary<int, int>();
            var partitions = Fragment_1R(nThreads);
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(new AccumulationStats<C, I, M>(_di3_1R, partitions[i].left, partitions[i].right, results, lockOnMe).AccDistribution);
                work.Complete(true, -1);
            }

            /// These three lines are to make sure no currentAcc is 
            /// skipped. For instance, lets say results have 1, 2, and 4 
            /// as keys. Then 3 is skipped, and these lines will add 3 with 
            /// a value of 0 to the results.
            int maxValue = results.ElementAt(results.Count - 1).Key;
            for (int i = 0; i < maxValue; i++)
                if (!results.ContainsKey(i)) results.Add(i, 0);
            return results;
        }

        public ICollection<BlockKey<C>> Merge()
        {
            return Merge(Environment.ProcessorCount);
        }
        public ICollection<BlockKey<C>> Merge(int nThreads)
        {
            /*Object lockOnMe = new Object();
            PartitionBlock<C>[] partitions = Fragment_2R(nThreads);
            var blocks = new SortedDictionary<BlockKey<C>, int>();
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(
                        new MergeComplement<C, I, M>(
                            _di3_2R,
                            partitions[i].left,
                            partitions[i].right,
                            blocks,
                            lockOnMe).Merge);

                work.Complete(true, -1);
            }
            return blocks;*/

            return _di3_2R.Keys;
        }

        public ICollection<BlockKey<C>> Complement()
        {
            return Complement(Environment.ProcessorCount);
        }
        public ICollection<BlockKey<C>> Complement(int nThreads)
        {
            Object lockOnMe = new Object();
            PartitionBlock<C>[] partitions = Fragment_2R(nThreads);
            var blocks = new SortedDictionary<BlockKey<C>, int>();
            using (WorkQueue work = new WorkQueue(nThreads))
            {
                for (int i = 0; i < nThreads; i++)
                    work.Enqueue(
                        new MergeComplement<C, I, M>(
                            _di3_2R,
                            partitions[i].left,
                            partitions[i].right,
                            blocks,
                            lockOnMe).Complement);

                work.Complete(true, -1);
            }

            for (int i = 0; i < nThreads - 1; i++)
            {
                var partitionsBlock = new BlockKey<C>(partitions[i].right.rightEnd, partitions[i + 1].left.leftEnd);
                if (!blocks.ContainsKey(partitionsBlock))
                    blocks.Add(partitionsBlock, 0);
            }

            return blocks.Keys;
        }

        public ICollection<BlockKey<C>> Dichotomies()
        {
            ICollection<C> keys = _di3_1R.Keys;
            var results = new SortedDictionary<BlockKey<C>, bool>();
            int keysCount = bookmarkCount;

            for (int key = 0; key < keysCount - 1; key++)
                results.Add(new BlockKey<C>(LeftEnd: keys.ElementAt(key), RightEnd: keys.ElementAt(key + 1)), true);

            var keys2R = _di3_2R.Keys;
            for (int key = 0; key < blockCount - 1; key++)
                results.Remove(new BlockKey<C>(LeftEnd: keys2R.ElementAt(key).rightEnd, RightEnd: keys2R.ElementAt(key + 1).leftEnd));

            return results.Keys;
        }

        private Partition<C>[] Fragment_1R(int fCount)
        {
            int range = Convert.ToInt32(Math.Floor((double)_indexesCardinality.GetValue(cKey_1R) / (double)fCount));

            /// Initialization
            Partition<C>[] partitions = new Partition<C>[fCount];
            for (int i = 0; i < fCount; i++)
            {
                partitions[i].left = _di3_1R.ElementAtOrDefault((i * range) + 1).Key;
                partitions[i].right = _di3_1R.ElementAtOrDefault((i + 1) * range).Key;
            }
            partitions[0].left = _di3_1R.First().Key;
            partitions[fCount - 1].right = _di3_1R.Last().Key;

            /// Refinement
            bool incrementRight = true;
            fCount--;
            for (int i = 0; i < fCount; i++)
            {
                foreach (var bookmark in _di3_1R.EnumerateFrom(partitions[i].right))
                {
                    if (incrementRight)
                    {
                        partitions[i].right = bookmark.Key;
                        if (bookmark.Value.lambda.Length - bookmark.Value.omega + bookmark.Value.mu == 0)
                            incrementRight = false;
                        continue;
                    }
                    else
                    {
                        partitions[i + 1].left = bookmark.Key;
                        break;
                    }                    
                }

                if (partitions[i + 1].left.CompareTo(partitions[i + 1].right) == 1)
                    partitions[i + 1].right = partitions[i + 1].left;
                incrementRight = true;
            }

            return partitions;
        }
        private PartitionBlock<C>[] Fragment_2R(int fCount)
        {
            // does this gets correct value ?
            int range = Convert.ToInt32(Math.Floor((double)_indexesCardinality.GetValue(cKey_2R) / (double)fCount));

            /// Initialization
            PartitionBlock<C>[] partitions = new PartitionBlock<C>[fCount];
            for (int i = 0; i < fCount; i++)
            {
                partitions[i].left = _di3_2R.ElementAtOrDefault((i * range) ).Key;
                partitions[i].right = _di3_2R.ElementAtOrDefault((i ) * range).Key;
            }
            partitions[0].left = _di3_2R.First().Key;
            partitions[fCount - 1].right = _di3_2R.Last().Key;

            return partitions;
        }

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {// Protected implementation of Dispose pattern. 
            if (disposed)
                return;

            if (disposing)
            {
                // Free managed objects here. 
                _di3_1R.Commit();
                _di3_1R.Dispose();
                _di3_2R.Commit();
                _di3_2R.Dispose();
                _di3_info.Commit();
                _di3_info.Dispose();
            }

            // Free unmanaged objects here. 
            disposed = true;
        }

        public void Commit()
        {
            _di3_1R.Commit();
            _di3_2R.Commit();
            _di3_info.Commit();
        }
    }
}