# arena-collector

In this minigame, players control cars inside a confined area, aiming to collect money while avoiding incoming traffic.

Gameplay Area

The playfield is circular, and cars must remain inside its boundaries.

Between 2 and 4 players participate at the same time.

Each player controls a car with a unique color.

One player is controlled by a human using mouse or touch input.

The remaining players are driven by computer-controlled logic.

At the start, each player spawns in a different quadrant of the circle.

After a brief countdown, the round begins.

Objectives & Pickups

Cash pickups appear at random locations inside the arena.

Each pickup has a different monetary value: 1K, 5K, 10K, or 50K.

To avoid clutter, the number of pickups on screen at any time should be limited.

When multiple players race toward the same pickup, only the first one to reach it receives the money.

Each player maintains a running total of collected cash.

Traffic Obstacles

In addition to player cars, there are grey traffic vehicles controlled by the game.

These traffic cars spawn at unpredictable moments and positions along the outer edge of the arena.

Once spawned, they travel across the playfield following some form of predefined or generated path.

After completing their route, traffic vehicles are removed from the game.

Traffic cars cannot collect money.

Player cars do not collide with one another, but collisions with traffic are possible.

Match Duration

Each session is designed to be short and intense.

A full match should last roughly 30 seconds.

AI Expectations

Computer-controlled players should, at minimum:

Choose a money pickup to pursue.

React to traffic cars in a simple way, such as stopping briefly or changing direction to avoid collisions.

Scoring Rules

Players earn money by collecting pickups.

Each time a player crashes into a traffic car, 10K is deducted from their total.

Every player should have a small on-screen UI element that displays their current score.
