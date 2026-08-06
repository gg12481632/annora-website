"use strict";

/**
 * Loads the shared header and footer fragments.
 *
 * @returns {Promise<void>}
 */
export async function loadLayout() {
    await Promise.all([
        loadFragment(
            "site-header",
            "fragments/header.html"
        ),
        loadFragment(
            "site-footer",
            "fragments/footer.html"
        )
    ]);

    setCurrentYear();
}

/**
 * Loads an HTML fragment into an existing element.
 *
 * @param {string} elementId
 * @param {string} fragmentUrl
 * @returns {Promise<void>}
 */
async function loadFragment(elementId, fragmentUrl) {
    const target = document.getElementById(elementId);

    if (!target) {
        return;
    }

    const response = await fetch(fragmentUrl, {
        cache: "no-cache"
    });

    if (!response.ok) {
        throw new Error(
            `Kunne ikke hente '${fragmentUrl}'. ` +
            `HTTP ${response.status}.`
        );
    }

    target.innerHTML = await response.text();
}

/**
 * Inserts the current year into the shared footer.
 *
 * @returns {void}
 */
function setCurrentYear() {
    const yearElement = document.getElementById("year");

    if (!yearElement) {
        return;
    }

    yearElement.textContent =
        new Date().getFullYear().toString();
}