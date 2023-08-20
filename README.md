# Desdemona

> Powerful Othello AI developed with Rust, Unity3D, WebGL, and Node.js

Development in progress... Stay tuned!

![](https://github.com/SeoulSKY/Desdemona/assets/48105703/0acd74f3-2b42-4949-8c9e-f156ff72da08)

## How to Play
* Move the player with `WASD` keys on keyboard and look around with the mouse
* Left-click the yellow tile to place a disk

## How to Build and Run

### Installation

This application requires Docker, Docker compose, Node.js and Unity3D. Please install them first.
* [Docker installation](https://www.docker.com/get-started)
* [Docker compose installation](https://docs.docker.com/compose/install/)
* [Node.js installation](https://nodejs.org/en)
* [Unity3D installation](https://unity.com/download)

### Build

First, open the folder `desdemona` in Unity and close it. This will create the necessary files to build
Then, change the directory into the folder `web-server` and run the following commands:
```
npm install
ts-node src/buildUnity.ts
```
This will build and move the Unity project to the `web-server`.

### Run

To run the app, use the following command from the folder where `docker-compose.yml` is located:
```
docker-compose up --build
```
When the `web-server` is ready, visit [here](http://localhost:8080) to play!
