"use strict";

const isLocal =
    window.location.hostname === "localhost" ||
    window.location.hostname === "127.0.0.1";

export const config = {
    apiBaseUrl: isLocal
        ? "http://localhost:7071/api"
        : "/api"
};