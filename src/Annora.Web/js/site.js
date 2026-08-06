"use strict";

import {
    loadLayout
} from "./layout/fragments.js";

import {
    highlightCurrentNavigation
} from "./layout/navigation.js";

import {
    configureAuthenticationNavigation
} from "./auth/authentication.js";

document.addEventListener(
    "DOMContentLoaded",
    initializeSite
);

/**
 * Initializes functionality shared by all Annora pages.
 *
 * @returns {Promise<void>}
 */
async function initializeSite() {
    try {
        await loadLayout();

        highlightCurrentNavigation();

        await configureAuthenticationNavigation();
    }
    catch (error) {
        console.error(
            "Annoras fælles sidelayout kunne ikke initialiseres.",
            error
        );
    }
}