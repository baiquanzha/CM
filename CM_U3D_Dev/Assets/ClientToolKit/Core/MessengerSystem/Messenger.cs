
using System;
using System.Collections.Generic;

namespace MTool.Core.MessengerSystem
{

    public enum MessengerMode
	{
		RequireListener,
		DontRequireListener,
	}

	static public class Messenger
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------


		static private Dictionary<int, Delegate> sEventTable = new Dictionary<int, Delegate>(10);


		static private List<int> sPermanentMessages = new List<int>(5);

		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		static public void MarkAsPermanent(int eventType)
		{
			sPermanentMessages.Add(eventType);
		}


		static public void Cleanup()
		{
			var messagesToRemove = new List<int>();

			foreach (var pair in sEventTable)
			{
				bool wasFound = false;
				foreach (var messageType in sPermanentMessages)
				{
					if (pair.Key == messageType)
					{
						wasFound = true;
						break;
					}
				}

				if (wasFound)
				{
					messagesToRemove.Add(pair.Key);
				}
			}

			foreach (var message in messagesToRemove)
			{
				sEventTable.Remove(message);
			}
			
		}

		static public BroadcastException CreateBroadcastSignatureException(int eventType)
		{
			return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
		}


		static private void OnListenerAdding(int eventType , Delegate listenerBeingAdded)
		{
			if (!sEventTable.ContainsKey(eventType))
			{
				sEventTable.Add(eventType,null);
			}

			Delegate d = sEventTable[eventType];

			if (d != null && d.GetType() != listenerBeingAdded.GetType())
			{
				throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}." +
				                                          "Current listeners have type {1} and listener being added has type {2} .",eventType,d.GetType().Name,listenerBeingAdded.GetType().Name));
			}
		}

		static private void OnListenerRemoving(int eventType,Delegate listenereBeingRemoved)
		{
			Delegate d;
			
			if (sEventTable.TryGetValue(eventType,out d))
			{
				if (d == null)
				{
					throw new ListenerException(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null",eventType));
				}
				else if(d.GetType() != listenereBeingRemoved.GetType())
				{
					throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}." +
					                                          " Current listeners have type {1} and listener being removed has type {2}",eventType,d.GetType().Namespace,listenereBeingRemoved.GetType().Name));
				}
			}
			else
			{
				throw new ListenerException(string.Format("Attempting to remove listener for type \"{0}\" but Messenger does't know about this event type.",eventType));
			}
		}

		static private void OnListenerRemoved(int eventType)
		{
			if (sEventTable[eventType] == null)
			{
				sEventTable.Remove(eventType);
			}
		}


		static private void OnBroadcasting(int eventType , MessengerMode mode)
		{
			if (mode == MessengerMode.RequireListener && !sEventTable.ContainsKey(eventType))
			{
				throw new BroadcastException(string.Format("Broadcasting meseage \"{0}\" but not listener found . Try marking the message with Messenger.MaskAsPermanent",eventType));
			}

		}

		#region Add Listeners

		static public void AddListener(int eventType , Callback handler)
		{
			OnListenerAdding(eventType,handler);

			sEventTable[eventType] = (Callback)sEventTable[eventType] + handler;
		}

		static public void AddListener<T>(int eventType, Callback<T> handler)
		{
			OnListenerAdding(eventType, handler);

			sEventTable[eventType] = (Callback<T>)sEventTable[eventType] + handler;
		}

		static public void AddListener<T, U>(int eventType, Callback<T, U> handler)
		{
			OnListenerAdding(eventType, handler);

			sEventTable[eventType] = (Callback<T, U>)sEventTable[eventType] + handler;
		}

		public static void AddListener<T,U,V>(int eventType, Callback<T, U, V> handler)
		{
			OnListenerAdding(eventType, handler);

			sEventTable[eventType] = (Callback<T, U, V>)sEventTable[eventType] + handler;
		}

		#endregion


		#region Remove Listeners

		public static void RemoveListener(int eventType , Callback handler)
		{
			OnListenerRemoving(eventType,handler);
			sEventTable[eventType] = (Callback) sEventTable[eventType] - handler;
			OnListenerRemoved(eventType);
		}

		public static void RemoveListener<T>(int eventType, Callback<T> handler)
		{
			OnListenerRemoving(eventType, handler);
			sEventTable[eventType] = (Callback<T>)sEventTable[eventType] - handler;
			OnListenerRemoved(eventType);
		}
		public static void RemoveListener<T,U>(int eventType, Callback<T,U> handler)
		{
			OnListenerRemoving(eventType, handler);
			sEventTable[eventType] = (Callback<T, U>)sEventTable[eventType] - handler;
			OnListenerRemoved(eventType);
		}
		public static void RemoveListener<T,U,V>(int eventType, Callback<T, U, V> handler)
		{
			OnListenerRemoving(eventType, handler);
			sEventTable[eventType] = (Callback<T, U, V>)sEventTable[eventType] - handler;
			OnListenerRemoved(eventType);
		}

		#endregion

		#region Broadcast message

		static public void Broadcast(int eventType, MessengerMode mode = MessengerMode.DontRequireListener)
		{

			OnBroadcasting(eventType , mode);

			Delegate d;

			if (sEventTable.TryGetValue(eventType,out d))
			{
				var callback = d as Callback;
				if (callback != null)
				{
					callback();
				}
				else
				{
					throw CreateBroadcastSignatureException(eventType);
				}
			}
		}

		static public void Broadcast<T>(int eventType, T arg1 , MessengerMode mode = MessengerMode.DontRequireListener)
		{

			OnBroadcasting(eventType, mode);

			Delegate d;

			if (sEventTable.TryGetValue(eventType, out d))
			{
				var callback = d as Callback<T>;
				if (callback != null)
				{
					callback(arg1);
				}
				else
				{
					throw CreateBroadcastSignatureException(eventType);
				}
			}
		}
        
        static public void Broadcast<T , U>(int eventType, T arg1 , U arg2 , MessengerMode mode = MessengerMode.DontRequireListener)
        {

            OnBroadcasting(eventType, mode);

            Delegate d;

            if (sEventTable.TryGetValue(eventType, out d))
            {
                var callback = d as Callback<T, U>;
                if (callback != null)
                {
                    callback(arg1,arg2);
                }
                else
                {
                    throw CreateBroadcastSignatureException(eventType);
                }
            }

        }

		static public void Broadcast<T , U , V>(int eventType, T arg1 , U arg2 , V arg3, MessengerMode mode = MessengerMode.DontRequireListener)
		{

			OnBroadcasting(eventType, mode);

			Delegate d;

			if (sEventTable.TryGetValue(eventType, out d))
			{
				var callback = d as Callback<T, U, V>;
				if (callback != null)
				{
					callback(arg1,arg2,arg3);
				}
				else
				{
					throw CreateBroadcastSignatureException(eventType);
				}
			}

		}
		
		#endregion
		
		#endregion

	}
}
