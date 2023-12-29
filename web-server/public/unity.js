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

    if (window.location.hostname === "localhost") {
        setupUnityWebGL({
            "loaderUrl": "Build/WebGL.loader.js",
            "dataUrl": "Build/WebGL.data.unityweb",
            "frameworkUrl": "Build/WebGL.framework.js.unityweb",
            "codeUrl": "Build/WebGL.wasm.unityweb",
            "productVersion": "v0.0.0",
        });
        return;
    }

    fetch("https://api.github.com/repos/seoulsky/desdemona/releases/latest")
        .then(response => {
            setupUnityWebGL({
                "loaderUrl": response["assets"].filter(asset => asset["name"] === "WebGL.loader.js")[0]["browser_download_url"],
                "dataUrl": response["assets"].filter(asset => asset["name"] === "WebGL.data.unityweb")[0]["browser_download_url"],
                "frameworkUrl": response["assets"].filter(asset => asset["name"] === "WebGL.framework.js.unityweb")[0]["browser_download_url"],
                "codeUrl": response["assets"].filter(asset => asset["name"] === "WebGL.wasm.unityweb")[0]["browser_download_url"],
                "productVersion": response["tag_name"],
            });
        }).catch(error => {
            alert("Failed to load the game. Please try again later.");
            console.error(error);
        });
}

function setupUnityWebGL(data) {
    let unity = new UnityWebgl("#unity-canvas", {
        loaderUrl: data["loaderUrl"],
        dataUrl: data["dataUrl"],
        frameworkUrl: data["frameworkUrl"],
        codeUrl: data["codeUrl"],
        streamingAssetsUrl: "StreamingAssets",
        companyName: "SeoulSKY",
        productName: "Desdemona",
        productVersion: data["productVersion"],
    });

    let progressBar = document.getElementById("progress-bar");

    unity.on("progress", (progression) => {
        progressBar.style.width = 100 * progression + "%";
    });

    unity.on("mounted", () => {
        let spinner = document.getElementById("spinner");
        spinner.style.display = "none";

        progressBar.style.display = "none";

        let canvas = document.getElementById("unity-canvas");
        canvas.style.display = "block";
    });
}


main()
