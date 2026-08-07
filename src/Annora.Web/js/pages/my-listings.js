"use strict";

import { config } from "../config.js";

const container =
    document.getElementById("listings-container");

const statusElement =
    document.getElementById("listings-status");

const template =
    document.getElementById("listing-template");

document.addEventListener(
    "DOMContentLoaded",
    initialize
);

async function initialize() {
    try {
        const listings =
            await getMyListings();

        renderListings(listings);
    }
    catch (error) {
        console.error(
            "Kunne ikke hente brugerens annoncer.",
            error
        );

        renderError(
            "Dine annoncer kunne ikke hentes."
        );
    }
}

async function getMyListings() {
    const response = await fetch(
        `${config.apiBaseUrl}/my/listings`,
        {
            method: "GET",
            headers: {
                "Accept": "application/json"
            },
            cache: "no-store"
        }
    );

    if (response.status === 401) {
        window.location.href = "/login";
        return [];
    }

    if (!response.ok) {
        throw new Error(
            `API'et returnerede HTTP ${response.status}.`
        );
    }

    return await response.json();
}

function renderListings(listings) {
    container.innerHTML = "";

    if (!Array.isArray(listings) ||
        listings.length === 0) {
        statusElement.textContent = "";

        container.innerHTML = `
            <div class="empty-state">
                <h2>Du har endnu ingen annoncer</h2>

                <p>
                    Opret din første annonce og få
                    den vist på Annora.
                </p>

                <a
                    class="button"
                    href="create.html"
                >
                    Opret annonce
                </a>
            </div>
        `;

        return;
    }

    statusElement.textContent =
        `${listings.length} annonce` +
        `${listings.length === 1 ? "" : "r"}`;

    for (const listing of listings) {
        const fragment =
            template.content.cloneNode(true);

        populateListing(
            fragment,
            listing
        );

        container.appendChild(fragment);
    }
}

function populateListing(
    fragment,
    listing
) {
    const link =
        fragment.querySelector(".listing-link");

    const category =
        fragment.querySelector(
            ".listing-category"
        );

    const date =
        fragment.querySelector(
            ".listing-date"
        );

    const title =
        fragment.querySelector(
            ".listing-title"
        );

    const description =
        fragment.querySelector(
            ".listing-description"
        );

    const price =
        fragment.querySelector(
            ".listing-price"
        );

    const location =
        fragment.querySelector(
            ".listing-location"
        );

    const image =
        fragment.querySelector(
            ".listing-image"
        );

    const placeholder =
        fragment.querySelector(
            ".listing-image-placeholder"
        );

    link.href =
        `listing.html?id=${encodeURIComponent(
            listing.id
        )}`;

    category.textContent =
        getCategoryLabel(
            listing.category
        );

    date.dateTime =
        listing.createdAt;

    date.textContent =
        formatDate(
            listing.createdAt
        );

    title.textContent =
        listing.title;

    description.textContent =
        listing.description;

    price.textContent =
        formatPrice(
            listing.price
        );

    location.textContent =
        `${listing.postalCode} ${listing.city}`;

    configureImage(
        image,
        placeholder,
        listing
    );
}

function configureImage(
    image,
    placeholder,
    listing
) {
    if (!listing.primaryImageId) {
        image.hidden = true;
        placeholder.hidden = false;
        return;
    }

    image.src =
        `${config.apiBaseUrl}/images/` +
        encodeURIComponent(
            listing.primaryImageId
        );

    image.alt =
        listing.title;

    image.hidden = false;
    placeholder.hidden = true;

    image.addEventListener(
        "error",
        () => {
            image.hidden = true;
            placeholder.hidden = false;
        }
    );
}

function formatPrice(price) {
    return new Intl.NumberFormat(
        "da-DK",
        {
            style: "currency",
            currency: "DKK",
            maximumFractionDigits: 0
        }
    ).format(price);
}

function formatDate(value) {
    return new Intl.DateTimeFormat(
        "da-DK",
        {
            day: "2-digit",
            month: "2-digit",
            year: "numeric"
        }
    ).format(
        new Date(value)
    );
}

function getCategoryLabel(category) {
    const labels = {
        furniture: "Møbler",
        electronics: "Elektronik",
        clothing: "Tøj",
        tools: "Værktøj",
        other: "Andet"
    };

    return labels[category] ?? category;
}

function renderError(message) {
    statusElement.textContent = "";

    container.innerHTML = `
        <div class="error-state">
            <h2>Der opstod en fejl</h2>
            <p>${escapeHtml(message)}</p>
        </div>
    `;
}

function escapeHtml(value) {
    const element =
        document.createElement("div");

    element.textContent = value;

    return element.innerHTML;
}