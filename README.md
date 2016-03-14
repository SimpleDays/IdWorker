## IdWorker
### C# 版本的 Twitter-Snowflake 64位自增ID
#### 该版本是从Java版本翻译过来。
```C#
  IdWorker idWorker1 = new IdWorker(1);
  long id = idWorker.NextId();
