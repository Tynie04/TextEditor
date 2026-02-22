# TextEditor

A custom text editor built from scratch in C# using OpenGL for rendering. This project implements low-level text editing with custom buffer management, cursor navigation, and OpenGL-based rendering using bitmap fonts.

## Features

### Core Editing

- **Text Insertion**: Type characters directly into the editor
- **Text Deletion**: Backspace and Delete key support
- **Cursor Navigation**: Move cursor with arrow keys (`←` `↑` `↓` `→`)
- **Line Management**: Create new lines with Enter key
- **Multi-line Support**: Full support for documents with multiple lines

### File Operations

- **Save**: `Ctrl+S` - Save current document
- **Open**: `Ctrl+O` - Load a file from disk
- **New**: `Ctrl+N` - Create a new document

### Rendering

- **Custom OpenGL Rendering**: Direct OpenGL rendering pipeline using OpenTK
- **Bitmap Font System**: Efficient text rendering using the Unifont bitmap font
- **Viewport System**: Scroll through documents larger than the visible area
- **Real-time Updates**: Immediate visual feedback for all editing operations

## Architecture

### Project Structure

```
TextEditor/
├── Commands/              # Command pattern implementations for editor actions
│   ├── EditorCommand.cs
│   ├── EditorCommands.cs     # Cursor movement commands
│   ├── EditCommands.cs       # Text modification commands
│   └── DocumentCommands.cs   # File operation commands
├── Editor/                # Core editor logic
│   ├── TextBuffer.cs         # Text storage and manipulation
│   ├── Cursor.cs             # Cursor position and movement
│   ├── Document.cs           # Document state management
│   ├── EditorController.cs   # Central coordinator for editor operations
│   └── Viewport.cs           # Visible area management
├── Input/                 # Input handling
│   ├── KeyCommandMapper.cs   # Maps keyboard input to commands
│   ├── RawInputEvent.cs
│   ├── RawKeyEvent.cs
│   └── RawTextEvent.cs
├── Platform/              # Cross-platform abstractions
│   ├── IFileDialogService.cs
│   └── CrossPlatformFileDialogService.cs
└── Rendering/             # Graphics and rendering
    ├── EditorWindow.cs       # Main OpenGL window
    ├── TextRenderer.cs       # Text drawing logic
    ├── BitmapFont.cs         # Bitmap font loading and management
    ├── Glyph.cs
    ├── Texture.cs
    └── ITexture.cs
```

### Design Patterns

**Command Pattern**: All editor operations (cursor movement, text insertion, file operations) are implemented as commands, allowing for future features like undo/redo.

**MVC-inspired Architecture**:

- **Model**: `TextBuffer`, `Document`, `Cursor`
- **View**: `EditorWindow`, `TextRenderer`, `BitmapFont`
- **Controller**: `EditorController`, `KeyCommandMapper`

**Dependency Injection**: Platform-specific services (like file dialogs) are injected through interfaces for testability and cross-platform support.

## Technology Stack

- **.NET 8.0**: Modern C# with implicit usings and nullable reference types
- **OpenTK 4.9.4**: OpenGL bindings for .NET
- **NativeFileDialogSharp**: Cross-platform file dialog support
- **ImageSharp 3.1.12**: Image processing for loading bitmap fonts
- **xUnit**: Unit testing framework

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows, Linux, or macOS (OpenTK is cross-platform)
- Graphics card with OpenGL 3.3 support

### Building the Project

```bash
# Clone the repository
git clone https://github.com/Tynie04/TextEditor.git
cd TextEditor

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the editor
dotnet run --project TextEditor
```

### Running Tests

```bash
dotnet test
```

## Keyboard Shortcuts

| Shortcut        | Action                         |
| --------------- | ------------------------------ |
| `←` `→` `↑` `↓` | Move cursor                    |
| `Backspace`     | Delete character before cursor |
| `Delete`        | Delete character after cursor  |
| `Enter`         | Insert new line                |
| `Ctrl+S`        | Save file                      |
| `Ctrl+O`        | Open file                      |
| `Ctrl+N`        | New document                   |

## Testing

The project includes unit tests covering:

- **TextBuffer**: Text insertion, deletion, line management, and file I/O
- **Cursor**: Position tracking and movement validation
- **BitmapFont**: Font loading and glyph rendering

Run tests using:

```bash
dotnet test TextEditor.Tests
```

## Technical Highlights

### Custom Text Buffer

The `TextBuffer` class implements a line-based text storage system optimized for typical text editing operations:

- Efficient character insertion and deletion
- Line splitting and merging
- File reading and writing

### OpenGL Rendering Pipeline

- Custom vertex and fragment shaders
- Batch rendering for efficient text display
- Bitmap font atlas system
- Orthographic projection for 2D text rendering

### Viewport Management

- Automatic scroll adjustment to keep cursor visible
- Efficient rendering of only visible lines
- Dynamic viewport resizing

## Backlog

- [ ] Replace `List<string>` with gap buffer or piece table for better performance
- [ ] Benchmark and optimize large file operations
- [ ] Implement undo/redo system with history stack
- [ ] Batch text operations for efficient undo
- [ ] Text selection with visual highlighting
- [ ] Cut, copy, and paste operations
- [ ] Recent files list
- [ ] UI improvements and customization options
- [ ] Adjustable font size and text colors
