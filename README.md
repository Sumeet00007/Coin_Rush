2D Multiplayer Coin Rush 

1) Networking Solution: 

Uses Photon Fusion with a Host/Client architecture. Fusion handles player connections, networked objects, input synchronization, and shared game state. Each room supports 2 players using a 6-character room code.

2) Architecture: 
Network Manager – Creates/joins rooms and manages the Fusion session.
Lobby Manager – Handles player readiness and match start.
Player Spawner – Spawns the two networked players.
Coin Spawner – Controls the single shared coin and its spawn position.
Score Manager – Manages scores, timer, and winner.
Player Respawner – Handles player respawning after falling.

The State Authority controls important gameplay decisions to keep both devices synchronized.

3) Coin Synchronization:

Only one networked coin is used. The State Authority selects its spawn position and synchronizes that position with both players. When collected, the collector's score is updated and the same coin is moved to a new position, ensuring both players see the same coin location.

4) Assumptions: 
Maximum 2 players per match.
Both players must join before the match starts.
Players are assigned as Player 1 and Player 2 based on connection order.
Coin locations are predefined in the level.
The State Authority is trusted for gameplay state.

5) Known Limitations:
No host migration or reconnection handling.
No matchmaking; players join using a room code.
Draw state has no dedicated result UI.
Game depends on a stable network connection.

6) Bonus Features
6-character room code system.
Multiplayer lobby with player readiness.
60-second synchronized match timer.
Single shared networked coin with randomized respawning.
Prevents consecutive spawning at the same position.
Winner UI with final coin count.
Automatic return to the main menu.
Player-specific respawn points.