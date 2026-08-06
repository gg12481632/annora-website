import {
    createListing,
    uploadImage
} from "./api.js";
"use strict";

const form = document.getElementById("listing-form");
const submitButton = form.querySelector('button[type="submit"]');

const resultSection = document.getElementById("result-section");
const resultHeading = document.getElementById("result-heading");
const resultMessage = document.getElementById("result-message");
const resultDetails = document.getElementById("result-details");

const imageInput = document.getElementById("images");

const imagePreviewContainer =
    document.getElementById("image-preview-container");

const imagePreview =
    document.getElementById("image-preview");

const imageFileName =
    document.getElementById("image-file-name");

const imageUploadStatus =
    document.getElementById("image-upload-status");

const removeImageButton =
    document.getElementById("remove-image");

let selectedImage = null;
let previewUrl = null;

imageInput.addEventListener("change", () => {
    const file = imageInput.files[0] ?? null;

    setSelectedImage(file);
});

removeImageButton.addEventListener("click", () => {
    imageInput.value = "";
    setSelectedImage(null);
});

function setSelectedImage(file) {
    if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
        previewUrl = null;
    }

    if (file) {
        const allowedTypes = [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

        const maximumSize =
            5 * 1024 * 1024;

        if (!allowedTypes.includes(file.type)) {
            imageInput.value = "";

            showImageValidationError(
                "Kun JPEG, PNG og WebP understøttes."
            );

            return;
        }

        if (file.size > maximumSize) {
            imageInput.value = "";

            showImageValidationError(
                "Billedet må højst fylde 5 MB."
            );

            return;
        }
    }
    selectedImage = file;

    if (!file) {
        imagePreviewContainer.hidden = true;
        imagePreview.removeAttribute("src");
        imageFileName.textContent = "";
        imageUploadStatus.textContent = "";
        return;
    }

    previewUrl = URL.createObjectURL(file);

    imagePreview.src = previewUrl;
    imageFileName.textContent = file.name;
    imageUploadStatus.textContent =
        formatFileSize(file.size);

    imagePreviewContainer.hidden = false;
}

function formatFileSize(size) {
    const megabytes = size / 1024 / 1024;

    return `${megabytes.toFixed(1)} MB`;
}

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
        let primaryImageId = null;

        if (selectedImage) {
            imageUploadStatus.textContent =
                "Uploader billede…";

            primaryImageId =
                await uploadImage(selectedImage);

            imageUploadStatus.textContent =
                "Billedet er uploadet";
        }

        listing.primaryImageId = primaryImageId;

        const responseBody =
            await createListing(listing);

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

    function showImageValidationError(message) {
        selectedImage = null;
        imagePreviewContainer.hidden = false;
        imagePreview.removeAttribute("src");
        imageFileName.textContent = "Ugyldigt billede";
        imageUploadStatus.textContent = message;
    }
});
