"use strict";

import { config } from "./config.js";

async function send(url, options = {}) {

    const response =
        await fetch(
            `${config.apiBaseUrl}${url}`,
            options);

    let body = null;

    const contentType =
        response.headers.get("content-type");

    if (contentType?.includes("application/json")) {

        body =
            await response.json();

    }

    if (!response.ok) {

        throw new Error(

            body?.message ??

            `HTTP ${response.status}`

        );

    }

    return body;

}

export async function createListing(listing) {

    return await send(

        "/listings",

        {

            method: "POST",

            headers: {

                "Content-Type":
                    "application/json"

            },

            body:
                JSON.stringify(listing)

        });

}

export async function getListings() {

    return await send("/listings");

}

export async function getListing(id) {

    return await send(

        `/listings/${id}`

    );
    
}

export async function createImageUpload(file) {
    return await send(
        "/images/uploads",
        {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                fileName: file.name,
                contentType: file.type,
                size: file.size
            })
        });
}

export async function uploadImageToBlob(
    uploadUrl,
    file
) {
    const response = await fetch(
        uploadUrl,
        {
            method: "PUT",
            headers: {
                "x-ms-blob-type": "BlockBlob",
                "Content-Type": file.type
            },
            body: file
        });

    if (!response.ok) {
        throw new Error(
            `Billedupload fejlede med HTTP ${response.status}.`
        );
    }
}

export async function completeImageUpload(imageId) {
    return await send(
        `/images/${encodeURIComponent(imageId)}/complete`,
        {
            method: "POST"
        });
}

export async function uploadImage(file) {
    const upload = await createImageUpload(file);

    await uploadImageToBlob(
        upload.uploadUrl,
        file
    );

    await completeImageUpload(
        upload.imageId
    );

    return upload.imageId;
}
