# AgeOfEmpire — RTS Architecture Demo (Unity)

A focused technical demo built to demonstrate **RTS-style gameplay architecture** in Unity: selection + right-click commands, unit capabilities, state-driven harvesting/building, and data-driven spawning via Addressables.

This project intentionally prioritizes **engineering decisions, performance awareness, and extensibility** over visuals or feature breadth. It is prepared as a code review sample for a gameplay/engineer role.

---

## Scope & Constraints

- Time box: prototype / technical demo (not production-complete)
- Platform / Engine / Runtime: Unity **6000.2.8f1** (Unity 6.2 Tech Stream)
- Non-goals:
  - Polished UI / final art
  - Full feature completeness (fog of war, tech tree, full AI, multiplayer, save/load)

Focus areas:
- Command-oriented input (right-click contextual actions)
- Unit capabilities + State/Strategy patterns for behavior
- Data-driven content loading (Addressables + config data)
- Allocation-aware gameplay loops (pooling, throttled retargeting, minimal per-frame allocations)

---

## How to Run

- Engine / Runtime version: Unity **6000.2.8f1**
- Open scene / entry file: `Assets/Scenes/Gameplay.unity`
- Press Play in the editor.

Controls (default):
- Left mouse: click to select, drag-box to multi-select
- Right mouse: contextual command (move/attack/harvest/build) based on what you click
- Camera:
  - Arrow keys: pan (accelerates while held)
  - Mouse wheel: zoom
  - Edge scroll: move camera when cursor hits screen edge
  - Middle mouse: drag to pan

---

## Recommended Code Entry Points

For reviewers who want to jump directly into the architecture:

- `Assets/Scripts/Gameplay/InputCommandResolver/InputCommandResolverController.cs` – right-click input → command resolution
- `Assets/Scripts/Gameplay/Unit/CommandExecutor.cs` – command queue + active command ticking
- `Assets/Scripts/Gameplay/Commands/BaseCommand.cs` – command lifecycle (template method)
- `Assets/Scripts/Gameplay/Unit/Controllers/UnitCombatController.cs` – combat loop (target, chase, cooldown attacks)
- `Assets/Scripts/Gameplay/Unit/Controllers/UnitHarvesterController.cs` – harvesting state machine + deposits
- `Assets/Scripts/Gameplay/Buildings/BuildModeController.cs` – building preview, placement validation, build command issuing
- `Assets/Scripts/Gameplay/GridSystem/GridManager.cs` – grid reservation system for movement + placement
- `Assets/Scripts/Gameplay/MapGenerator.cs` – procedural starter-layout generator (N-Queens region placement)
- `Assets/Scripts/Config/ConfigManager.cs` – loads unit/building costs from config data
- `Assets/Scripts/Gameplay/Events/EventChannelSO.cs` – ScriptableObject event channels (UI/gameplay decoupling)

Supporting docs:
- `ProjectSummary.txt` – implemented systems overview
- `DesignPatterns.txt` – pattern inventory with code references

---

## Architecture Overview

High-level flow:

```
Player Input (mouse)
    ↓
Selection (SelectionController) + Context Resolver (InputCommandResolverController)
    ↓
ICommand instances (Move/Attack/Harvest/Build)
    ↓
CommandExecutor (queue + tick active command)
    ↓
Unit Capabilities (Movement/Combat/Harvester/Builder controllers)
    ↓
World changes (NavMesh movement, damage, resource transfer, building progress)
    ↓
UI refresh (ScriptableObject event channels)
```

Key principles:
- Separation of concerns: input resolves “intent”, commands orchestrate, controllers execute capability logic.
- Extensibility: adding a new action is typically “new resolver + new command + capability method”.
- Data-driven content: buildings/units/icons are loaded by key, costs come from config data.

---

## Key Technical Decisions

