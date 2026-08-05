"use strict";

const apiBaseUrl = "http://localhost:7071/api";

const container = document.getElementById("listings-container");
const template = document.getElementById("listing-template");
const statusMessage = document.getElementById("status-message");

const searchInput = document.getElementById("search");
const categorySelect = document.getElementById("category");
const sortSelect = document.getElementById("sort");

let listings = [];

async function loadListings() {
    statusMessage.textContent = "Henter annoncer…";

    try {
        const response = await fetch(`${apiBaseUrl}/listings`);

        if (!response.ok) {
            throw new Error(
                `API'et returnerede HTTP ${response.status}.`
            );
        }

        listings = await response.json();

        renderListings();
    }
    catch (error) {
        console.error(error);

        statusMessage.textContent = "";

        container.innerHTML = `
            <div class="error-state">
                <strong>Annoncerne kunne ikke hentes.</strong>
                <p>${escapeHtml(error.message)}</p>
            </div>
        `;
    }
}

function renderListings() {
    const searchText =
        searchInput.value.trim().toLowerCase();

    const selectedCategory = categorySelect.value;
    const selectedSort = sortSelect.value;

    let filteredListings = listings.filter((listing) => {
        const matchesSearch =
            !searchText ||
            listing.title.toLowerCase().includes(searchText) ||
            listing.description.toLowerCase().includes(searchText);

        const matchesCategory =
            !selectedCategory ||
            listing.category === selectedCategory;

        return matchesSearch && matchesCategory;
    });

    filteredListings = [...filteredListings].sort((left, right) => {
        if (selectedSort === "price-ascending") {
            return left.price - right.price;
        }

        if (selectedSort === "price-descending") {
            return right.price - left.price;
        }

        return new Date(right.createdAt) - new Date(left.createdAt);
    });

    container.innerHTML = "";

    statusMessage.textContent =
        `${filteredListings.length} annonce` +
        `${filteredListings.length === 1 ? "" : "r"}`;

    if (filteredListings.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <h2>Ingen annoncer fundet</h2>
                <p>Prøv at ændre søgningen eller kategorien.</p>
                <a class="button" href="create.html">
                    Opret den første annonce
                </a>
            </div>
        `;

        return;
    }

    for (const listing of filteredListings) {
        const fragment = template.content.cloneNode(true);

        const link = fragment.querySelector(".listing-link");
        const category =
            fragment.querySelector(".listing-category");
        const date = fragment.querySelector(".listing-date");
        const title = fragment.querySelector(".listing-title");
        const description =
            fragment.querySelector(".listing-description");
        const price = fragment.querySelector(".listing-price");
        const location =
            fragment.querySelector(".listing-location");

        link.href =
            `listing.html?id=${encodeURIComponent(listing.id)}`;

        category.textContent =
            getCategoryLabel(listing.category);

        date.dateTime = listing.createdAt;
        date.textContent = formatDate(listing.createdAt);

        title.textContent = listing.title;
        description.textContent = listing.description;

        price.textContent =
            new Intl.NumberFormat("da-DK", {
                style: "currency",
                currency: "DKK",
                maximumFractionDigits: 0
            }).format(listing.price);

        location.textContent =
            `${listing.postalCode} ${listing.city}`;

        container.appendChild(fragment);
    }
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

function formatDate(value) {
    return new Intl.DateTimeFormat("da-DK", {
        day: "numeric",
        month: "short",
        year: "numeric"
    }).format(new Date(value));
}

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value;
    return element.innerHTML;
}

searchInput.addEventListener("input", renderListings);
categorySelect.addEventListener("change", renderListings);
sortSelect.addEventListener("change", renderListings);

loadListings();