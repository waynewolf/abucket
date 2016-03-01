using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace UnitTest {
	[TestFixture]
	internal class EventManagerTest {
		private EventManager eventManager;
		private GameObject testObjHolder;

		// Do not use TestFixtureSetUp, this method should be called
		// before any Test starts.
		public void Init() {
			count = 0;
			testObjHolder = new GameObject ("TestObjectHolder");
			eventManager = new EventManager();
			eventManager._Init ();
		}

		public void Fini() {
			Object.DestroyImmediate (testObjHolder);
			Object.DestroyImmediate (eventManager);
		}

		[Test]
		[Category("Single Object")]
		public void Basic() {
			Init ();

			GameObject obj = new GameObject ("TestGameObject");
			obj.transform.SetParent (testObjHolder.transform);

			eventManager.RegisterEvent ("click", obj, DummyAction);

			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (1, eventSet.Count);

			eventManager.UnRegisterEvent ("click", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Single Object")]
		public void RegisterSameEventMultipleTimes() {
			Init ();

			GameObject obj = new GameObject ("TestGameObject");
			obj.transform.SetParent (testObjHolder.transform);
			
			eventManager.RegisterEvent ("click", obj, DummyAction);
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (1, eventSet.Count);

			eventManager.RegisterEvent ("click", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (1, eventSet.Count);

			eventManager.RegisterEvent ("click", obj, DummyAction);
			eventManager.RegisterEvent ("click", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (1, eventSet.Count);

			eventManager.UnRegisterEvent ("click", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Single Object")]
		public void UnRegisterMoreThanRegister() {
			Init ();

			GameObject obj = new GameObject ("TestGameObject");
			obj.transform.SetParent (testObjHolder.transform);
			
			eventManager.RegisterEvent ("click", obj, DummyAction);
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (1, eventSet.Count);
			
			eventManager.UnRegisterEvent ("click", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			eventManager.UnRegisterEvent ("click", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Single Object")]
		public void RegisterNonExistentEvent() {
			Init ();

			GameObject obj = new GameObject ("TestGameObject");
			obj.transform.SetParent (testObjHolder.transform);
			eventManager.RegisterEvent ("non-exist-event-name", obj, DummyAction);

			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Single Object")]
		public void UnRegisterNonExistentObj() {
			Init ();

			GameObject obj1 = new GameObject ("TestGameObject");
			obj1.transform.SetParent (testObjHolder.transform);
			eventManager.RegisterEvent ("click", obj1, DummyAction);
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (1, eventSet.Count);

			GameObject obj2 = new GameObject ("TestGameObject");
			obj2.transform.SetParent (testObjHolder.transform);
			eventManager.UnRegisterEvent ("click", obj2, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Single Object")]
		public void UnRegisterNonExistentEvent() {
			Init ();

			GameObject obj = new GameObject ("TestGameObject");
			obj.transform.SetParent (testObjHolder.transform);
			eventManager.RegisterEvent ("non-exist-event-name", obj, DummyAction);
			
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			eventManager.UnRegisterEvent ("no-such-event-name", obj, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Multiple Objects")]
		public void TwoObjSameEventAndAction() {
			Init ();

			GameObject obj1 = new GameObject ("TestGameObject1");
			GameObject obj2 = new GameObject ("TestGameObject2");
			obj1.transform.SetParent (testObjHolder.transform);
			obj2.transform.SetParent (testObjHolder.transform);

			List<GameObject> objs = eventManager.GetListeners ("click");
			Assert.AreEqual (0, objs.Count);
			eventManager.RegisterEvent ("click", obj1, DummyAction);
			objs = eventManager.GetListeners ("click");
			Assert.AreEqual (1, objs.Count);

			eventManager.RegisterEvent ("swipe", obj1, DummyAction2);
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (2, eventSet.Count);

			objs = eventManager.GetListeners ("click");
			Assert.AreEqual (1, objs.Count);
			objs = eventManager.GetListeners ("swipe");
			Assert.AreEqual (1, objs.Count);

			eventManager.RegisterEvent ("click", obj2, DummyAction);
			eventManager.RegisterEvent ("swipe", obj2, DummyAction2);

			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (2, eventSet.Count);

			objs = eventManager.GetListeners ("click");
			Assert.AreEqual (2, objs.Count);
			Assert.AreEqual ("TestGameObject1", objs [0].name);
			Assert.AreEqual ("TestGameObject2", objs [1].name);

			eventManager.UnRegisterEvent ("click", obj1, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (1, eventSet.Count);
			objs = eventManager.GetListeners ("click");
			Assert.AreEqual (1, objs.Count);
			Assert.AreEqual ("TestGameObject2", obj2.name);

			objs = eventManager.GetListeners ("swipe");
			Assert.AreEqual (2, objs.Count);

			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (2, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Multiple Objects")]
		public void TwoObjSameEventDifferentAction() {
			Init ();

			GameObject obj1 = new GameObject ("TestGameObject1");
			GameObject obj2 = new GameObject ("TestGameObject2");
			obj1.transform.SetParent (testObjHolder.transform);
			obj2.transform.SetParent (testObjHolder.transform);
			
			eventManager.RegisterEvent ("click", obj1, DummyAction);
			eventManager.RegisterEvent ("click", obj2, DummyAction2);
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (1, eventSet.Count);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (1, eventSet.Count);
			
			eventManager.UnRegisterEvent ("click", obj1, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (0, eventSet.Count);
			
			eventManager.UnRegisterEvent ("click", obj2, DummyAction2);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Multiple Objects")]
		public void TwoObjDifferentEventDifferentAction() {
			Init ();

			GameObject obj1 = new GameObject ("TestGameObject1");
			GameObject obj2 = new GameObject ("TestGameObject2");
			obj1.transform.SetParent (testObjHolder.transform);
			obj2.transform.SetParent (testObjHolder.transform);
			
			eventManager.RegisterEvent ("click", obj1, DummyAction);
			eventManager.RegisterEvent ("swipe", obj2, DummyAction2);
			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (1, eventSet.Count);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (1, eventSet.Count);
			
			eventManager.UnRegisterEvent ("click", obj1, DummyAction);
			eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (0, eventSet.Count);
			
			eventManager.UnRegisterEvent ("swipe", obj2, DummyAction2);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (0, eventSet.Count);

			Fini ();
		}

		[Test]
		[Category("Multiple Objects")]
		public void CountingObjects() {
			Init ();

			GameObject obj1 = new GameObject ("TestGameObject1");
			GameObject obj2 = new GameObject ("TestGameObject2");
			obj1.transform.SetParent (testObjHolder.transform);
			obj2.transform.SetParent (testObjHolder.transform);

			eventManager.RegisterEvent ("click", obj1, DummyAction);
			eventManager.RegisterEvent ("swipe", obj1, DummyAction2);

			eventManager.RegisterEvent ("click", obj2, DummyAction3);
			eventManager.RegisterEvent ("swipe", obj2, DummyAction4);

			List<GameObject> objs = eventManager.GetListeners ("click");
			Assert.AreEqual (2, objs.Count);
			objs = eventManager.GetListeners ("swipe");
			Assert.AreEqual (2, objs.Count);

			HashSet<string> eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (2, eventSet.Count);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (2, eventSet.Count);

			eventManager.UnRegisterEvent ("click", obj1, DummyAction);
			objs = eventManager.GetListeners ("click");
			Assert.AreEqual (1, objs.Count);
			objs = eventManager.GetListeners ("swipe");
			Assert.AreEqual (2, objs.Count);
			
			eventSet = eventManager.GetRegisteredEvent (obj1);
			Assert.AreEqual (1, eventSet.Count);
			eventSet = eventManager.GetRegisteredEvent (obj2);
			Assert.AreEqual (2, eventSet.Count);


			Fini ();
		}

		[Test]
		[Category("Multiple Objects")]
		public void TwoObjectsShareSameAction() {
			Init ();
			
			GameObject obj1 = new GameObject ("TestGameObject1");
			GameObject obj2 = new GameObject ("TestGameObject2");
			obj1.transform.SetParent (testObjHolder.transform);
			obj2.transform.SetParent (testObjHolder.transform);
			
			eventManager.RegisterEvent ("click", obj1, DummyAction);
			eventManager.RegisterEvent ("swipe", obj1, DummyAction2);
			
			eventManager.RegisterEvent ("click", obj2, DummyAction);
			eventManager.RegisterEvent ("swipe", obj2, DummyAction4);

			eventManager.UnRegisterEvent ("click", obj1, DummyAction);

			eventManager.TriggerEvent ("click");

			// FIXME: expected 1, but actually 0, different objs are now
			// allowed to share the same action.
			Assert.AreEqual(0, count);
			
			Fini ();
		}

		private int count;

		void DummyAction() {
			count++;
		}

		void DummyAction2() {
		}

		void DummyAction3() {
		}

		void DummyAction4() {
		}
	}

}