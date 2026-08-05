# ADR-004: Billedarkitektur

## Status

Accepteret

## Dato

6. august 2026

## Kontekst

Annora skal understøtte billeder til annoncer. Billeder er større end de øvrige annoncedata og har en anden livscyklus end selve annoncen.

Løsningen skal:

* holde Blob Storage-containeren privat;
* undgå at eksponere Storage Account-nøgler til browseren;
* understøtte upload, før annoncen er færdigskrevet;
* gøre det muligt senere at generere thumbnails;
* kunne rydde ubrugte billeder op;
* holde Azure-omkostningerne meget lave;
* fungere med Azure Static Web Apps og den eksisterende Functions-backend.

## Beslutning

Billeder modelleres som selvstændige ressourcer med deres eget `ImageId`.

Browseren anmoder først Annora API’et om en kortlivet upload-URL. API’et:

1. validerer filtype og størrelse;
2. opretter et nyt `ImageId`;
3. genererer et internt blob-navn;
4. gemmer billedmetadata med status `Pending`;
5. returnerer en kortlivet, begrænset SAS-URL.

Browseren uploader derefter billedfilen direkte til Azure Blob Storage.

Når uploaden er afsluttet, kalder browseren et completion-endpoint. API’et kontrollerer, at blobben eksisterer, og ændrer billedets status til `Uploaded`.

Når annoncen oprettes, sendes `PrimaryImageId` sammen med annoncedataene. API’et kontrollerer, at billedet eksisterer og har status `Uploaded`, knytter billedet til annoncen og ændrer status til `Attached`.

## API

### Opret upload

```http
POST /api/images/uploads
```

Request:

```json
{
  "fileName": "stol.jpg",
  "contentType": "image/jpeg",
  "size": 1842350
}
```

Response:

```json
{
  "imageId": "9f595122-b2e6-4ac2-9325-f7a32733117c",
  "uploadUrl": "https://storage/...sas...",
  "expiresAt": "2026-08-06T00:15:00Z"
}
```

### Bekræft upload

```http
POST /api/images/{imageId}/complete
```

### Hent billede

```http
GET /api/images/{imageId}
```

Endpointet kan i første version redirecte til en kortlivet read-SAS-URL. Blob-containeren forbliver privat.

### Opret annonce

```http
POST /api/listings
```

Requesten kan indeholde:

```json
{
  "primaryImageId": "9f595122-b2e6-4ac2-9325-f7a32733117c"
}
```

## Storage

### Blob-container

```text
images
```

Blob-navn:

```text
originals/{imageId}/original
```

Det oprindelige filnavn gemmes kun som metadata og anvendes ikke som blob-navn.

### Images-tabel

```text
PartitionKey = image
RowKey       = imageId uden bindestreger
```

Felter:

```text
OriginalFileName
BlobName
ContentType
Size
Status
UploadedAt
AttachedAt
ListingId
```

Mulige statusværdier:

```text
Pending
Uploaded
Attached
Deleted
Failed
```

## Begrænsninger i første version

* Ét hovedbillede pr. annonce.
* Maksimal filstørrelse: 5 MB.
* Tilladte typer: JPEG, PNG og WebP.
* Ingen thumbnail-generering.
* Ingen AI-analyse.
* Ingen virus- eller indholdsmoderation.
* Ingen automatisk oprydning i første implementation.

## Fremtidige udvidelser

En senere Azure Function kan reagere på Blob Storage-events og:

* kontrollere billedets faktiske filformat;
* fjerne metadata;
* normalisere orientering;
* reducere opløsning;
* generere thumbnails;
* beregne hash;
* udføre moderation.

Et periodisk oprydningsjob kan slette billeder med status `Pending` eller `Uploaded`, som ikke er blevet knyttet til en annonce inden for en fastsat tidsperiode.

## Konsekvenser

### Fordele

* Billeddata passerer ikke gennem Annora API’et.
* Storage credentials eksponeres ikke til browseren.
* Upload kan ske, før annoncen oprettes.
* Genforsøg kan udføres uden at genindsende annoncedata.
* Billedbehandling kan tilføjes asynkront senere.
* Blob-containeren kan forblive privat.

### Ulemper

* Uploadprocessen består af flere HTTP-kald.
* Uafsluttede uploads kræver senere oprydning.
* Metadata og Blob Storage kan midlertidigt være ude af synkronisering.
* SAS-generering og CORS på Storage Accounten skal konfigureres korrekt.

## Alternativer

### Upload gennem Azure Function

Afvist som primær løsning, fordi API’et ellers skal modtage og videresende hele billedfilen.

### Offentlig Blob-container

Afvist, fordi billeder ikke bør være offentligt tilgængelige uden Annoras kontrol.

### Gem billedet direkte på annoncen

Afvist, fordi billeder og annoncer har forskellige størrelser, egenskaber og livscyklusser.

### Event Grid og thumbnail-generator fra første version

Udskudt for at holde den første implementation enkel og billig.
