# Isolated baseline texture reconstruction

Prepared from previously approved sources on 2026-11-12. Source code, baked world files, material settings and runtime pins were unchanged by recovery. Downloaded maps were copied without transformation.

## Verification scope

- Coast Sand 01: all three maps equal the SHA-256 and size in tracked `docs/assets/p1-starter-manifest.md`; current official API MD5 also matched.
- ambientCG: exact approved asset IDs and 2K-JPG selections, from official provider-versioned ZIPs; response size and provider SHA-1 verified, ZIP CRCs clean, observed archive and map SHA-256 retained.
- Historical ambientCG local-byte equality remains **UNVERIFIED**: the original ignored P2 inventory is unavailable. Its tracked expected SHA-256 is `391099f0febe9ce04daa9be8d7614ae32b90f89d6c25f12f57aeda9cdee681d0`. Do not report baseline asset identity PASS.
- Only Color, NormalGL, Roughness, AmbientOcclusion, and Displacement JPEGs were copied unchanged into ignored vendor-local. No .blend, .tres, or vendor code was imported/executed.

## Official packages

| Asset / variant | Versioned official delivery URL | Upload UTC | Bytes | Provider SHA-1 | Observed SHA-256 |
| --- | --- | --- | ---: | --- | --- |
| Rock063 / 2K-JPG | https://acg-download.struffelproductions.com/file/ambientCG-Web/download/Rock063_ueB0D6VU/Rock063_2K-JPG.zip | 2026-02-08T11:03:20.386000+00:00 | 14917993 | `be45ab62bd6a057470bc66ea2b27ed261c04caee` | `c510afdcf7ba0bddf8603ecebfd817d660e8ba5df2f2750e9e8faa884a5b88d2` |
| Gravel041 / 2K-JPG | https://acg-download.struffelproductions.com/file/ambientCG-Web/download/Gravel041_OdrMjFH2/Gravel041_2K-JPG.zip | 2025-10-31T09:37:49.913000+00:00 | 32000842 | `fa5a2ee06620791f5d90b49a7931edbc25fd5082` | `8d8363a2b5905d2a4b444a5294a462910b598233f5adb57964c905c418364944` |
| Ground060 / 2K-JPG | https://acg-download.struffelproductions.com/file/ambientCG-Web/download/Ground060_vnj2Y86b/Ground060_2K-JPG.zip | 2025-10-31T10:02:47.803000+00:00 | 19436729 | `9154b9cc609b567ea2fe6d937aebbc904655bc84` | `d68ae52cd6f6689e611868000c228f4c6fabbc100b5df2a76ee9b07c48fab15a` |

Original metadata and headers: `ambientcg-api.json`, `ambientcg-download-heads.json`. Full installed map hashes: `installed-ambientcg-provenance.json`. Coast Sand URLs and installed hashes: `coast-sand-recovery.json`.

All downloads remain ignored; runtime copies are under `game/assets/vendor-local/`. Official source listings: https://ambientcg.com/view?id=Ground060, https://ambientcg.com/view?id=Gravel041, https://ambientcg.com/view?id=Rock063, https://polyhaven.com/a/coast_sand_01. Licensing records: https://docs.ambientcg.com/license/ and https://polyhaven.com/license.

## Installed map SHA-256 values

| Map filename | SHA-256 |
| --- | --- |
| `coast_sand_01_Diffuse_2k.jpg` | `e1c37ea8b4282fda612b0584d864ca4eef2da28c2fa9f3930cad9209414eb979` |
| `coast_sand_01_nor_gl_2k.jpg` | `e5b1430c751727c8ab0332adfd8960c48342936146de258aa9b7736630cdbb62` |
| `coast_sand_01_Rough_2k.jpg` | `93caeb7332d1593702d97a59040ce178a27c9d705b30c55413e0d2507e09a762` |
| `Rock063_2K-JPG_Color.jpg` | `24555bec01af3e99a0a2ce8f479dfb9dcbeeb01371edabc215aba8358b0d5fc1` |
| `Rock063_2K-JPG_NormalGL.jpg` | `c23f2a9c417876a44827eab737c55f59fa52b52778d49029f71c49fed7fef930` |
| `Rock063_2K-JPG_Roughness.jpg` | `2036e910ae9c5b61c39a0f1b63da37fea09d756e3e0db397d8cc8463a9fd1ad3` |
| `Rock063_2K-JPG_AmbientOcclusion.jpg` | `7f3ba4e9c3b922c4a4a3d374b0888dfdbf94352b37814785f61fc594cae92f89` |
| `Rock063_2K-JPG_Displacement.jpg` | `f1931d351d3803d996093e276b213741fb961384e20a0c6e7658788ec4528b41` |
| `Gravel041_2K-JPG_Color.jpg` | `ec231785a636d2946bae749f3295b482fd16823fcb7c7c83272746b13f85c6cb` |
| `Gravel041_2K-JPG_NormalGL.jpg` | `24813a2497d4a234167274cffbd661556ab335f69fb7519e3306735c33e46fe7` |
| `Gravel041_2K-JPG_Roughness.jpg` | `2fd3b3b65ae0691d9e8d1b9ba1d603e9a8ed9e808fff3fcb9d87f0658bf07668` |
| `Gravel041_2K-JPG_AmbientOcclusion.jpg` | `444eb3079bfe6086744ba0aa4e4c50fcbab6574cf1d08fc06a816050d31c7a77` |
| `Gravel041_2K-JPG_Displacement.jpg` | `2d28094536a1d585500102b64d3da652bb0ef386ee01a488fc16559989e1e9eb` |
| `Ground060_2K-JPG_Color.jpg` | `8951fac5d176a34c6b1a64af912b00ce960f8fb9efd28ad712624c00ab802901` |
| `Ground060_2K-JPG_NormalGL.jpg` | `a676f58f46b2ce09b714f3b56c68a6eccff4d44f39e7f9f54c41cd12a77e801f` |
| `Ground060_2K-JPG_Roughness.jpg` | `d119ca6c6d54a8a95b7e838d46da7319b0a5020362ec3beb4ee449d2ddad95af` |
| `Ground060_2K-JPG_AmbientOcclusion.jpg` | `1821042313e56e24b2bcdc06c67473eeb79bac29b384d1719f2f7da1a9fcd31b` |
| `Ground060_2K-JPG_Displacement.jpg` | `46bcd175a7f6462a9af65a55be3680af18f0fe7fdd8d3750ec0bf8ecc659c46d` |

