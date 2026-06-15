Upgrade plan — .NET Framework 4.8 -> .NET 6.0

Goal
- Convert project to .NET 6 (SDK-style) so we can use modern APIs and fix ZATCA invoice-reporting issues.
- Ensure ZATCA compliance: correct invoice hashing, canonicalization (C14N11), XAdES SignedProperties digest and SignedInfo signing, and VAT rounding rules.

Phases
1) Preparation
  - Create branch: `upgrade-to-net6`.
  - Commit/backup current work.
  - Ensure CI uses .NET 6 SDK image (update pipeline later).

2) Minimal hotfix (high-priority, no project conversion required)
  - Fix invoice-hash mismatch (already applied in `AdminBusiness.SaveSalesDetails`): compute invoiceHash = Base64(SHA256(UTF8Bytes(base64SignedXml))). Use `new UTF8Encoding(false)`.
  - Add deterministic logging of `xml`, `signedXml`, `base64SignedXml`, `invoiceHash`, invoice digest and signedProperties digest to a debug file.
  - Add unit tests to validate these computations locally.

3) ZATCA signing correctness (medium priority)
  - Implement deterministic canonicalization (C14N11) for:
    - invoice digest (invoice without UBLExtensions, without cac:Signature, and without QR AdditionalDocumentReference)
    - SignedProperties digest
    - SignedInfo canonicalization (C14N11) prior to ECDSA signing
  - Ensure SignedInfo contains:
    - Reference for invoice (with transforms shown in ZATCA sample)
    - Reference for SignedProperties (URI="#xadesSignedProperties")
  - Sign canonicalized SignedInfo bytes using ECDSA-SHA256 and embed Base64 signature into `ds:SignatureValue`.
  - Include certificate DER Base64 in `ds:X509Certificate`.

4) VAT/Tax calculations (medium priority)
  - Compute each `cac:TaxSubtotal/cbc:TaxAmount` as Round(taxableAmount * rate/100, 2, MidpointRounding.AwayFromZero).
  - Ensure `TaxTotal` equals sum of `TaxSubtotal` amounts.
  - Format numbers with two decimals where required and percentages with two decimals ("15.00").

5) Project conversion to .NET 6 (larger scope)
  - Convert .csproj to SDK-style and set <TargetFramework>net6.0</TargetFramework>.
  - Update/remove System.Web / WebForms references. Plan for separate migration of ASP.NET WebForms UI to ASP.NET Core or keep server-side services in .NET 6 separated from UI layer.
  - Upgrade NuGet packages to net6-compatible releases (Newtonsoft.Json, BouncyCastle updated package `Portable.BouncyCastle` or `BouncyCastle.NetCore`).
  - Add `System.Security.Cryptography.Xml` package if needed for canonicalization and transforms.
  - Update code for obsolete APIs and run full build.

6) Tests and CI
  - Add unit tests for digest/hash/signature flows (strict equality with independent computation).
  - Add an integration test that posts to the ZATCA compliance endpoint (using compliance CSID) and validates response JSON for no `invalid-invoice-hash`.
  - Update CI pipeline to use .NET 6 SDK image and run tests.

Files to change (minimal set)
- `EsalesApi\Models\AdminBusiness.cs` (ZATCA fixes: invoice hashing, canonicalization usage, SignXml improvements, tax rounding)
- `EsalesApi.csproj` (convert to SDK-style and set net6.0) — optional if you want immediate full migration
- `tests\Zatca.Tests.cs` (new unit tests for digest/hash/signature)
- CI pipeline file (e.g., `.github\workflows\ci.yml`) to use .NET 6 (after conversion)

Validation checklist (pre-PR)
- All unit tests pass locally.
- For a produced invoice: logged `base64SignedXml` and `invoiceHash` — `invoiceHash` == Base64(SHA256(UTF8Bytes(base64SignedXml))).
- Signed XML: embedded invoice digest equals local computed canonicalized invoice digest.
- ZATCA compliance call returns no `invalid-invoice-hash` error and returns XSD validation PASS.

Risks
- Full WebForms migration is non-trivial; consider keeping backend service layer on .NET 6 and migrating UI separately.
- Canonicalization differences (C14N11) between environments may cause mismatch — include binary tests comparing bytes.

Next deliverable (Execution stage)
- I will provide exact code patches for the minimal ZATCA fixes (signedXml base64 handling, invoiceHash computation, canonicalization and signing updates) and a unit test file. After you review, I will apply project file changes to target .NET 6.

Request: confirm you want me to generate the code patches for the ZATCA fixes now.