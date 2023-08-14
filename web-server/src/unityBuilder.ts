import {logger, PROJECT_ROOT_PATH} from "./index";
import { exec } from "child_process";
import path from "path";

const UNITY_PROJECT_PATH = path.join(PROJECT_ROOT_PATH, "..", "othello");
const BUILD_PATH = path.join(UNITY_PROJECT_PATH, "Builds", "WebGL", "buildDefault");
const UNITY_VERSION = "2022.3.7f1";


function execAsync(command: string) {
    return new Promise((resolve, reject) => {
        exec(command,(error, stdout) => {
            if (error) {
                reject(error);
            } else {
                resolve(stdout);
            }
        });
    });
}

export async function build() {
    let unityPath;
    if (process.platform === "win32") {
        unityPath = `C:\\Program Files\\Unity\\Hub\\Editor\\${UNITY_VERSION}\\Editor\\Unity.exe`
    } else if (process.platform === "darwin") {
        unityPath = `/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity`
    } else if (process.platform === "linux") {
        unityPath = `/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/Linux/Unity`
    } else {
        throw new Error("Unknown OS platform");
    }

    logger.info("Building Unity project...")
    await execAsync(`${unityPath} -projectPath ${UNITY_PROJECT_PATH} -logFile ${path.join("logs", "build.log")} -executeMethod UBuilder.Command.Build -quit -batchmode -nographics`);

    logger.info("Moving the Build in this server...")
    if (process.platform === "win32") {
        await execAsync(`Move-Item -Path ${path.join(BUILD_PATH, "Build")} -Destination ${path.join(PROJECT_ROOT_PATH, "public")}`)
    } else {
        await execAsync(`mv ${path.join(BUILD_PATH, "Build")} ${path.join(PROJECT_ROOT_PATH, "public")}`)
    }

    logger.info("Cleaning the remaining files...")
    if (process.platform === "win32") {
        await execAsync(`Remove-Item -Path ${path.join(UNITY_PROJECT_PATH, "Builds")} -Recurse`)
    } else {
        await execAsync(`rm -rf ${path.join(UNITY_PROJECT_PATH, "Builds")}`)
    }
}
