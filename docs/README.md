# docs

Project documentation. The [root README](../README.md) covers what MowTrackr is and how to run it locally.

| Document | Read it for |
| --- | --- |
| [azure-setup.md](azure-setup.md) | The Azure environments, the one-time setup, how deploys work (`dev` and `prod`), custom domains and email. |
| [security.md](security.md) | The latest security review: what was checked, what was fixed, known risks, and setting up phone verification. |
| [phase-2-plan.md](phase-2-plan.md) | The draft plan for tablets, multiple customers, offline use and billing (a proposal, not a commitment). |

Related: the [infra README](../infra/README.md) lists the Bicep templates and scripts these documents refer to.

## Branches and deploys

Work goes on a feature, fix or chore branch and merges into `main` by pull request, which deploys the dev site. The
same run then waits for an approval before it deploys prod. The `build` check must pass on every pull request. Details are in [azure-setup.md](azure-setup.md#deploying).

## Adding a document

Put it in this folder with a lowercase, hyphenated name, start it with a single `# Title`, and add a row to the table above.
