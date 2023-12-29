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

    fetch("https://api.github.com/repos/seoulsky/desdemona/releases/latest")
        .then(response => {
            let unity = new UnityWebgl("#unity-canvas", {
                loaderUrl: response["assets"].filter(asset => asset["name"] === "WebGL.loader.js")[0]["browser_download_url"],
                dataUrl: response["assets"].filter(asset => asset["name"] === "WebGL.data.unityweb")[0]["browser_download_url"],
                frameworkUrl: response["assets"].filter(asset => asset["name"] === "WebGL.framework.js.unityweb")[0]["browser_download_url"],
                codeUrl: response["assets"].filter(asset => asset["name"] === "WebGL.wasm.unityweb")[0]["browser_download_url"],
                streamingAssetsUrl: "StreamingAssets",
                companyName: "SeoulSKY",
                productName: "Desdemona",
                productVersion: response["tag_name"],
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

        }).catch(error => {
            alert("Failed to load the game. Please try again later.");
            console.error(error);
        });
}


main()