## Texture recovery verification (before Fish3 recovery)

Source remained clean at `e93ecf5e6bc2d00d6738fce9766d98f17585a4ca`. One reimport completed with exit 0 in 13.47 s using pinned Godot `4.7.2.stable.mono.official.ed1daf0bf`; full Fish build had already passed with pinned .NET SDK `10.0.403` (0 warnings, 0 errors). The import regenerated only the absent older transmission-probe UID sidecar; it was preserved outside the checkout, leaving tracked source unchanged.

| Scoped command | Result after recovery | Time |
| --- | --- | ---: |
| `./tools/dev inspect-pond-atmosphere` | PASS: required state marker, exit 0 | 13.72 s |
| `./tools/dev inspect-pond-scenery` | PASS: required state marker, exit 0 | 6.39 s |
| `./tools/dev inspect-pond-camera-immersion` | PASS: required state marker, exit 0 | 6.29 s |
| `./tools/dev inspect-ocean-equal-input` | BLOCKED: baked Ocean loads; missing owner Quaternius Fish3.fbx stops player-art loading | 5.29 s |

These are headless C#/state checks, not visual evidence. Initial asset-failure logs and new `logs/recovered-*` logs both remain under the isolated runtime directory; `recovered-check-results.json` stores exact markers. Native Pond rendering was handed to the render agent after these checks; this report makes no visual or target-hardware performance claim.

## Exact Fish3 recovery (before render handoff)

The creator's official [Animated Fish page](https://quaternius.com/packs/animatedfish.html) links to the public [original pack folder](https://drive.google.com/drive/folders/1SvlOveJJjmhSn-FgCRyojc1T5QHjjGkF). Its FBX folder exposes `Fish3.fbx` (87,468 bytes, last modified April 2, 2018). Downloaded bytes match the tracked `docs/tasks/p2/qualification/renderer-decision.md` SHA-256 exactly: `142bb6559b928c61ff839eecbba4ee0cfafa964df37720db6a6ba3a90b4ed639`.

Official file URL: https://drive.google.com/uc?export=download&id=1IhETkaj4bssjH3lNmxvSW7VwW8uCcM0C. The unchanged 361-byte `License.txt` names Animated Fish Pack by Quaternius and CC0 1.0 Universal; its observed SHA-256 is `2832c27ece15255677c5a6d7437ed05d6a2f49ccb73d867ab13d2be3f93e84e2`. The retained [readable notice](quaternius/license-notice.txt) normalizes line endings and trailing spaces; the recorded 361-byte size and SHA-256 identify the unchanged downloaded/runtime notice. The original owner timestamped ZIP itself was not recreated or claimed identical. Only the exact needed FBX and original notice were fetched; records are in [quaternius/fish3-recovery.json](quaternius/fish3-recovery.json).

At that preparation step the files remained in isolated scratch. No additional import or Ocean runtime was started while the rendering agent owned the shared project; final installation is recorded below.

## Final Fish3 installation and Ocean recheck

After the rendering agent explicitly released the shared project, the exact Fish3 FBX and original CC0 notice were copied into ignored `artifacts/assets/source/quaternius-animated-fish/direct-official-2018/` and `game/assets/vendor-local/quaternius/`. No other fish was installed. [quaternius/fish3-installation.json](quaternius/fish3-installation.json) records both paths and unchanged hashes.

- `./tools/dev import`: exit 0, 7.93 s; log `fish3-import-wrapper.log`.
- `./tools/dev inspect-ocean-equal-input`: exit 0, 10.83 s; log `fish3-inspect-ocean-equal-input.log`.

Ocean required marker: `ATMOSPHERE_MOVEMENT_OK equal-physics-route enabled=disabled steps=30`. All four scoped C#/state checks now have successful runs after restoring their specific dependencies. Initial and intermediate missing-asset failures remain separately recorded.

Final Ocean run source identity is `e93ecf5e6bc2d00d6738fce9766d98f17585a4ca+dirty`: the only observed tracked modification was the native reflection probe from rendering work, which this recovery preserved. [fish3-check-source-identity.json](../fish3-check-source-identity.json) records its hash and status. This result is headless simulation equality evidence, not Ocean visual identity or target-device performance.
