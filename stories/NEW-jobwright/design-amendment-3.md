<!-- aidlc template: design-note -->
# NEW-jobwright · design note · amendment 3

## For the reviewer
- **Asked of:** Prashant Sharma, as tech lead and as security owner (R-k), to approve ADR 0015 and the changes below
- **Record it:** On the APPROVED line below, and on this note's commit
- **Tier and main risk:** 3: anyone who can write `main` of `jobwright-gitops` reaching cluster-admin through Argo CD (R-k, open until part 4's AppProject)
- **Changed since the last version:** first version (2026-11-13)

Inputs: [spec v4](spec.md) (approved 2026-11-11), the approved design, design amendments 1 and 2, ADRs 0001 to 0014,
and `jobwright-gitops`'s part 2 plan: its table "Root changes this plan needs" and their decisions in
`plans/NEW-jobwright/part-2/progress-slice-0-bootstrap.md`, "Root changes decided" (Prashant Sharma, 2026-11-13: "i
accept your recommendations for all root changes"), both on branch `feat/NEW-jobwright-slice-0` at 628471f. C4 needed
no change (part 2 is one pull request). No criterion changes, so the spec stays v4. Everything else in the approved
design stands.

| Item | Change |
|---|---|
| C1 · U1, U2, U9 | U1 (client address) is answered in part 4's Traefik slice and U2 (TraefikService routing and stickiness) in its Rollouts slice, each with its script: the pieces they measure arrive in part 4 (part 2's K1, S1). U9 stays in part 2 and is answered: 1,746,025,841 bytes idle (k3s, Argo CD, Sealed Secrets; measured by the owner 2026-11-13), under the 6 GB limit (E21), re-measured after every platform change. This amends where ADR 0008 ("Slice 0 of `jobwright-gitops` measures"), ADR 0012 ("U2 still proved in slice 0") and the design note's Unknowns place U1, U2 and U9; none of them is edited |
| C2 · bootstrap/ | Part 2's `bootstrap/` holds the k3d cluster, Sealed Secrets and its key export, Argo CD's repository credential (sealed) and Argo CD. The local certificate authority (ADR 0008) and the image pull secrets (ADR 0013) are sealed by part 4's slices that first need them, so their namespaces and names are fixed with what uses them (part 2's S2) |
| C3 · ADR 0015 (new, [adr-draft-15.md](adr-draft-15.md)) | 127.0.0.1:6550 (the Kubernetes API) is a Jobwright port beside 8443 and 8080, bound to 127.0.0.1 only; the guard's identity rule (part 2's K2) defines the local cluster. Amends one line each of ADR 0008 and ADR 0013 (not edited: ADRs are accepted). Rules conflict X10 settled; `jobwright-gitops`'s AGENTS.md ports line follows, by Prashant Sharma |
| C5 · Threat model | New row below. Decided: mitigate, with an Argo CD AppProject designed in part 4, before part 3's release job holds STAGING_TOKEN. Risk R-k stays open until then |
| C6 · kubectl | kubectl 1.37.x, within k3s 1.37.2's supported skew (1.36 to 1.38), installed by the owner under `~/.local/opt/kubectl-1.37.x`; `/usr/local/bin/kubectl` (the lab's, v1.36.0) unchanged |
| `new-system.md` v8 | `jobwright-gitops`: DEPENDENCIES gains kubectl (machine); a PORTS line (C3) and the identity rule (ADR 0015); Pinned versions gains kubectl |
| Coordination plan v4 | Part 2's note (U9 only; U7 gone with Release ledger v2), OUTSIDE (bootstrap's contents, kubectl, the measurements), ORDER (the AppProject before part 3's release job), part 3's Waits for, UNKNOWNS (U1, U2 to part 4; U9 answered) |
| Contracts | None change: Jobwright API v1, Released versions v2 and Release ledger v2 stand |
| Dependencies | kubectl 1.37.x on the machine (C6) |

**Threat model, new row** (the design note's table, STRIDE):

| Boundary | Threats | Mitigations (where decided) |
|---|---|---|
| `jobwright-gitops` `main` -> Argo CD (cluster-admin) | T, E: whoever can write `main` (STAGING_TOKEN can write the whole repository, KL4; nothing protects `main`, KL1) makes Argo CD apply anything with cluster-admin, on a node container Docker Desktop runs privileged (INFERENCE), on the machine that holds the live kubeconfig (part 2's X7) | Decided 2026-11-13 (C5): an Argo CD AppProject limiting what `platform/` and `environments/` may deploy, designed in part 4 before part 3's release job holds STAGING_TOKEN. Until then slice 0 adds no writer to `main`; the first writer is part 3's release job. Risk R-k: open, owner Prashant Sharma (security owner) |

Unknowns: U1 and U2 move to part 4 (above). The AppProject's limits (which kinds and namespaces each of `platform/`
and `environments/` may deploy) are part 4's to design; no new unknown blocks part 2.

APPROVED     Prashant Sharma (tech lead; security owner for R-k), 2026-11-13 ("i approve design amendment"), recorded by
             Claude at his instruction: ADR 0015, `new-system.md` v8, coordination plan v4 and the threat-model row above
