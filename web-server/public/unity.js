function main() {
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
    let spinnerText = document.getElementById("spinner-text");
    let progressBar = document.getElementById("progress-bar");

    // make them visible
    spinner.style.removeProperty("display");
    progressBar.style.removeProperty("display");

    unity.on("progress", (progression) => {
        const downloadPercentage = Math.round(100 * progression / 0.3);
        progressBar.style.width = downloadPercentage + "%";

        if (progression < 0.9) {
            spinnerText.innerText = `Downloading data... ${downloadPercentage}%`;
        } else {
            progressBar.style.width = "100%";
            spinnerText.innerText = "Loading...";
        }
    });

    unity.on("mounted", () => {
        spinner.style.display = "none";
        progressBar.style.display = "none";

        let canvas = document.getElementById("unity-canvas");
        canvas.style.display = "block"
        canvas.focus();
    });

    unity.on("error", (message) => {
        console.error(message);
        spinner.style.display = "none";
        progressBar.style.display = "none";

        let error = document.getElementById("error");
        error.innerText = "Unable to load the game. Please try again later.";
    });
}

main();
