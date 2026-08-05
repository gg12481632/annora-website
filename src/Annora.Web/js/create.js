"use strict";

const form = document.getElementById("listing-form");
const submitButton = form.querySelector('button[type="submit"]');

const resultSection = document.getElementById("result-section");
const resultHeading = document.getElementById("result-heading");
const resultMessage = document.getElementById("result-message");
const resultDetails = document.getElementById("result-details");

const apiBaseUrl = "http://localhost:7071/api";

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (!form.reportValidity()) {
        return;
    }

    const formData = new FormData(form);

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
        }
    };

    submitButton.disabled = true;
    submitButton.textContent = "Opretter annonce…";

    resultSection.hidden = false;
    resultHeading.textContent = "Opretter annonce";
    resultMessage.textContent = "Annoncen sendes til Annora.";
    resultDetails.textContent = "";

    try {
        const response = await fetch(`${apiBaseUrl}/listings`, {
            method: "POST",

            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify(listing)
        });

        const responseBody = await response.json();

        if (!response.ok) {
            throw new Error(
                responseBody.message ??
                `API'et returnerede HTTP ${response.status}.`
            );
        }

        resultHeading.textContent = "Annoncen er oprettet";
        resultMessage.textContent =
            "Din annonce blev gemt korrekt.";

        resultDetails.textContent =
            `Annonce-id: ${responseBody.id}\n` +
            `Titel: ${responseBody.title}\n` +
            `Status: ${responseBody.status}\n` +
            `Oprettet: ${responseBody.createdAt}`;

        form.reset();
    }
    catch (error) {
        console.error(error);

        resultHeading.textContent = "Annoncen kunne ikke oprettes";
        resultMessage.textContent = error.message;
        resultDetails.textContent = "";
    }
    finally {
        submitButton.disabled = false;
        submitButton.textContent = "Opret annonce";

        resultSection.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
    }
});
