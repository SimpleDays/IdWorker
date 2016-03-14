using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdWorkerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<long> set = new HashSet<long>();
            IdWorker idWorker1 = new IdWorker(0);
            IdWorker idWorker2 = new IdWorker(1);
            IdWorker idWorker3 = new IdWorker(2);
            Thread t1 = new Thread(() => DoTestIdWoker(idWorker1, set));
            Thread t2 = new Thread(() => DoTestIdWoker(idWorker2, set));
            Thread t3 = new Thread(() => DoTestIdWoker(idWorker3, set));

            t1.Start();
            t2.Start();
            t3.Start();

            try
            {
                Thread.Sleep(30000);
                t1.Abort();
                t2.Abort();
                t3.Abort();
            }
            catch (Exception e)
            {
            }


            Console.ReadKey();
        }

        private static void DoTestIdWoker(IdWorker idWorker,HashSet<long> set)
        {
            int i= 0;

            while (true)
            {
                long id = idWorker.NextId();

                if (!set.Add(id))
                {
                    Console.WriteLine("duplicate[" + i + "]:" + id);
                }

                if (i == 200)
                {
                    Console.WriteLine("done!");
                    break;
                }
                
                i++;

                Thread.Sleep(1);
            }
        }
    }
}
