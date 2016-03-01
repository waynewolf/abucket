using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using NUnit.Framework;
using System.Threading;

namespace UnitTest {
	[TestFixture]
	internal class UnityEventTest {
		private int count;

		void MyAction() {
			count++;
			Debug.Log ("MyAction called: " + count);
		}

		void MyAction2() {
			Debug.Log ("MyAction2 called");
		}

		[Test]
		public void BaseTrigger() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (1, count);
		}

		[Test]
		public void AddSameActionTwiceThenTrigger() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction);
			thisEvent.AddListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (2, count);
		}

		// FIXME: This case is hard to understand and conflict with
		// above test case: AddSameActionTwiceThenTrigger, pay attention!
		[Test]
		public void AddSameActionTwiceThenRemoveOneThenTrigger() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction);
			thisEvent.AddListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (2, count);

			count = 0;
			thisEvent.RemoveListener (MyAction);
			thisEvent.Invoke ();
			// FIXME: count is 0, but should be one to be consistent
			// with the test case: AddSameActionTwiceThenTrigger
			Assert.AreEqual (0, count);
		}

		[Test]
		public void AddSameActionTwiceThenRemoveNonExistentOneThenTrigger() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction);
			thisEvent.AddListener (MyAction);
			thisEvent.RemoveListener (MyAction2);
			thisEvent.Invoke ();
			Assert.AreEqual (2, count);
		}

		[Test]
		public void AddSameActionTwiceThenRemoveNonExistentLamdaThenTrigger() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction);
			thisEvent.AddListener (MyAction);
			thisEvent.RemoveListener ( () => {
				Debug.Log ("Lamda");
			});
			thisEvent.Invoke ();
			Assert.AreEqual (2, count);
		}

		[Test]
		public void AddDifferentActionTwiceThenTrigger() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction2);
			thisEvent.AddListener (MyAction2);
			thisEvent.AddListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (1, count);
		}

		[Test]
		public void RemoveNotExistedAction() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.RemoveListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (0, count);
		}

		[Test]
		public void RemoveNotExistedAction2() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction2);
			thisEvent.RemoveListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (0, count);
		}

		internal class ActionClass : MonoBehaviour {
			public int count;

			public ActionClass() {
				count = 0;
			}

			public void ActionInside () {
				count++;
				Debug.Log ("ActionInside, count: " + count);
			}

			public void ActionInside2 () {
				Debug.Log ("ActionInside2");
			}

			public void MyAction() {
				count++;
				Debug.Log ("MyAction in ActionClass");
			}

			void OnDestroy() {
				Debug.Log ("ActionClass is destroyed");
			}
		}

		[Test]
		public void AddActionAsAnotherClassMethod() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			ActionClass actionClass = new ActionClass ();
			thisEvent.AddListener (actionClass.ActionInside);
			thisEvent.Invoke ();
			Assert.AreEqual (1, actionClass.count);
		}

		[Test]
		public void AddActionAsAnotherClassMethodTwice() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			ActionClass actionClass = new ActionClass ();
			thisEvent.AddListener (actionClass.ActionInside);
			thisEvent.AddListener (MyAction);
			thisEvent.AddListener (actionClass.ActionInside2);
			thisEvent.Invoke ();
			Assert.AreEqual (1, actionClass.count);
			Assert.AreEqual (1, count);
		}

		// FIXME: No exception thrown as expected, maybe something related
		// with the MonoBehaviour class and Object.DestroyImmediate method ? 
		[Test]
		//[ExpectedException]
		public void TriggerAfterObjectIsDestroyed() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			ActionClass actionClass = new ActionClass ();
			thisEvent.AddListener (actionClass.ActionInside);
			Object.DestroyImmediate (actionClass);
			Thread.Sleep (3000);
			thisEvent.Invoke ();
			Assert.AreEqual (1, actionClass.count);
		}

		[Test]
		public void AddActionTwiceWithOrWithoutUnityActionWrapper() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			thisEvent.AddListener (MyAction);
			UnityAction action = new UnityAction (MyAction);
			thisEvent.AddListener (action);
			thisEvent.Invoke ();
			Assert.AreEqual (2, count);
		}

		[Test]
		public void AddWrapperButRemoveMethodOrAddMethodButRemoveWrapper() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			UnityAction action = new UnityAction (MyAction);
			thisEvent.AddListener (action);
			thisEvent.RemoveListener (action);
			thisEvent.Invoke ();
			Assert.AreEqual (0, count);

			thisEvent.AddListener (action);
			thisEvent.RemoveListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (0, count);

			thisEvent.AddListener (MyAction);
			thisEvent.RemoveListener (action);
			thisEvent.Invoke ();
			Assert.AreEqual (0, count);
		}

		[Test]
		public void AddMethodButRemoveSameNameMethodInAnotherClass() {
			UnityEvent thisEvent = new UnityEvent ();
			count = 0;
			ActionClass actionClass = new ActionClass ();
			thisEvent.AddListener (actionClass.MyAction);
			thisEvent.RemoveListener (MyAction);
			thisEvent.Invoke ();
			Assert.AreEqual (1, actionClass.count);
		}
	}
}
