import express from "express";
import compress from "compression";
import pino from "pino";
import path from "path";
import fs from "fs";


const PORT = 8080

export const logger = pino({
    transport: {
        target: "pino-pretty"
    },
}, pino.destination({sync: true}))

export const PROJECT_ROOT_PATH = path.dirname(require.main?.path as string);

const app = express();
app.use(compress());
app.use(express.static("public"))

import {build} from "./unityBuilder";

(async () => {
    if (!fs.existsSync(path.join("public", "Build"))) {
        await build()
    }

    app.listen(PORT, () => {
        logger.info(`Server is running on port ${PORT}`);
    });
})();
