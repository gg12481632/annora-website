"use strict";

const form = document.getElementById("listing-form");
const resultSection = document.getElementById("result-section");
const resultElement = document.getElementById("result");
const yearElement = document.getElementById("year");

if (yearElement) {
    yearElement.textContent = new Date().getFullYear().toString();
}

form.addEventListener("submit", (event) => {
    event.preventDefault();

    if (!form.reportValidity()) {
        return;
    }

    const formData = new FormData(form);
    const imageInput = document.getElementById("images");

    const listing = {
        title: formData.get("title"),
        category: formData.get("category"),
        description: formData.get("description"),
        price: Number(formData.get("price")),
        condition: formData.get("condition"),
        location: {
            postalCode: formData.get("postalCode"),
            city: formData.get("city")
        },
        seller: {
            email: formData.get("email")
        },
        images: Array.from(imageInput.files).map((file) => ({
            name: file.name,
            size: file.size,
            type: file.type
        })),
        createdAt: new Date().toISOString()
    };

    resultElement.textContent = JSON.stringify(listing, null, 2);
    resultSection.hidden = false;
    resultSection.scrollIntoView({ behavior: "smooth" });
});