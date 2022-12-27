using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using MultiThreading;

namespace HelloWorldApplication
{
	class Program
	{
		public static void Main(string[] args)
		{
			BC_Asynchronous.main();
            Console.ReadKey();
		}

		public static void Main_BackUp(string[] args)
		{
			DateTime StartTime = DateTime.Now;
			Task<string> Result;

			//	Code 1
			Thread.Sleep(25 * 1000);
			Console.WriteLine("Step ①: Main Start . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤①

			//	Code 2 :async function start
			Result = subMethod();


			// Code 3
			Thread.Sleep(30 * 1000);
			Console.WriteLine("Step ②:  Main running . . . , ThreadID: " + GetThreadID() + "; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤②

			//	Code 4
			Console.WriteLine("Step ③: Running results: " + Result.Result + " ; ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤③

			//	Code 5
			Thread.Sleep(5 * 1000);
			Console.WriteLine("Step ⑥: Main End . . . , ThreadID: " + GetThreadID() + "; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤②

			DateTime EndTime = DateTime.Now;
			Console.WriteLine("Total Second: " + (EndTime - StartTime).TotalSeconds.ToString());

			Console.ReadKey();
		}


		public static string subMethod_sync()
		{
			Console.WriteLine("Step ⑦: subMethod Start . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤④

			//	Code 6																																										//	Code 5
			Thread.Sleep(5 * 1000);

            Task<string> Result = TimeDelay_async();
            //string Result = TimeDelay_sync();

            //	Code 7
            Thread.Sleep(15 * 1000);
			Console.WriteLine("Step ⑧: subMethod End . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤⑤
			return Result.Result;
		}
		async static Task<string> subMethod()
		{
            Console.WriteLine("Step ⑦: subMethod Start . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤④

            //	Code 6																																										//	Code 5
            Thread.Sleep(5 * 1000);

            string Result = await TimeDelay_async().ConfigureAwait(false);
            //string Result = TimeDelay_sync();

			//	Code 7
			Thread.Sleep(15 * 1000);
            Console.WriteLine("Step ⑧: subMethod End . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤⑤
            return Result;
		}

		async static Task<string> TimeDelay_async()
		{
			Console.WriteLine("Step ④: TimeDelay Start . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤④

			Task tasks = Task.Run(()=>{
				Thread.Sleep(15 * 1000);
			});
			await tasks;

			Console.WriteLine("Step ⑤: TimeDelay End . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤⑤
			return "OK";
		}

		public static void ThreadsTest()
		{
			//Thread thread2 = new Thread(() => {
			//	Console.WriteLine("Thread - 2 ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + "; IsThreadPoolThread: " + IsThreadPoolThread());
			//});

			//Thread thread3 = new Thread(() => {
			//	Console.WriteLine("Thread - 3 ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + "; IsThreadPoolThread: " + IsThreadPoolThread());
			//});


		}
		public static string TimeDelay_sync()
		{
			Console.WriteLine("Step ④: TimeDelay Start . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤④

			Task tasks = Task.Run(() => {
				Thread.Sleep(15 * 1000);
			});
			tasks.Wait();
			Task abc = Task.Delay(100000);

			Console.WriteLine("Step ⑤: TimeDelay End . . . , ThreadID: " + GetThreadID() + " ; ThreadState: " + ThreadState() + " ; IsThreadPoolThread: " + IsThreadPoolThread());//步骤⑤
			return "OK";
		}

		private static string GetThreadID()
		{
			return System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
		}
		private static string ThreadState()
		{
			return System.Threading.Thread.CurrentThread.ThreadState.ToString();
		}
		private static string IsThreadPoolThread()
		{
			return System.Threading.Thread.CurrentThread.IsThreadPoolThread.ToString();
		}



		private async Task<string> CallMethod()
		{
			Task<string> tasks =  DoSomethingAsync();
			// Code 1
			Thread.Sleep(5 * 1000);

			string result = await tasks;
			return result;
		}

		private async Task<string> DoSomethingAsync()
		{
			// Code 2
			await Task.Delay(7 * 1000);
			return "OK";
		}

	}

}