### Why a Command system for right-click actions?
- Keeps input handling small and testable: input only decides which command to create.
- Commands provide a clean lifecycle (Execute/Update/Cancel/Complete) and readable “intent” objects.
- Enables queueing and future extensions (shift-queue, patrol, attack-move) without rewriting unit controllers.

### Why State machines for harvesting/building?
- Harvesting/building are multi-step behaviors (move → act → transition), which are clearer as explicit states.
- Avoids giant Update() methods and makes edge cases (target depleted, inventory full, deposit missing) easier to handle.

### Why Strategy for combat?
- Melee and ranged/projectile attacks share a loop but differ in execution details.
- Strategies encapsulate “how to attack” and (optionally) “how to space while chasing”.

### Why a lightweight grid reservation layer?
- Provides deterministic spacing: movement reserves target cells to reduce stacking.
- Reuses the same grid occupancy concept for building placement validation.

### Why Addressables?
- Demonstrates scalable asset loading patterns (spawn by key, icons by key).
- Keeps code paths data-driven: `Constructions/{id}.prefab`, `Units/{id}.prefab`, `Icons/{...}.png`.

---

## Performance & Engineering Considerations

- Object pooling:
  - `Assets/Scripts/ObjectPool.cs` used for FX/projectiles to avoid frequent Instantiate/Destroy.
- Collection pooling:
  - `UnityEngine.Pool.ListPool<T>` used while issuing commands to selected units.
- Allocation avoidance:
  - Many systems use cached buffers / preallocated arrays (example: combat overlap results).
- Throttled path updates:
  - Combat chasing retargets at a fixed interval (0.2s) rather than every frame to reduce NavMesh path churn.

---

## Testing & Validation

Manual test scenarios (editor Play Mode):
- Selection: drag-select multiple units, verify selection visuals and command issuing.
- Move: right-click ground, verify units navigate and avoid stacking via reservations.
- Harvest: right-click a resource, verify harvest → inventory fill → deposit at best drop-off → UI updates.
- Build: open build UI, place building preview, verify invalid placement feedback and builder assignment.
- Combat: right-click enemy, verify chase, attack cooldown, and death handling.
- Map generation: generate with different faction counts/seeds and verify separated spawns and resources.

---

## Known Limitations

- No fog of war, tech tree/research, population caps, save/load, networking, or robust AI director.
- UI is functional but intentionally minimal.
- Addressables setup is assumed to be present in the project; this repo focuses on code/architecture review.

These are intentional given the project scope and time constraints.

---

## Future Improvements

If extended further, the next steps would be:

- More performant technique to optimize draw calls create by skinned mesh renderer of units.
- Add shift-queue commands and composite commands (patrol, attack-move).
- Add basic AI managers (economy + combat) using the existing command layer.
- Add AI for other factions to play with Player1.
- Add behaviour tree AI for units to handle complex behaviors (patrol, attack, defend).
- Introduce formal unit tests for pure logic (config parsing, command state transitions).
- Add fog of war and visibility-based targeting.

---

## FAQ

**Q: What should I review first to understand the architecture quickly?**  
A: Start at `InputCommandResolverController` → `CommandExecutor` → `BaseCommand`, then jump into one capability loop (combat or harvest).

**Q: Why use ScriptableObject event channels instead of direct references?**  
A: To decouple UI and gameplay systems so scenes can be wired in the inspector without hard dependencies between controllers. FindReferences plugins in imported to handle events "Find References" feature like in IDE.

**Q: What part best represents your engineering skills?**  
A: The right-click command pipeline (resolver → command → executor → capability) and the harvesting/building state machines. Moreover, the awareness of GC allocation and asynchronous code for heavy task

---

## Notes for Reviewers

- This project is designed to be reviewed primarily from a **code and architecture perspective**.
- The most relevant code is under `Assets/Scripts/Gameplay` and `Assets/Scripts/GameplayUI`.
- Plugins and art packs are included only to make the demo runnable; architectural focus is in the gameplay code.
