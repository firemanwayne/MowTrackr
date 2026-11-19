#!/usr/bin/env bash
# Verification of a custom Azure Communication Services email domain.
#
#   email-domain.sh state  <resource-group> <email-service> <domain>
#       Prints "true" if the domain exists and its Domain, SPF, DKIM and DKIM2 checks have all passed, else "false".
#
#   email-domain.sh verify <resource-group> <email-service> <domain> [timeout-seconds]
#       Asks Azure to check the DNS records (they must already exist), waits until all four pass or the timeout
#       (default 600s) runs out, then prints "true" or "false". Failures are reported on stderr, never as an error exit,
#       so a slow DNS propagation does not fail a deployment: the next run simply tries again.
#
# ACS can only link a custom domain once it is verified, which is why linking is a separate, later step.
set -uo pipefail

command="${1:?usage: email-domain.sh <state|verify> <resource-group> <email-service> <domain> [timeout]}"
resource_group="${2:?missing resource group}"
email_service="${3:?missing email service name}"
domain="${4:?missing domain}"
timeout_seconds="${5:-600}"

subscription="$(az account show --query id --output tsv)"
base="https://management.azure.com/subscriptions/${subscription}/resourceGroups/${resource_group}/providers/Microsoft.Communication/emailServices/${email_service}/domains/${domain}"
api="api-version=2023-04-01"
checks=(Domain SPF DKIM DKIM2)

# Status of one check: NotStarted, VerificationRequested, VerificationInProgress, Verified or VerificationFailed.
status_of() {
  az rest --method GET --url "${base}?${api}" --query "properties.verificationStates.${1}.status" --output tsv 2>/dev/null || echo "Missing"
}

all_verified() {
  local check
  for check in "${checks[@]}"; do
    [ "$(status_of "${check}")" = "Verified" ] || return 1
  done
  return 0
}

case "${command}" in
  state)
    if all_verified; then echo true; else echo false; fi
    ;;

  verify)
    if all_verified; then
      echo true
      exit 0
    fi

    for check in "${checks[@]}"; do
      status="$(status_of "${check}")"
      if [ "${status}" = "NotStarted" ] || [ "${status}" = "VerificationFailed" ]; then
        echo "Requesting ${check} verification for ${domain}" >&2
        az rest --method POST --url "${base}/initiateVerification?${api}" \
          --headers "Content-Type=application/json" \
          --body "{\"verificationType\":\"${check}\"}" --output none 2>/dev/null \
          || echo "Could not start ${check} verification (the record may not exist yet)" >&2
      fi
    done

    deadline=$((SECONDS + timeout_seconds))
    while [ "${SECONDS}" -lt "${deadline}" ]; do
      if all_verified; then
        echo true
        exit 0
      fi
      sleep 15
    done

    for check in "${checks[@]}"; do
      echo "${check}: $(status_of "${check}")" >&2
    done
    echo false
    ;;

  *)
    echo "unknown command: ${command}" >&2
    exit 2
    ;;
esac
