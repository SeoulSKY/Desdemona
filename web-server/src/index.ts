import express from "express";
import compression from "compression";
import pino from "pino";
import path from "path";
import fs from "fs";
import {duration} from "moment";

const AI_SERVER_HOST = "http://ai-server:8000/api"
const HOST = "0.0.0.0";
const PORT = 8080;
const RETRY_INTERVAL = duration({second: 5});

export const logger = pino({
    transport: {
        target: "pino-pretty"
    },
})

export const PROJECT_ROOT_PATH = path.dirname(require.main?.path as string);


const app = express();

app.use(compression());

app.use(express.static("public"));

import {build} from "./buildUnity";

if (require.main === module) {
    (async () => {
        if (!fs.existsSync(path.join("public", "Build"))) {
            if (process.env.DOCKER) {
                logger.error("Build not found. Run 'npm run build-unity' in your local machine with unity installed");
                process.exit(1);
            } else {
                await build();
            }
        }

        while (true) {
            try {
                await fetch(AI_SERVER_HOST, { method: "GET" });
                break;
            } catch (e) {
                logger.debug(`Couldn't get a response from ${AI_SERVER_HOST}. Retrying in ${RETRY_INTERVAL.asSeconds()} seconds...`)
                await new Promise(r => setTimeout(r, RETRY_INTERVAL.asMilliseconds()));
            }
        }
      
        app.listen(PORT, HOST, () => {
          logger.info("Web server is running");
        });
    })();
}
