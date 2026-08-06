"use strict";

import { config } from "./config.js";
import { getListing } from "./api.js";

const loadingState = document.getElementById("loading-state");
const errorState = document.getElementById("error-state");
const errorMessage = document.getElementById("error-message");
const listingContent = document.getElementById("listing-content");

const breadcrumbTitle =
    document.getElementById("breadcrumb-title");

const titleElement = document.getElementById("title");
const categoryElement = document.getElementById("category");
const createdAtElement = document.getElementById("created-at");
const priceElement = document.getElementById("price");
const conditionElement = document.getElementById("condition");
const locationElement = document.getElementById("location");
const descriptionElement = document.getElementById("description");
const mainImage =
    document.getElementById("main-image");

const mainImagePlaceholder =
    document.getElementById(
        "main-image-placeholder"
    );

async function loadListing() {
    const parameters = new URLSearchParams(window.location.search);
    const id = parameters.get("id");

    if (!id) {
        showError("Annonce-id mangler i adressen.");
        return;
    }

    try {
        const listing = await getListing(id);

        renderListing(listing);
    }
    catch (error) {
        console.error(error);
        showError(error.message);
    }
}

function renderListing(listing) {
    document.title = `${listing.title} – Annora`;

    breadcrumbTitle.textContent = listing.title;
    titleElement.textContent = listing.title;

    categoryElement.textContent =
        getCategoryLabel(listing.category);

    createdAtElement.dateTime = listing.createdAt;
    createdAtElement.textContent =
        formatDate(listing.createdAt);

    priceElement.textContent =
        new Intl.NumberFormat("da-DK", {
            style: "currency",
            currency: "DKK",
            maximumFractionDigits: 0
        }).format(listing.price);

    conditionElement.textContent =
        getConditionLabel(listing.condition);

    locationElement.textContent =
        `${listing.postalCode} ${listing.city}`;

    descriptionElement.textContent = listing.description;

    loadingState.hidden = true;
    errorState.hidden = true;
    listingContent.hidden = false;

    if (listing.primaryImageId) {
        mainImage.src =
            `${config.apiBaseUrl}/images/` +
            encodeURIComponent(listing.primaryImageId);

        mainImage.alt = listing.title;
        mainImage.hidden = false;
        mainImagePlaceholder.hidden = true;

        mainImage.addEventListener("error", () => {
            mainImage.hidden = true;
            mainImagePlaceholder.hidden = false;
        });
    }
}

function showError(message) {
    loadingState.hidden = true;
    listingContent.hidden = true;
    errorState.hidden = false;
    errorMessage.textContent = message;
}

function getCategoryLabel(category) {
    const categories = {
        furniture: "Møbler",
        electronics: "Elektronik",
        bicycles: "Cykler",
        clothing: "Tøj",
        garden: "Have",
        tools: "Værktøj",
        books: "Bøger",
        other: "Andet"
    };

    return categories[category] ?? category;
}

function getConditionLabel(condition) {
    const conditions = {
        new: "Som ny",
        good: "God",
        used: "Brugt",
        worn: "Slidt"
    };

    return conditions[condition] ?? condition;
}

function formatDate(value) {
    return new Intl.DateTimeFormat("da-DK", {
        day: "numeric",
        month: "long",
        year: "numeric"
    }).format(new Date(value));
}

loadListing();