using UnityEngine;
using System.Collections;
using System;

public class CoRoutine
{
	CoRoutineHandler.CoRoutineMonitor coRoutine;

	public CoRoutine( IEnumerator c, CoRoutineCompletionEvent e, bool autoStart )
	{
		complete += e;
		coRoutine = CoRoutineHandler.CreateCoRoutine( c );
		coRoutine.Finished += TaskFinished;
		if( autoStart ) { Start(); }
	}
	public CoRoutine( IEnumerator c, Action e, bool autoStart ) : this( c, ( unused ) => e(), autoStart ) { }
	public CoRoutine( IEnumerator c, bool autoStart ) : this(c, (CoRoutineCompletionEvent)null, autoStart) { }
	public CoRoutine( IEnumerator c, Action e ) : this( c, (unused) => e(), true ) { }
	public CoRoutine( IEnumerator c, CoRoutineCompletionEvent e ) : this(c, e, true) { }
	public CoRoutine( IEnumerator c ) : this(c, true) { }
	
	public static CoRoutine AfterWait(float delay, Action fn) { return new CoRoutine(afterWait(delay, fn)); }
	static IEnumerator afterWait(float delay, Action fn)
	{
		yield return new WaitForSeconds(delay);
		fn();
	}

	public bool Running { get{return coRoutine.Running;} }
	public bool Paused { get{return coRoutine.Paused;} }
	
	public void Start() { coRoutine.Start(); }
	public void Stop() { coRoutine.Stop(); }
	public void Pause() { coRoutine.Pause(); }
	public void Resume() { coRoutine.Resume(); }

	public delegate void CoRoutineCompletionEvent(bool manual);
	public event CoRoutineCompletionEvent complete;
	void TaskFinished(bool manual)
	{
		CoRoutineCompletionEvent handler = complete;
		if (handler != null) { handler(manual); }
	}
}

class CoRoutineHandler : MonoBehaviour
{
	static CoRoutineHandler handler;

	public static CoRoutineMonitor CreateCoRoutine(IEnumerator coroutine)
	{
		if (handler == null)
		{
			GameObject go = new GameObject("CoRoutines");
			handler = go.AddComponent<CoRoutineHandler>();
		}
		return new CoRoutineMonitor(coroutine);
	}

	public class CoRoutineMonitor
	{
		IEnumerator coroutine;
		bool running, paused, stopped;
		
		public CoRoutineMonitor(IEnumerator c) { coroutine = c; }
		
		public delegate void FinishedHandler(bool manual);
		public event FinishedHandler Finished;

		public bool Running { get {return running;} }
		public bool Paused { get {return paused;} }
		
		public void Start() { running = true; handler.StartCoroutine(CallWrapper()); }
		public void Stop() { stopped = true; running = false; }
		public void Pause() { paused = true; }
		public void Resume() { paused = false; }
		
		IEnumerator CallWrapper()
		{
			yield return null;
			IEnumerator iEnum = coroutine;
			while (running)
			{
				if (paused) { yield return null; }
				else
				{
					if (iEnum != null && iEnum.MoveNext()) { yield return iEnum.Current; }
					else { running = false; }
				}
			}
			
			FinishedHandler finishedHandler = Finished;
			if (finishedHandler != null) { finishedHandler(stopped); }
		}
	}
}