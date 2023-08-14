import express from "express";
import compress from "compression";
import pino from "pino";
import dotenv from "dotenv";
import path from "path";
import fs from "fs";


dotenv.config();

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
if (!fs.existsSync(path.join("public", "Build"))) {
    build()
}

const PORT = 8080
app.listen(PORT, () => {
    logger.info(`Server is running on port ${PORT}`);
});
