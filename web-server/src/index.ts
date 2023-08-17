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
})

export const PROJECT_ROOT_PATH = path.dirname(require.main?.path as string);

const app = express();
app.use(compress());
app.use(express.static("public"))

import {build} from "./unityBuilder";

(async () => {
    if (!fs.existsSync(path.join("public", "Build"))) {
        try {
            await build()
        } catch (e) {
            logger.error(e);
            process.exit(1);
        }
    }

    app.listen(PORT, () => {
        logger.info(`Server is running on port ${PORT}`);
    });
})();
