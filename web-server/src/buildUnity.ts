import {logger, PROJECT_ROOT_PATH} from "./index";
import { exec } from "child_process";
import path from "path";
import moment, {duration} from "moment";

const UNITY_PROJECT_PATH = path.join(PROJECT_ROOT_PATH, "..", "desdemona");
const BUILD_PATH = path.join(UNITY_PROJECT_PATH, "Builds", "WebGL", "buildDefault");
const UNITY_VERSION = "2022.3.7f1";


function execAsync(command: string) {
    return new Promise((resolve, reject) => {
        exec(command,(error, stdout, stderr) => {
            if (error) {
                reject(error);
            } else {
                resolve(stdout ?? stderr);
            }
        });
    });
}

export async function build() {
    let unityPath;
    if (process.env.UNITY_PATH) {
        unityPath = process.env.UNITY_PATH;
    } else if (process.platform === "win32") {
        unityPath = `C:\\Program Files\\Unity\\Hub\\Editor\\${UNITY_VERSION}\\Editor\\Unity.exe`
    } else if (process.platform === "darwin") {
        unityPath = `/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity`
    } else if (process.platform === "linux") {
        unityPath = `/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/Linux/Unity`
    } else {
        throw new Error("Unknown OS platform");
    }

    logger.info("Building Unity project...")
    let start = moment();
    await execAsync(`${unityPath} -projectPath ${UNITY_PROJECT_PATH} -logFile ${path.join("logs", "build.log")} -executeMethod UBuilder.Command.Build -quit -batchmode -nographics`);
    let end = moment();
    logger.info(`Finished building in ${duration(end.diff(start)).humanize()}`)

    logger.info("Moving the Build in this server...")
    start = moment();
    let dest = path.join(PROJECT_ROOT_PATH, "public", "Build");
    if (process.platform === "win32") {
        await execAsync(`Move-Item -Path ${path.join(BUILD_PATH, "Build")} -Destination ${dest} -Force`)
    } else {
        await execAsync(`rm -rf ${dest} && mv ${path.join(BUILD_PATH, "Build")} ${dest}`)
    }
    end = moment()
    logger.info(`Finished moving in ${duration(end.diff(start)).humanize()}`)

    logger.info("Cleaning the remaining files...")
    start = moment()
    if (process.platform === "win32") {
        await execAsync(`Remove-Item -Path ${path.join(UNITY_PROJECT_PATH, "Builds")} -Recurse`)
    } else {
        await execAsync(`rm -rf ${path.join(UNITY_PROJECT_PATH, "Builds")}`)
    }
    end = moment();
    logger.info(`Finished cleaning in ${duration(end.diff(start)).humanize()}`)
}

if (require.main === module) {
    (async () => {
        try {
            await build()
        } catch (e) {
            logger.error(e);
            process.exit(1);
        }
    })();
}
