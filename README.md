# FreeSpinsGame

A backend service for handling player spins in campaigns with concurrency safety using Redis distributed locks.

---

## How to Run

1. Run **docker-compose up --build** command to run the project with redis and mssql server
2. Run **dotnet test --filter "Category=Integration"** to run an integration test with multiple simultaneous spin requests
3. Run **dotnet test --filter "Category=Unit"** to run unit tests
4. Enter http://localhost:5000/swagger/index.html url to open swagger


## How Concurrency is Handled

Uses a distributed Redis lock (RedLock) with keys formatted as lock:spin:{campaignId}:{playerId}.

Only one spin per player per campaign can be processed at a time.

If the lock is not acquired, the request fails with 403 (Forbidden).

Lock expiry is set to a short duration (e.g., 5 seconds) to prevent deadlocks.
