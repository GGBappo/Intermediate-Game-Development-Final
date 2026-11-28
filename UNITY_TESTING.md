# Last Delivery — Unity Test Framework

This branch includes an initial automated test suite for the Intermediate Game Development final project.

## Setup

- Unity editor version from `ProjectSettings/ProjectVersion.txt`: **6000.5.9f1**.
- Unity Test Framework is already present in `Packages/packages-lock.json` (1.7.0 transitively). If the Test Runner is unavailable in the Editor, install **Test Framework** from Package Manager.
- `Assets/Scripts/LastDelivery.Gameplay.asmdef` compiles runtime gameplay scripts into a reusable assembly. It explicitly references `Unity.TextMeshPro` and `UnityEngine.UI`.
- `Assets/Scripts/Tests/Tests.asmdef` is the **EditMode** test assembly.
- `Assets/Tests/PlayMode/LastDelivery.PlayModeTests.asmdef` is the **PlayMode** test assembly.
- Two unused editor-only imports were removed from `CargoManager.cs`, which now compiles as runtime code.

## How to run

1. Open the Unity project in **Unity 6000.5.9f1**.
2. Let Unity import the new assets and compile scripts.
3. Open **Window > General > Test Runner** (or **Window > General > Test Framework**).
4. Select **EditMode** and run all tests.
5. Select **PlayMode** and run all tests.

### EditMode tests

`Assets/Scripts/Tests/CargoManagerEditModeTests.cs`:

- Zero maximum health returns zero stars.
- Seven scoring boundary cases return the correct number of stars.
- Active and delivered health combine when calculating the live rating.
- Package damage reduces HP and notifies subscribers.
- Partial damage keeps the correct remaining health.

### PlayMode tests

`Assets/Tests/PlayMode/DeliveryPlayModeTests.cs`:

- A package entering its assigned trigger checkpoint completes delivery.
- Entering the wrong checkpoint does not complete delivery.
- Package damage forwards an event, updates total damage and live rating.
- Destroyed packages are removed, counted and assigned zero rating.

The PlayMode tests construct temporary GameObjects, ScriptableObjects, Rigidbody components and trigger colliders; they do not load or modify the playable scene. Tests restore `Time.timeScale` after each case.

## Notes

- The test suite has **not been executed in the Unity Editor** from the GitHub integration. Passing results must be verified locally.
- The current `CargoManager.DeliveriesCompleted` and `DeliveriesFailed` properties derive counts from `deliveries.Count` and `activePackages.Count`. These do not independently distinguish delivered versus destroyed packages; consider addressing this separately if the results UI requires accurate delivered/failed counts.
- Package destruction is intentionally tested in PlayMode, where `Destroy(gameObject)` is supported.
- The tests use reflection to configure existing private fields rather than changing production APIs solely for testing.
