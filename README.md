# Desdemona

> Powerful Othello AI developed with Rust, Unity3D, WebGL, and Node.js
> 
> Can You Beat Impossible Difficulty?

## Game Overview

> Play Othello with a powerful AI!

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/97e73583-2bbc-425f-bd02-2959341ea139"></img>

> Explore the environment around you!

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/24f8d6d8-5706-493c-8f05-d3706f067bc5"></img>

> Open the door and explore the inside of the buildings!

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/dd78bf62-984b-4679-8c67-66f3f756ad64"></img>

> This game is powered by Unity-chan, the official mascot of Unity3D

<img style="width: 720px;" src="https://github.com/SeoulSKY/Desdemona/assets/48105703/2dd2edde-36f9-4b68-86f1-8cb62cfa5951"></img>



## How to Play
* Move the player with `W`, `A`, `S`, and `D` keys on the keyboard
* Jump with `Space`
* Sprint with `Shift` holding
* Open Settings with `Q`
* Look around with the `mouse`
* Place a disk with `Left-click`
* Zoom in with `Right-click`

## How to Run (Option 1)

* Install [Docker](https://www.docker.com/get-started) and [Docker-compose](https://docs.docker.com/compose/install/)
* Run the following command:
```
docker-compose up --build
```
* When the `web-server` is running, visit [here](HTTP://localhost:8080) to play!

## How to Build and Run (Option 2)

* Install [Docker](https://www.docker.com/get-started),  [Docker-compose](https://docs.docker.com/compose/install/), [Node.js](https://nodejs.org/en) and [Unity3D](https://unity.com/download)
* Open the folder `desdemona` in Unity3D and close it. This will create the necessary files to build.
* Change the directory into the folder `web-server` and run the following commands:
```
npm install
npm run build-unity
```
* To run the app, use the following command from the folder where `docker-compose-dev.yml` is located:
```
docker-compose -f docker-compose-dev.yml up --build
```
* When the `web-server` is running, visit [here](http://localhost:8080) to play!
