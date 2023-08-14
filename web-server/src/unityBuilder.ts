import {logger, PROJECT_ROOT_PATH} from "./index";
import { execSync } from "child_process";
import path from "path";

const UNITY_PROJECT_PATH = path.join(PROJECT_ROOT_PATH, "..", "othello");
const BUILD_PATH = path.join(UNITY_PROJECT_PATH, "Builds", "WebGL", "buildDefault");

export function build() {
    logger.info("Building Unity project...")
    execSync(`${process.env.UNITY_PATH} -projectPath ${UNITY_PROJECT_PATH} -logFile ${path.join("logs", "build.log")} -executeMethod UBuilder.Command.Build -quit -batchmode -nographics`);

    logger.info("Moving the Build in this server...")
    execSync(`mv ${path.join(BUILD_PATH, "Build")} ${path.join(PROJECT_ROOT_PATH, "public")}`)

    logger.info("Cleaning the Build...")
    execSync(`rm -rf ${path.join(UNITY_PROJECT_PATH, "Builds")}`)
}

