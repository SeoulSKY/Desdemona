# Desdemona

> Powerful Othello AI developed with Rust, Unity3D, WebGL, and Node.js

[Click here to play!](https://desdemona.seoulsky.org) (Using Chrome browser is recommended)

<https://github.com/SeoulSKY/Desdemona/assets/48105703/60d35a72-98dc-4799-ad6d-17564029e0b4>

## Game Overview

> Play Othello with a powerful AI!

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/97e73583-2bbc-425f-bd02-2959341ea139"></img>

> There are 4 difficulties. Can you beat the Impossible?

<img style="width: 720px" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/ec1b010b-15ae-4664-9d21-90cd7bce5552"></img>

> Explore the environment around you!

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/24f8d6d8-5706-493c-8f05-d3706f067bc5"></img>

> Open the door and explore the inside of the buildings!

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/dd78bf62-984b-4679-8c67-66f3f756ad64"></img>

> This game is powered by Unity-chan, the official mascot of Unity3D

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/2dd2edde-36f9-4b68-86f1-8cb62cfa5951"></img>

## How to Play

- Move the player with `W`, `A`, `S`, and `D` keys on the keyboard
- Jump with `Space`
- Sprint with `Shift` holding
- Open Settings with `Q`
- Look around with the `mouse`
- Place a disk with `Left-click`
- Zoom in with `Right-click`

## How to Run (Option 1)

- Install [Docker](https://www.docker.com/get-started) and [Docker-compose](https://docs.docker.com/compose/install/)
- Run the following command:

```shell
docker-compose pull && docker-compose up
```

- When the `web-server` is running, visit [here](http://localhost:8080) to play!

## How to Build Docker Images and Run (Option 2)

This option builds the docker images and runs them with released Unity3D builds.

- Install [Docker](https://www.docker.com/get-started) and [Docker-compose](https://docs.docker.com/compose/install/)

- To run the app, use the following command from the folder where `docker-compose-dev.yml` is located:

```shell
docker-compose -f docker-compose-dev.yml up --build
```

- When both the `ai-server` and `web-server` are running, visit [here](http://localhost:8080) to play!

## How to Build Everything and Run (Option 3)

- Install [Rust](https://www.rust-lang.org/tools/install), [Node.js](https://nodejs.org/en) and [Unity3D](https://unity.com/download)

- Change the directory into the folder `ai-server` and run the following commands:

```shell
cargo run
```

- In a separate terminal, change the directory into the folder `web-server` and run the following commands:

```shell
npm install
npm run build-unity
npm run dev
```

- When both the `ai-server` and `web-server` are running, visit [here](http://localhost:8080) to play!

## Architecture

![Desdemona](https://github.com/SeoulSKY/Desdemona/assets/48105703/2825305b-203c-4285-920d-765333ffe7fa)
