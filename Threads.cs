using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace MultiThreading
{
	public class BC_Asynchronous
	{

		public static void main()
		{
			DateTime StartTime = DateTime.Now;
			Console.WriteLine("Step ①: Main start . . .  " + ThreadInfo());

			//string abc =  DoSomethingAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			Task<string> res1 = CallMethod();

            // Code 1
            //Thread.Sleep(25 * 1000);

			Console.WriteLine("Running result . . .  " + res1.Result);

            Console.WriteLine("Step ②: Main end . . .  " + ThreadInfo());
			DateTime EndTime = DateTime.Now;
			Console.WriteLine("Total Second: " + (EndTime - StartTime).TotalSeconds.ToString());
		}
        private static async Task<string> CallMethod()
        {
            Console.WriteLine("Step ⑤: CallMethod start . . .  " + ThreadInfo());
            string runningResult = await DoSomethingAsync();

			if (runningResult == "OK")
			{
                // Code 3
                Thread.Sleep(16 * 1000);
            }
			

			Console.WriteLine("Step ⑥: CallMethod end . . .   " + ThreadInfo());
            return runningResult;
        }
        private static async Task<string> CallMethod1()
        {
            Console.WriteLine("Step ⑦: CallMethod1 start . . .  " + ThreadInfo());
            string runningResult = await DoSomethingAsync();
            // Code 4
            Thread.Sleep(9 * 1000);

            Console.WriteLine("Step ⑧: CallMethod1 end . . .   " + ThreadInfo());
            return runningResult;
        }
        private static async Task<string> CallMethod2()
        {
            Console.WriteLine("Step ⑨: CallMethod2 start . . .  " + ThreadInfo());
            string runningResult = await CallMethod3();
            // Code 5
            Thread.Sleep(4 * 1000);

            Console.WriteLine("Step ⑩: CallMethod2 end . . .   " + ThreadInfo());
            return runningResult;
        }
        private static async Task<string> CallMethod3()
        {
            Console.WriteLine("Step 11: CallMethod3 start . . .  " + ThreadInfo());
            string runningResult = await DoSomethingAsync();
            // Code 5
            Thread.Sleep(10 * 1000);

            Console.WriteLine("Step 12: CallMethod3 end . . .   " + ThreadInfo());
            return runningResult;
        }

        private static async Task<string> DoSomethingAsync()
		{
			Console.WriteLine("Step ③: DoSomethingAsync start . . .  " + ThreadInfo());
			// Code 2
			await Task.Delay(20 * 1000);
			Console.WriteLine("Step ④: DoSomethingAsync end . . .   " + ThreadInfo());
            return "OK";
        }


		

		private static async Task CallMethod_Sub1()
		{
			Console.WriteLine("Step ③: CallMethod_Sub1 start . . .  " + ThreadInfo());
			Task<string> tasks=  DoSomethingAsync();
			Thread.Sleep(5 * 1000);
			await tasks;
			Console.WriteLine("Step ④: CallMethod_Sub1 end . . .   " + ThreadInfo());
		}



		

		private static string ThreadInfo()
		{
			return "ThreadID: " + Thread.CurrentThread.ManagedThreadId.ToString() + "; ThreadState: " + Thread.CurrentThread.ThreadState.ToString() + "; IsThreadPoolThread: " + Thread.CurrentThread.IsThreadPoolThread.ToString();
		}

		private static async Task CallMethod_Sub2()
		{
			Console.WriteLine("Step ③: CallMethod_Sub2 start . . .  " + ThreadInfo());
			await CallMethod_Sub3();
			Thread.Sleep(5 * 1000);
			Console.WriteLine("Step ④: CallMethod_Sub2 end . . .   " + ThreadInfo());
		}
		private static async Task CallMethod_Sub3()
		{
			Console.WriteLine("Step ⑤: CallMethod_Sub3 start . . .  " + ThreadInfo());
			await CallMethod_Sub4();
			Thread.Sleep(5 * 1000);
			Console.WriteLine("Step ⑥: CallMethod_Sub3 end . . .   " + ThreadInfo());
		}
		private static async Task CallMethod_Sub4()
		{
			Console.WriteLine("Step ③: CallMethod_Sub4 start . . .  " + ThreadInfo());
			await CallMethod_Sub5();
			Thread.Sleep(5 * 1000);
			Console.WriteLine("Step ④: CallMethod_Sub4 end . . .   " + ThreadInfo());
		}
		private static async Task CallMethod_Sub5()
		{
			Console.WriteLine("Step ③: CallMethod_Sub5 start . . .  " + ThreadInfo());
			await DoSomethingAsync();
			Thread.Sleep(5 * 1000);
			Console.WriteLine("Step ④: CallMethod_Sub5 end . . .   " + ThreadInfo());
		}

		static object locker1 = new object();
		static object locker2 = new object();


		public static void Mains()
		{

			var taskList = new Task[2];
			taskList[0] = Task.Run(() =>
			{
				Console.WriteLine("线程1 id:" + Thread.CurrentThread.ManagedThreadId);
				Console.WriteLine("线程1准备获取锁1");
				lock (locker1)
				{
					Console.WriteLine("线程1获取了锁1");
					Thread.Sleep(3000);

					Console.WriteLine("线程1准备获取锁2");
					lock (locker2)
					{
						Console.WriteLine("线程1获取了锁2");
					}
				}

			});
			taskList[1] = Task.Run(() =>
			{
				Console.WriteLine("线程2 id:" + Thread.CurrentThread.ManagedThreadId);
				Console.WriteLine("线程2准备获取锁2");
				lock (locker2)
				{
					Console.WriteLine("线程2获取了锁2");
					Thread.Sleep(3000);

					Console.WriteLine("线程2准备获取锁1");
					lock (locker1)
					{
						Console.WriteLine("线程2获取了锁1");
					}
				}

			});

			var finished = Task.WaitAll(taskList, 1000 * 60);
			if (finished)
			{
				Console.WriteLine("没有发生死锁");
			}
			else
			{
				Console.WriteLine("发生了死锁");
			}

			Console.ReadLine();
		}
	}
}
