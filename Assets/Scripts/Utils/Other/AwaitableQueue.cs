using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;


public class AwaitableQueue {
    private readonly object _queueLock = new object();
    private Queue<Action> queue = new Queue<Action>();


	/** Add action to Queue */
	public void Enqueue(Action el) {
		lock (_queueLock) {
			queue.Enqueue(el);
		}
	}

    /** Executes the actions queued by other threads on the main thread. */
	public void ExecuteActionInQueue() {
		if (queue != null) {
			lock (_queueLock) {
				Queue<Action> queueActions = new Queue<Action>(queue);
				int queueLength = queueActions.Count;

				for (int i = 0; i < queueLength; i++) {
					Action act = queueActions.Dequeue();
					act.Invoke();
				}

                queue = queueActions;
			}
		}
	}


	public object getQueueLock() {
		return this._queueLock;
	}
}
