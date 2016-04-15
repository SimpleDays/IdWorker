using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdWorkerServer
{
    /**
    * tweeter的snowflake 移植到Java翻译成Net:
    *   (a) id构成: 42位的时间前缀 + 10位的节点标识 + 12位的sequence避免并发的数字(12位不够用时强制得到新的时间前缀)
    *       注意这里进行了小改动: snowkflake是5位的datacenter加5位的机器id; 这里变成使用10位的机器id
    *   (b) 对系统时间的依赖性非常强，需关闭ntp的时间同步功能。当检测到ntp时间调整后，将会拒绝分配id
    */

    public class IdWorker
    {
        private long workerId;
        private long epoch = 1403854494756L;   // 时间起始标记点，作为基准，一般取系统的最近时间()
        private static long workerIdBits = 10L;      // 机器标识位数
        private long maxWorkerId = -1L ^ -1L << (int)workerIdBits;// 机器ID最大值: 1023
        private long sequence = 0L;                   // 0，并发控制
        private static long sequenceBits = 12L;      //毫秒内自增位

        private long workerIdShift = sequenceBits;
        private long timestampLeftShift = sequenceBits + workerIdBits;
        private long sequenceMask = -1L ^ -1L << (int)sequenceBits;
        private long lastTimestamp = -1L;
        private static object syncRoot = new object();

        public IdWorker(long workerId)
        {
            if (workerId > maxWorkerId || workerId < 0)
            {
                throw new ArgumentOutOfRangeException(String.Format("worker Id can't be greater than {0} or less than 0", maxWorkerId));
            }

            this.workerId = workerId;
        }

        public long NextId()
        {
            lock (syncRoot)
            {
                long timestamp = this.TimeGen();

                if (this.lastTimestamp == timestamp)
                { 
                    // 如果上一个timestamp与新产生的相等，则sequence加一(0-4095循环); 对新的timestamp，sequence从0开始
                    this.sequence = this.sequence + 1 & this.sequenceMask;
                    if (this.sequence == 0)
                    {
                        timestamp = this.TilNextMillis(this.lastTimestamp);// 重新生成timestamp
                    }
                }
                else {
                    this.sequence = 0;
                }

                if (timestamp < this.lastTimestamp)
                {
                    //Log.E("IdWorker", "127.0.0.1", null,String.Format("clock moved backwards.Refusing to generate id for {0} milliseconds", (this.lastTimestamp - timestamp)));
                    throw new Exception(String.Format("clock moved backwards.Refusing to generate id for {0} milliseconds", (this.lastTimestamp - timestamp)));
                }

                this.lastTimestamp = timestamp;
                return timestamp - this.epoch << (int)this.timestampLeftShift | this.workerId << (int)this.workerIdShift | this.sequence;
            }
        }

        /**
         * 等待下一个毫秒的到来, 保证返回的毫秒数在参数lastTimestamp之后
         */
        private long TilNextMillis(long lastTimestamp)
        {
            long timestamp = this.TimeGen();

            while (timestamp <= lastTimestamp)
            {
                timestamp = this.TimeGen();
            }

            return timestamp;
        }

        /**
         * 获得系统当前毫秒数
         */
        private long TimeGen()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
