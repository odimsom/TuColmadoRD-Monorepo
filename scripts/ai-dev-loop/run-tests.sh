#!/usr/bin/env bash
# Runs e2e tests against production and outputs structured summary
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
FRONTEND="$REPO_ROOT/frontend/web-admin"
RESULTS="$FRONTEND/e2e/results"

mkdir -p "$RESULTS"

echo "=== TuColmadoRD AI Dev Loop - Test Run $(date '+%Y-%m-%d %H:%M:%S') ==="
echo ""

cd "$FRONTEND"

# Ensure playwright browsers are installed
npx playwright install chromium --with-deps 2>/dev/null || true

# Run tests against production
echo "Running e2e tests against https://tucolmadord.com..."
EXIT_CODE=0
npx playwright test --config=playwright.prod.config.ts --reporter=list,json 2>&1 || EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
  echo "STATUS: ALL_PASS"
else
  echo "STATUS: FAILURES_DETECTED"
fi

# Print JSON summary if available
if [ -f "$RESULTS/test-results.json" ]; then
  python3 -c "
import json, sys
data = json.load(open('$RESULTS/test-results.json'))
suites = data.get('suites', [])
failed = []
passed = []
for suite in suites:
  for spec in suite.get('specs', []):
    for test in spec.get('tests', []):
      name = suite.get('title','') + ' > ' + spec.get('title','')
      if test.get('status') == 'expected':
        passed.append(name)
      else:
        failed.append(name + ' [' + str(test.get('results',[{}])[-1].get('error',{}).get('message','unknown error'))[:200] + ']')
print(f'PASSED: {len(passed)}')
print(f'FAILED: {len(failed)}')
for f in failed:
  print(f'  FAIL: {f}')
"
fi

exit $EXIT_CODE
