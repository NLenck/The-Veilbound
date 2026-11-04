# Veilborne greatsword Tripo reference pack

## Use

1. Copy this folder into the `C:\The Veilbound` project or another path Codex can read.
2. Send Codex the contents of `CODEX_PROMPT.md` and point it at this folder.
3. Start with `references/all_10_swords_tripo_input.png` **as an experimental one-run input**.
4. Keep `references/original_8_swords_user_reference.png` and `references/tin_greatsword_full.png` as full visual concept references.
5. If the combined generation fails, `references/individual_fallback/` has mechanical crops of the ten swords; the crops are lower-resolution and should be checked and possibly regenerated before separate Tripo runs.

## Important limitations

- Reference images are 2D artwork, **not** already-generated 3D meshes.
- The composite is tightly packed. Tripo may fuse the objects or generate them as a flat plane. One composite generation is a hypothesis to test, not a known supported bulk-discount feature.
- The composite images were created as concept artwork, then **mechanically cropped** to remove headings and detail panels for better Image-to-3D input.
- The source artwork includes some visual refinements beyond the original eight-sword sheet, notably Tin and Tungsten.
- Preview sources are arranged horizontally and from the front only. For higher 3D quality, generating individual orthographic multiview references may become necessary.
