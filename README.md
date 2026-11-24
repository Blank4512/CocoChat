# CocoChat

A learning project: the same chat app built several times, starting from raw TCP in a console and moving up to SignalR with a UI. Each version lives in its own folder so the progression stays visible.

## Versions

| Folder | Tech | Status |
|---|---|---|
| `01-tcp-chat` | C# console apps, `TcpListener` / `TcpClient` | Done |
| `02-signalr-chat` | ASP.NET Core + SignalR, WPF client | Planned |

## 01 - TCP chat

A multi-client chat server and a console client, built on plain TCP with no chat library.

### Features
- Multiple clients at once, each handled in its own task
- Usernames with unique IDs, shown as `Name#ID` (duplicate names are allowed)
- Broadcast messages, plus join and leave announcements
- Commands: `/list`, `/u Name#ID message` (private message), `/help`, `/quit`
- Clean disconnect handling, including clients that drop abruptly
- Server shutdown from the keyboard, with a notice sent to connected clients

### Run it
1. Open the solution in Visual Studio (.NET 8 or later).
2. Set both projects (`ChatServer`, `ChatClient`) as startup projects.
3. Press F5. To add more clients, right-click `ChatClient` and choose Debug > Start New Instance.
4. Type a username in each client window, then start chatting.

The server listens on `127.0.0.1:5000`.

### What I learned
- TCP is a byte stream, so messages need framing (here: one message per line)
- `ReadLineAsync` returns `null` on a clean close and throws `IOException` on an abrupt one
- Starting a task without `await` is how one server serves many clients
- Shared state needs thread-safe collections, and concurrent writes to one stream need a lock

### Known limitations
- No encryption, authentication or reconnect
- Messages are plain text lines, so there is no structured protocol
- Local connections only by default

## Roadmap
- [x] Stage 1: single client echo
- [x] Stage 2: continuous messaging and disconnect handling
- [x] Stage 3: multiple clients and broadcast
- [x] Stage 4: usernames, commands, private messages, shutdown
- [ ] SignalR version with groups and rooms
- [ ] WPF client
- [ ] Message history with EF Core and SQLite
