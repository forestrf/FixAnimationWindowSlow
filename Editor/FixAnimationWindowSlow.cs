/*
Copyright 2020 Andrés Leone Gámez

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
https://github.com/forestrf
When a gameobject that has or it's parent has an Animator is selected, the AnimationWindow will be initialized around it. Let's call it Pepe.
That object will be forgotten when a new Animator is inspected.
The AnimationWindow calls GetComponent many times when playing an animation. The deeper Pepe is in the hierarchy of the Animator, the more GetComponents get called, deep*animationComplexity times.
To have good performance, Pepe must be the object that contains the Animator itself and no a child.
Selecting any child will not update Pepe because the check (in UnityEditor.AnimationWindow.ShouldUpdateGameObjectSelection) returns false.
*/

public static class FixAnimationWindowSlow {
	static Type windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
	static FieldInfo animEditorField = windowType.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
	static Type m_AnimEditorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimEditor");
	static PropertyInfo selectionProperty = m_AnimEditorType.GetProperty("selection");
	static Type selectionType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.AnimationWindowSelectionItem");
	static PropertyInfo pepeProperty = selectionType.GetProperty("gameObject");

	[InitializeOnLoadMethod]
	private static void Init() {
		EditorApplication.update += OnUpdate;
	}

	private static void OnUpdate() {
		IList windows = (IList) windowType.GetMethod("GetAllAnimationWindows").Invoke(null, new object[0]);
		foreach (EditorWindow window in windows) {
			var m_AnimEditor = animEditorField.GetValue(window);
			if (m_AnimEditor == null) continue;

			var selection = selectionProperty.GetValue(m_AnimEditor);
			if (selection == null) continue;

			GameObject pepe = pepeProperty.GetValue(selection) as GameObject;
			if (pepe == null) continue;


			// If Pepe doesn't contain the Animator:
			var animator = pepe.GetComponentInParent<Animator>();
			if (animator != null && animator.gameObject != pepe) {

				// Set Pepe to null
				pepeProperty.SetValue(selection, null);

				// Select root
				UnityEngine.Object prevObject = Selection.activeObject;
				Selection.activeObject = animator.gameObject;

				// Call UnityEditor.AnimationWindow.OnSelectionChange
				window.GetType().GetMethod("OnSelectionChange").Invoke(window, new object[0]);

				// Restore previous selection
				Selection.activeObject = prevObject;
			}
		}
	}
}
