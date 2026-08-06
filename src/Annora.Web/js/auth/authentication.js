"use strict";

/**
 * Updates the shared navigation according to the current
 * Static Web Apps authentication principal.
 *
 * @returns {Promise<void>}
 */
export async function configureAuthenticationNavigation() {
    const loginLink =
        document.getElementById("login-link");

    const accountLink =
        document.getElementById("account-link");

    const logoutLink =
        document.getElementById("logout-link");

    if (!loginLink || !accountLink || !logoutLink) {
        return;
    }

    try {
        const principal = await getClientPrincipal();
        const authenticated = principal !== null;

        loginLink.hidden = authenticated;
        accountLink.hidden = !authenticated;
        logoutLink.hidden = !authenticated;

        accountLink.textContent =
            authenticated && principal.userDetails
                ? `Mine annoncer (${principal.userDetails})`
                : "Mine annoncer";
    }
    catch (error) {
        console.error(
            "Loginstatus kunne ikke hentes.",
            error
        );

        loginLink.hidden = false;
        accountLink.hidden = true;
        logoutLink.hidden = true;
    }
}

/**
 * Returns the current Static Web Apps client principal.
 *
 * @returns {Promise<object|null>}
 */
async function getClientPrincipal() {
    const response = await fetch("/.auth/me", {
        cache: "no-store"
    });

    if (!response.ok) {
        throw new Error(
            "Authentication endpoint returned " +
            `HTTP ${response.status}.`
        );
    }

    const authentication = await response.json();

    return authentication.clientPrincipal ?? null;
}