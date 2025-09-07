# Word Puzzle Game (WPG)

## Project Overview

Word Puzzle Game is a Unity-based puzzle game where players drag letter clusters to form words in designated slots. The game features multiple levels, save/load functionality, and a clean UI with victory screens and level progression.

## Architecture Overview

### Event-Driven Mediator Topology

The application follows an **event-driven mediator topology** where different modules communicate through a centralized mediation system rather than direct coupling. This architecture provides:

- **Loose coupling** between components
- **Centralized flow control** through mediator classes
- **Event-based communication** using reactive programming (R3 library)
- **Clean separation of concerns** across different game modules

### N-ary Tree Topology

The project maintains an **n-ary tree topology** structure where:

- **Root Level**: Bootstrap module serves as the application entry point
- **Branch Nodes**: Menu and Gameplay modules represent major game states
- **Leaf Nodes**: Individual controllers, views, and services form the terminal nodes
- **Hierarchical Dependencies**: Each level depends on services from parent levels but remains isolated from siblings

```
Bootstrap (Root)
├── Menu Module
│   ├── MenuFlow
│   ├── MenuScope
│   └── MenuView
└── Gameplay Module
    ├── GameplayFlow
    ├── GameplayScope
    ├── LevelController
    └── Views (GameField, WordSlot, LetterCluster, Victory)
```

## UI Architecture - MVVM Pattern

The UI layer implements the **Model-View-ViewModel (MVVM)** pattern, though this may be considered somewhat excessive for a game of this scope:

### Components:
- **Models**: Data structures (DTOs, level data)
- **ViewModels**: Business logic and state management with reactive properties (R3 Observables)
- **Views**: UI presentation layer inheriting from base `View` class

### Implementation:
- **Views** subscribe to **ViewModel** properties using reactive streams
- **Commands** handle user interactions through reactive commands
- **Data binding** achieved through R3 Observable subscriptions
- **Lifecycle management** through proper disposal patterns

## Key Architectural Components

### 1. Flow Pattern (Mediator Implementation)
Each major module has a **Flow** class that acts as a mediator:

- **BootstrapFlow**: Orchestrates application startup and scene transitions
- **MenuFlow**: Manages menu state and navigation
- **GameplayFlow**: Coordinates gameplay initialization and component setup

**Characteristics**:
- Implement `IStartable` interface for VContainer integration
- Handle cross-module communication
- Manage loading sequences and progress reporting
- Coordinate between controllers, views, and services

### 2. Dependency Injection (VContainer)
**Scope** classes define dependency graphs for each module:

- **LifetimeScope inheritance** for hierarchical DI container structure
- **Scoped lifetimes** for module-specific components
- **Singleton services** for cross-module utilities
- **Entry points** registration for Flow classes

### 3. Loading System
Centralized loading management:

- **LoadingService**: Coordinates loading operations across modules
- **ILoadUnit**: Interface for loadable components
- **Loading progress tracking** with visual feedback
- **Async/await patterns** using UniTask

### 4. Save System
Persistent data management:

- **ISaveController**: Interface for save/load operations
- **JSON serialization** for game state persistence
- **DTO pattern** for data transfer objects
- **Async file operations** with error handling

### 5. Logging System
Centralized logging with tagged categories:

- **Tagged logging** for different subsystems (Bootstrap, Gameplay, etc.)
- **Structured logging** with consistent formatting
- **Performance monitoring** capabilities

## Project Structure

```
WPG.Runtime/
├── Bootstrap/           # Application startup and initialization
│   ├── BootstrapFlow   # Main application flow coordinator
│   ├── BootstrapScope  # DI container configuration
│   └── LoadingController # Loading state management
├── Menu/               # Main menu functionality
│   ├── MenuFlow       # Menu state coordinator
│   ├── MenuScope      # Menu DI configuration
│   └── MenuView       # Menu UI presentation
├── Gameplay/          # Core game functionality
│   ├── GameplayFlow   # Game state coordinator
│   ├── GameplayScope  # Gameplay DI configuration
│   ├── LevelController # Level management
│   ├── ViewModels/    # MVVM ViewModels
│   └── Views/         # UI Views (GameField, WordSlot, etc.)
├── Data/              # Data structures and DTOs
├── Persistent/        # Runtime constants and configuration
└── Utilities/         # Shared services and utilities
    ├── Loading/       # Loading system
    ├── SaveController/ # Persistence system
    ├── Logging/       # Logging infrastructure
    └── AddressablesController/ # Asset loading
```

## Additional Architectural Decisions

### 1. Reactive Programming (R3)
- **Observable streams** for state management and UI updates
- **Command pattern** for user interactions
- **Automatic disposal** through CompositeDisposable
- **Event-driven updates** replacing traditional callbacks

### 2. Async Programming (UniTask)
- **Non-blocking operations** for loading and save operations
- **Cancellation support** for long-running operations
- **Unity-optimized** async operations
- **Better performance** compared to standard Tasks

### 3. Addressable Assets
- **Dynamic asset loading** for levels and resources
- **Memory management** with asset reference counting
- **Flexible content organization** for future expansions

### 4. Component Lifecycle Management
- **IDisposable pattern** for proper resource cleanup
- **Hierarchical disposal** following the n-ary tree structure
- **Memory leak prevention** through careful subscription management

## Benefits of This Architecture

1. **Scalability**: Easy to add new game modes or features
2. **Testability**: Clean separation enables unit testing
3. **Maintainability**: Clear boundaries and responsibilities
4. **Performance**: Efficient resource management and loading
5. **Flexibility**: Modular design supports various game configurations

## Trade-offs

1. **Complexity**: MVVM might be excessive for simple game UI
2. **Learning Curve**: Multiple patterns require understanding of various concepts
3. **Performance Overhead**: Additional abstraction layers
4. **Development Time**: More setup required compared to simpler architectures

The architecture prioritizes maintainability and extensibility over simplicity, making it well-suited for a game that may expand with additional features and content.