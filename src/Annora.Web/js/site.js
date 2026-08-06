"use strict";

document.addEventListener("DOMContentLoaded", async () => {
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
    markCurrentNavigationItem();
    await configureAuthenticationNavigation();
});

async function loadFragment(elementId, fragmentUrl) {
    const target = document.getElementById(elementId);

    if (!target) {
        return;
    }

    try {
        const response = await fetch(fragmentUrl);

        if (!response.ok) {
            throw new Error(
                `Kunne ikke hente ${fragmentUrl}. HTTP ${response.status}.`
            );
        }

        target.innerHTML = await response.text();
    }
    catch (error) {
        console.error(
            `Kunne ikke indlæse fragmentet ${fragmentUrl}.`,
            error
        );
    }
}

function setCurrentYear() {
    const yearElement = document.getElementById("year");

    if (yearElement) {
        yearElement.textContent =
            new Date().getFullYear().toString();
    }
}

function markCurrentNavigationItem() {
    const currentPage =
        document.body.dataset.page;

    if (!currentPage) {
        return;
    }

    const currentLink = document.querySelector(
        `[data-page="${currentPage}"]`
    );

    if (currentLink) {
        currentLink.setAttribute(
            "aria-current",
            "page"
        );
    }
}

async function configureAuthenticationNavigation() {
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
        const response = await fetch("/.auth/me");

        if (!response.ok) {
            throw new Error(
                `Authentication endpoint returned HTTP ${response.status}.`
            );
        }

        const authentication =
            await response.json();

        const principal =
            authentication.clientPrincipal;

        const authenticated =
            principal !== null &&
            principal !== undefined;

        loginLink.hidden = authenticated;
        accountLink.hidden = !authenticated;
        logoutLink.hidden = !authenticated;

        if (
            authenticated &&
            principal.userDetails
        ) {
            accountLink.textContent =
                `Mine annoncer (${principal.userDetails})`;
        }
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