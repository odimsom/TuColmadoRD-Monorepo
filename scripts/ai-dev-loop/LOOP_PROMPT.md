# TuColmadoRD — Autonomous AI Dev Loop

You are running an autonomous development loop for TuColmadoRD, an Angular 19 + .NET 10 ERP/POS SaaS for colmados in the Dominican Republic.

**Your job:** Run e2e tests, find failures, fix them, deploy, verify. Repeat until all tests pass.
**Never ask the user anything.** Make decisions independently.

---

## Environment

- Repo: `/home/fcastro_dev/dev_directories/projector_importantes_para_el_futuro/TuColmadoRD-Monorepo`
- Frontend: `frontend/web-admin/` (Angular 19)
- Backend API: `https://api.tucolmadord.com` (deployed on Hostinger)
- Production frontend (web-admin): `https://app.tucolmadord.com`
- Production landing: `https://tucolmadord.com`
- Git remote: `https://github.com/odimsom/TuColmadoRD-Monorepo.git`
- Deploy flow: push to `main` → GitHub Actions `cd-deploy.yml` → self-hosted runner on server deploys
- Test user: `test@tucolmadord.com` / `Test1234!`
- Server SSH: `root@177.7.48.169` (use sshpass or SSH key if available)

---

## Each Iteration

### Step 1 — Run Tests
```bash
cd /home/fcastro_dev/dev_directories/projector_importantes_para_el_futuro/TuColmadoRD-Monorepo
bash scripts/ai-dev-loop/run-tests.sh 2>&1 | tee scripts/ai-dev-loop/last-run.log
```

### Step 2 — Analyze Results
- Read `frontend/web-admin/e2e/results/test-results.json`
- Read screenshots for failed tests: `frontend/web-admin/e2e/results/*.png`
- Read screenshots VISUALLY (they are PNG images) to understand what the UI looked like when it failed
- Decide: is the failure in the **app code** or in the **test spec**?
  - App bug → fix `frontend/web-admin/src/**`
  - Test outdated → fix `frontend/web-admin/e2e/*.spec.ts`
  - Missing feature → implement it in `src/` and add tests in `e2e/`

### Step 3 — Fix
- Edit the relevant files
- If fixing a test selector, verify the HTML structure in the corresponding Angular component first
- Log what you did in `scripts/ai-dev-loop/LOOP_LOG.md`

### Step 4 — Deploy (only if app code changed)
```bash
cd /home/fcastro_dev/dev_directories/projector_importantes_para_el_futuro/TuColmadoRD-Monorepo
git add -A
git commit -m "fix(e2e-loop): <short description of what was fixed>"
git push origin main
```
Then wait for CI/CD to deploy (check every 60s for up to 10 minutes):
```bash
gh run list --branch main --limit 3
gh run watch <run-id>
```

### Step 5 — Verify on Server (if deploy issues suspected)
```bash
# Check server health after deploy
ssh -o StrictHostKeyChecking=no -o ConnectTimeout=10 root@177.7.48.169 "cd /app/tucolmadord && docker compose ps && docker compose logs --tail=20 2>&1 | tail -30"
```
If services are down, restart them:
```bash
ssh -o StrictHostKeyChecking=no root@177.7.48.169 "cd /app/tucolmadord && docker compose up -d"
```

### Step 6 — Repeat
Go back to Step 1.

---

## Stop Condition
Stop the loop when:
- All tests pass for **2 consecutive runs** with no changes needed
- OR you have been running for more than 4 hours
- OR you encounter an error you cannot fix (authentication error on server, CI/CD pipeline broken, etc.)

When stopping, write a final summary in `scripts/ai-dev-loop/LOOP_LOG.md`.

---

## Rules
1. **Never ask the user for input.** Make your best judgment call.
2. When in doubt about a fix, try the less invasive option first.
3. If a test fails because a feature doesn't exist yet in the UI, implement the feature first, then add/update the test.
4. Always look at screenshots to understand visual failures — `Read` the PNG files.
5. Keep commits small and descriptive.
6. If the server is unreachable, skip SSH steps and continue with local fixes.
7. Do not change the test user credentials or the production API URL.
8. Log every fix and result in `LOOP_LOG.md`.
