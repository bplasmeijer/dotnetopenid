﻿//-----------------------------------------------------------------------
// <copyright file="CoordinatorBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Threading;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal abstract class CoordinatorBase<T1, T2> {
		private Action<T1> party1Action;
		private Action<T2> party2Action;

		protected CoordinatorBase(Action<T1> party1Action, Action<T2> party2Action) {
			ErrorUtilities.VerifyArgumentNotNull(party1Action, "party1Action");
			ErrorUtilities.VerifyArgumentNotNull(party2Action, "party2Action");

			this.party1Action = party1Action;
			this.party2Action = party2Action;
		}

		internal abstract void Run();

		protected void RunCore(T1 party1Object, T2 party2Object) {
			Thread party1Thread = null, party2Thread = null;
			Exception failingException = null;

			// Each thread we create needs a surrounding exception catcher so that we can
			// terminate the other thread and inform the test host that the test failed.
			Action<Action> safeWrapper = (action) => {
				try {
					action();
				} catch (Exception ex) {
					// We may be the second thread in an ThreadAbortException, so check the "flag"
					if (failingException == null) {
						failingException = ex;
						if (Thread.CurrentThread == party1Thread) {
							party2Thread.Abort();
						} else {
							party1Thread.Abort();
						}
					}
				}
			};

			// Run the threads, and wait for them to complete.
			// If this main thread is aborted (test run aborted), go ahead and abort the other two threads.
			party1Thread = new Thread(() => { safeWrapper(() => { this.party1Action(party1Object); }); });
			party2Thread = new Thread(() => { safeWrapper(() => { this.party2Action(party2Object); }); });
			try {
				party1Thread.Start();
				party2Thread.Start();
				party1Thread.Join();
				party2Thread.Join();
			} catch (ThreadAbortException) {
				party1Thread.Abort();
				party2Thread.Abort();
				throw;
			}

			// Use the failing reason of a failing sub-thread as our reason, if anything failed.
			if (failingException != null) {
				throw new AssertFailedException("Coordinator thread threw unhandled exception: " + failingException, failingException);
			}
		}
	}
}