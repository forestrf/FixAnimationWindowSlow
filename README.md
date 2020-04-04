# FixAnimationWindowSlow

Fix for a bug that slows down previewing and editing animations with the `AnimationWindow`.

The slowdown appears if the first clicked object that initializes the `AnimatorWindow` is a child of the `Animator` instead of the `GameObject` that has the `Animator`.

Another fix without using this scripts is to click something else that changes the `AnimationWindow` scope and then clicking on the `GameObject` that has the `Animator`. But that's annoying...
