async function main() {
    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
        alert("Mobile devices are not supported. Please use a desktop.");
        return;
    }

    const canvas = document.querySelector("#unity-canvas");
    if (canvas === null) {
        throw new Error("Cannot find canvas element");
    }

    canvas.style.width = "100%";
    canvas.style.height = "100%";
    canvas.style.position = "fixed";

    document.body.style.textAlign = "left";

    let unity = new UnityWebgl("#unity-canvas", {
        loaderUrl: "Build/WebGL.loader.js",
        dataUrl: "Build/WebGL.data.unityweb",
        frameworkUrl: "Build/WebGL.framework.js.unityweb",
        codeUrl: "Build/WebGL.wasm.unityweb",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "SeoulSKY",
        productName: "Desdemona",
    });

    let spinner = document.getElementById("spinner");
    let progressBar = document.getElementById("progress-bar");

    // make them visible
    spinner.style.removeProperty("display");
    progressBar.style.removeProperty("display");

    unity.on("progress", (progression) => {
        progressBar.style.width = 100 * progression + "%";
    });

    unity.on("mounted", () => {
        spinner.style.display = "none";
        progressBar.style.display = "none";

        document.getElementById("unity-canvas").style.display = "block";
    });

    unity.on("error", (message) => {
        spinner.style.display = "none";
        progressBar.style.display = "none";

        document.getElementById("error").innerText = message;
    });
}

try {
    main();
} catch (e) {
    console.error(e);
    alert("Unable to load the game. Please try again later.");
}
