import express from "express";
import pino from "pino";
import path from "path";
import fs from "fs";
import { duration } from "moment";
import { createProxyMiddleware } from "http-proxy-middleware";


const IS_PRODUCTION = process.env.PRODUCTION !== undefined;
const AI_SERVER_HOST = IS_PRODUCTION ? "http://ai-server:8000/api" : "http://localhost:8000/api";
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

app.use(express.static("public"));

if (require.main === module) {
    (async () => {
        if (!IS_PRODUCTION && !fs.existsSync(path.join(PROJECT_ROOT_PATH, "public", "Build"))) {
            logger.error("Unity build not found. Run 'npm run build-unity' first.");
            process.exit(1);
        }

        if (IS_PRODUCTION) {
            let response = await fetch(
                "https://api.github.com/repos/seoulsky/desdemona/releases/latest"
            );
            if (!response.ok) {
                logger.error("Failed to get latest release info");
                logger.error(await response.text());
                process.exit(1);
            }

            let json = await response.json();

            app.use(
                "/Build",
                createProxyMiddleware({
                    target: `https://github.com/SeoulSKY/Desdemona/releases/download/${json.tag_name}`,
                    changeOrigin: true,
                    headers: {
                        "Access-Control-Allow-Origin": "*",
                    },
                    pathRewrite: {
                        "^/Build": "",
                    },
                })
            );

            while (true) {
                try {
                    await fetch(AI_SERVER_HOST);
                    break;
                } catch (e) {
                    logger.debug(
                    `Couldn't get a response from ${AI_SERVER_HOST}. Retrying in ${RETRY_INTERVAL.asSeconds()} seconds...`
                    );
                    await new Promise((r) => setTimeout(r, RETRY_INTERVAL.asMilliseconds()));
                }
            }
        }

        app.listen(PORT, HOST, () => {
          logger.info("Web server is listening http://%s:%s", HOST, PORT);
        });
    })();
}
