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