
/*
Copyright 2013 Sannel Software, L.L.C.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/
using System;
using System.Linq;
using log4net;
using Sannel.Helpers;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using Sannel.HandBrakeRunner.Interfaces;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sannel.HandBrakeRunner
{
	class Program
	{
		static ILog log = LogManager.GetLogger(typeof(Program));

		private sealed class SingleThreadSynchronizationContext :  
			SynchronizationContext
		{
			private readonly
			 BlockingCollection<KeyValuePair<SendOrPostCallback,object>>
			  m_queue =
			   new BlockingCollection<KeyValuePair<SendOrPostCallback,object>>();
 
			public override void Post(SendOrPostCallback d, object state)
			{
				m_queue.Add(
					new KeyValuePair<SendOrPostCallback,object>(d, state));
			}
 
			public void RunOnCurrentThread()
			{
				KeyValuePair<SendOrPostCallback, object> workItem;
				while(m_queue.TryTake(out workItem, Timeout.Infinite))
					workItem.Key(workItem.Value);
			}
 
			public void Complete() { m_queue.CompleteAdding(); }

		}

		static int Main(String[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			Arguments arguments = new Arguments();
			arguments.Parse(args);

			Runner runner = new Runner();


			var prevCtx = SynchronizationContext.Current;
			try
			{
				var syncCtx = new SingleThreadSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(syncCtx);

				var t = runner.RunAsync(arguments);
				t.ContinueWith(
					delegate { syncCtx.Complete(); }, TaskScheduler.Default);

				syncCtx.RunOnCurrentThread();

				var results = t.GetAwaiter().GetResult();
				return results;
			}
			finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
		}
	}
}
