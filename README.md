# ShopScene
Build for the application at PsyCurio by Frederick Thiemer.

# Use of Tags
The GameObjects in the ShopScene holding important functionality are marked with tags used for setup and testing.
The tags are stored in a static class in Assets/Scripts/Tags.cs.

# Known Bugs
1. In the Editor occasionally after testing, all devices in the input system are removed and the following error pops up: "A Native Collection has not been disposed, resulting in a memory leak. Allocated from:...". 
    - This can be fixed by restarting the Unity Editor. 
    - The reason for the memory leak seems to be a call in the Input System class "InputTestRuntime", which is called by the class "InputTestFixture", which I subclass in the playmode tests to simulate input.
2. ~~Text looks bad on lower resolutions (fizzles out on the edges).~~ -> Fixed (Anti-Aliasing was the problem, created overlay camera without pre-processing to fix it)
    - ~~It looks ok/good on 1920x1080.~~ 
	- ~~Was not fixable by regenerating the used font set and fixing the sampling point to padding ratio at near 10% or more, as recommended on unity answers.~~