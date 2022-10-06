# ShopScene
Build for the application at PsyCurio by Frederick Thiemer.

# Documentation
The GameObjects in the ShopScene holding important functionality are marked with tags used for setup and testing.
The tags are stored in a static class in Assets/Scripts/Tags.cs.


# Known Bugs
1. In the Editor sometimes after testing, all devices in the input system are removed and the following error pops up: "A Native Collection has not been disposed, resulting in a memory leak. Allocated from:...". 
    - This can be fixed by restarting the Unity Editor. 
    - The reason for the memory leak seems to be a call in the Input System class "InputTestRuntime", which is called by the class "InputTestFixture", which I subclass in the playmode tests to simulate input.
