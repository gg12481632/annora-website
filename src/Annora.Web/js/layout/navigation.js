"use strict";

/**
 * Marks the navigation link corresponding to body[data-nav].
 *
 * @returns {void}
 */
export function highlightCurrentNavigation() {
    const navigationName =
        document.body.dataset.nav;

    if (!navigationName) {
        return;
    }

    const navigationLink =
        document.querySelector(
            `[data-nav="${navigationName}"]`
        );

    if (!navigationLink) {
        return;
    }

    navigationLink.setAttribute(
        "aria-current",
        "page"
    );
}