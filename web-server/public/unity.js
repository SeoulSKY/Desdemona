function main() {
    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
        const meta = document.createElement("meta");
        meta.name = "viewport";
        meta.content = "width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes";
        document.getElementsByTagName("head")[0].appendChild(meta);
    }

    const canvas = document.querySelector("#unity-canvas");
    if (canvas == null) {
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
        productVersion: "0.1.0",
    });

    unity.on("mounted", () => {
        let spinner = document.getElementById("spinner");
        spinner.style.display = "none";

        let canvas = document.getElementById("unity-canvas");
        canvas.style.display = "block";
    });
}


main()
