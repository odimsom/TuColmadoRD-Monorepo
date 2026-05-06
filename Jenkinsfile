pipeline {
    agent any

    options {
        buildDiscarder(logRotator(numToKeepStr: '20'))
        timeout(time: 40, unit: 'MINUTES')
        disableConcurrentBuilds()
    }

    environment {
        VPS_HOST  = '177.7.48.169'
        APP_DIR   = '/app/tucolmadord'
        GH_REPO   = 'odimsom/TuColmadoRD-Monorepo'
        GH_REMOTE = 'https://github.com/odimsom/TuColmadoRD-Monorepo.git'
    }

    stages {

        // ─── PRUEBAS UNITARIAS — dev y PRs hacia dev ─────────────────────
        stage('Unit Test · .NET Backend') {
            when {
                anyOf {
                    branch 'dev'
                    changeRequest target: 'dev'
                }
            }
            steps {
                sh '''
                    docker run --rm \
                        -v "${WORKSPACE}/backend:/workspace" \
                        -w /workspace \
                        mcr.microsoft.com/dotnet/sdk:10.0-preview \
                        bash -c "dotnet restore && dotnet build -c Release --no-restore && dotnet test -c Release --no-build --verbosity normal"
                '''
            }
        }

        stage('Unit Test · Auth Service') {
            when {
                anyOf {
                    branch 'dev'
                    changeRequest target: 'dev'
                }
            }
            steps {
                sh '''
                    docker run --rm \
                        -v "${WORKSPACE}/auth:/workspace" \
                        -w /workspace \
                        node:22-alpine \
                        sh -c "npm install -g pnpm && pnpm install --frozen-lockfile && pnpm test"
                '''
            }
        }

        // ─── AUTO-PR dev → qa (solo tras merge/commit a dev, nunca en PRs) ─
        stage('Auto PR: dev → qa') {
            when { branch 'dev' }
            steps {
                withCredentials([string(credentialsId: 'github-token', variable: 'GH_TOKEN')]) {
                    sh '''
                        PR_EXISTS=$(curl -sf \
                            "https://api.github.com/repos/${GH_REPO}/pulls?head=odimsom:dev&base=qa&state=open" \
                            -H "Authorization: token ${GH_TOKEN}" \
                            | grep -c '"number"' || true)

                        if [ "${PR_EXISTS:-0}" -gt 0 ]; then
                            echo "ℹ️  Ya existe un PR abierto dev → qa"
                        else
                            curl -sf -X POST \
                                "https://api.github.com/repos/${GH_REPO}/pulls" \
                                -H "Authorization: token ${GH_TOKEN}" \
                                -H "Content-Type: application/json" \
                                -d "{
                                    \\"title\\": \\"[Auto] Promote dev → qa · Build #${BUILD_NUMBER}\\",
                                    \\"head\\": \\"dev\\",
                                    \\"base\\": \\"qa\\",
                                    \\"body\\": \\"PR automático: pruebas unitarias pasaron (build #${BUILD_NUMBER}).\\\\n\\\\nPendiente: pruebas de integración deben pasar en este PR antes del merge.\\"
                                }" > /dev/null
                            echo "✅ PR creado: dev → qa"
                        fi
                    '''
                }
            }
        }

        // ─── PRUEBAS DE INTEGRACIÓN — qa y PRs hacia qa ──────────────────
        stage('Integration Test · .NET Backend') {
            when {
                anyOf {
                    branch 'qa'
                    changeRequest target: 'qa'
                }
            }
            steps {
                sh '''
                    docker run --rm \
                        -v "${WORKSPACE}/backend:/workspace" \
                        -w /workspace \
                        mcr.microsoft.com/dotnet/sdk:10.0-preview \
                        bash -c "dotnet restore && dotnet build -c Release --no-restore && dotnet test -c Release --no-build --verbosity normal"
                '''
            }
        }

        stage('Integration Test · Auth Service') {
            when {
                anyOf {
                    branch 'qa'
                    changeRequest target: 'qa'
                }
            }
            steps {
                sh '''
                    docker run --rm \
                        -v "${WORKSPACE}/auth:/workspace" \
                        -w /workspace \
                        node:22-alpine \
                        sh -c "npm install -g pnpm && pnpm install --frozen-lockfile && pnpm test"
                '''
            }
        }

        // ─── AUTO-PR qa → main (solo tras merge/commit a qa, nunca en PRs) ─
        stage('Auto PR: qa → main') {
            when { branch 'qa' }
            steps {
                withCredentials([string(credentialsId: 'github-token', variable: 'GH_TOKEN')]) {
                    sh '''
                        PR_EXISTS=$(curl -sf \
                            "https://api.github.com/repos/${GH_REPO}/pulls?head=odimsom:qa&base=main&state=open" \
                            -H "Authorization: token ${GH_TOKEN}" \
                            | grep -c '"number"' || true)

                        if [ "${PR_EXISTS:-0}" -gt 0 ]; then
                            echo "ℹ️  Ya existe un PR abierto qa → main"
                        else
                            curl -sf -X POST \
                                "https://api.github.com/repos/${GH_REPO}/pulls" \
                                -H "Authorization: token ${GH_TOKEN}" \
                                -H "Content-Type: application/json" \
                                -d "{
                                    \\"title\\": \\"[Auto] Promote qa → main · Build #${BUILD_NUMBER}\\",
                                    \\"head\\": \\"qa\\",
                                    \\"base\\": \\"main\\",
                                    \\"body\\": \\"PR automático: pruebas de integración pasaron en qa (build #${BUILD_NUMBER}).\\\\n\\\\nMerge para desplegar a producción.\\"
                                }" > /dev/null
                            echo "✅ PR creado: qa → main"
                        fi
                    '''
                }
            }
        }

        // ─── DEPLOY A PRODUCCIÓN — solo en main ───────────────────────────
        stage('Deploy to Production') {
            when { branch 'main' }
            steps {
                withCredentials([sshUserPrivateKey(
                    credentialsId: 'vps-deploy-key',
                    keyFileVariable: 'SSH_KEY'
                )]) {
                    sh '''
                        ssh -i "$SSH_KEY" \
                            -o StrictHostKeyChecking=no \
                            -o ConnectTimeout=30 \
                            root@${VPS_HOST} \
                        "cd ${APP_DIR} && \
                         git fetch origin && \
                         git reset --hard origin/main && \
                         sudo bash deploy-production.sh"
                    '''
                }
            }
        }

        // ─── RELEASE EN GITHUB — solo en main ─────────────────────────────
        stage('Create GitHub Release') {
            when { branch 'main' }
            steps {
                withCredentials([string(credentialsId: 'github-token', variable: 'GH_TOKEN')]) {
                    sh '''
                        VERSION=$(grep -m1 '"version"' frontend/package.json \
                                  | sed 's/.*"version": *"\\([^"]*\\)".*/\\1/')
                        TAG="v${VERSION}"
                        COMMIT="${GIT_COMMIT}"
                        SHORT="${GIT_COMMIT:0:7}"
                        BUILD="${BUILD_NUMBER}"

                        echo "📦 Creando release ${TAG} (build #${BUILD})..."

                        git config user.email "jenkins@tucolmadord.com"
                        git config user.name  "Jenkins CI"

                        REMOTE="https://x-access-token:${GH_TOKEN}@${GH_REMOTE#https://}"
                        git tag -f "$TAG" -m "Release ${TAG}"
                        git push "$REMOTE" "$TAG" --force

                        BODY="### Deploy #${BUILD} — Producción\\n\\n\
**Versión:** \\`${TAG}\\`  \\n\
**Commit:** [\\`${SHORT}\\`](https://github.com/${GH_REPO}/commit/${COMMIT})  \\n\
**Rama:** main  \\n\\n\
> Instalador Windows adjuntado automáticamente por GitHub Actions."

                        CREATE_RESP=$(curl -s -w "\\n%{http_code}" \
                            -X POST \
                            "https://api.github.com/repos/${GH_REPO}/releases" \
                            -H "Authorization: token ${GH_TOKEN}" \
                            -H "Content-Type: application/json" \
                            -d "{
                                \\"tag_name\\": \\"${TAG}\\",
                                \\"name\\": \\"TuColmadoRD ${TAG}\\",
                                \\"body\\": \\"${BODY}\\",
                                \\"draft\\": false,
                                \\"prerelease\\": false
                            }")
                        HTTP_CODE=$(echo "$CREATE_RESP" | tail -1)

                        if [ "$HTTP_CODE" = "422" ]; then
                            echo "ℹ️  Release ${TAG} ya existe — actualizando body..."
                            RELEASE_ID=$(curl -s \
                                "https://api.github.com/repos/${GH_REPO}/releases/tags/${TAG}" \
                                -H "Authorization: token ${GH_TOKEN}" \
                                | grep '"id"' | head -1 | sed 's/[^0-9]//g')
                            curl -s -X PATCH \
                                "https://api.github.com/repos/${GH_REPO}/releases/${RELEASE_ID}" \
                                -H "Authorization: token ${GH_TOKEN}" \
                                -H "Content-Type: application/json" \
                                -d "{
                                    \\"name\\": \\"TuColmadoRD ${TAG}\\",
                                    \\"body\\": \\"${BODY}\\"
                                }" > /dev/null
                        fi

                        echo "✅ Release ${TAG} lista"
                        echo "   https://github.com/${GH_REPO}/releases/tag/${TAG}"
                    '''
                }
            }
        }
    }

    post {
        success {
            echo "✅ Pipeline completado: ${env.BRANCH_NAME}"
        }
        failure {
            echo "❌ Pipeline falló en: ${env.STAGE_NAME} — rama: ${env.BRANCH_NAME}"
        }
        always {
            cleanWs()
        }
    }
}
