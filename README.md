Codexにて作成

# Spherical Harmonics Viewer

An educational Unity 2022.3 viewer for normalized real and complex spherical harmonics with `0 ≤ l ≤ 3`.

## Mathematical conventions

- Complex harmonics are normalized and include the Condon–Shortley phase.
- For every `l`, real `m > 0` is cosine type, real `m < 0` is sine type, and `m = 0` is zonal.
- `R[l,+k] = (Y[l,-k] + (-1)^k Y[l,+k]) / sqrt(2)` and `R[l,-k] = i(Y[l,-k] - (-1)^k Y[l,+k]) / sqrt(2)`.
- Real and complex coefficient banks are independent and retain all sixteen coefficients while the UI edits one `(l,m)` at a time.
- Flow uses `V_l = grad(r^l F_l)/l` for `l >= 1`, `V_0 = F_0 x`, and recomputes every selected time from the reference sphere with RK4.

`BasisDefinitionTable` embeds the supplied JSON values in an immutable table. This is deliberate: it preserves the source data exactly while avoiding runtime filesystem/JSON-loading differences in WebGL.

## Runtime controls

- Sphere, Orbital, and Flow displays; selecting Flow from Complex automatically switches to the Real bank.
- Real coefficient or Complex magnitude/phase editing, Pure Mode, and Clear All.
- Display coordinates use the explicitly requested mapping `(x,y,z) → (x,z,y)`: mathematical `x` is Unity `+x`, mathematical `y` is Unity `+z`, and mathematical `z` is Unity `+y`. Axes extend in both directions with positive arrowheads, and the UI reports components in display order `(x,z,y)`.
- Function surface, colored function wireframe, unit-sphere surface, and unit-sphere wireframe are independently visible. Blue/red half-axis-width vectors show positive/negative values: Sphere connects the unit sphere to the surface, Orbital starts at the origin, and Flow shows instantaneous normal velocity. Orbital geometry and its final reference sphere use the same 1.5x presentation scale.
- Function/Coordinates rotation with immediate preview and Apply/Cancel. Apply bakes the rotation into coefficients; Reset Axes re-expresses coefficients while preserving the world-space function.
- Fourier Bridge stages Sphere, Circle, and Line. Its parameter controls use the normal viewer state semantics. Entering Bridge resets coordinate axes and disables Rotate.
- Desktop: drag to orbit, wheel to zoom. Mobile: one-finger orbit, two-finger pinch.

## Architecture

- `Math`: basis metadata, complex arithmetic, and evaluators.
- `State`: independent coefficient banks and viewer state.
- `Rendering`: reference topology, surface displacement/color, wire/reference/axes overlays.
- `Rotation`: active rotation and passive coordinate re-expression, isolated per `l`.
- `Flow`: solid harmonics, velocity construction, and reference-based RK4 integration.
- `Fourier`: bridge state and real/complex curve generation.
- `UI` and `Input`: responsive controls and camera gestures.

Rotation uses deterministic spherical projection to construct the `l ≤ 3` coefficient transform. This is the simplest convention-safe equivalent of Wigner-D for the specified range and is tested by sample-point invariance rather than Euler-sign assumptions.

## Tests

EditMode tests cover all 16 table entries, representative normalized values and axis signs, the cosine/sine rule, the Condon–Shortley negative-`m` relation, active/passive rotations without `l` mixing, Flow initial normal velocity, pure-`l=1` translation, and Complex Fourier helix handedness.

From the repository root:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\2022.3.22f1\Editor\Unity.exe' -batchmode -nographics -projectPath $PWD -runTests -testPlatform editmode -testResults TestResults.xml -logFile Logs/editmode-tests.log
```

## WebGL and GitHub Pages

The project uses Gzip with Decompression Fallback and a responsive WebGL template containing only relative asset paths, viewport/safe-area handling, and `touch-action: none` on the canvas. Engine-code stripping is disabled because the scene composes its MonoBehaviour graph dynamically at runtime; this avoids stripped-class warnings in the browser build.

The workflow in `.github/workflows/deploy-webgl-github-pages.yml` runs EditMode tests and invokes the same project build method used locally, keeping `index.html` directly under `build/WebGL`. It verifies that publish root before uploading it. A push to `main` additionally deploys `build/WebGL` to GitHub Pages. Configure repository **Settings > Pages > Source** as **GitHub Actions** and add the GameCI activation secrets appropriate for the repository's Unity license (`UNITY_LICENSE`, plus `UNITY_EMAIL` and `UNITY_PASSWORD` when required). No license material belongs in the repository.

## Deliberately simple unspecified behavior

- The initial mode is real `R(1,0)` in Sphere display.
- Flow time is limited to `[-0.65, 0.65]` for the MVP; RK4 uses 32 steps.
- Mesh quality, vector density, color epsilon, negative-radius protection, and display scale remain serialized component settings rather than runtime controls.
- Formula cards use larger italic Unicode text rendered by the bundled Noto Sans Math font, so Greek letters, proportional/root symbols, and superscript/subscript characters do not depend on browser or operating-system font fallback. Noto Sans Math is distributed under the SIL Open Font License 1.1; its license is included beside the font in `Assets/Resources/Fonts`. Pre-rendered LaTeX images are feasible, but would require a maintained image atlas for every Real/Complex and normalization variant; that optional asset pipeline is not included in the MVP.
