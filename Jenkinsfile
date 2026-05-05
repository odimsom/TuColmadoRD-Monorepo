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

        // ─── CI: Tests ────────────────────────────────────────────────────
        stage('Test .NET Backend') {
            steps {
                sh '''
                    docker run --rm \
                        -v "${WORKSPACE}/backend:/workspace" \
                        -w /workspace \
                        mcr.microsoft.com/dotnet/sdk:10.0-preview \
                        bash -c "
                            dotnet restore TuColmadoRD.slnx &&
                            dotnet build TuColmadoRD.slnx -c Release --no-restore &&
                            dotnet test TuColmadoRD.slnx -c Release --no-build --verbosity normal
                        "
                '''
            }
        }

        stage('Test Auth Service') {
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

        // ─── Promoción dev → qa ───────────────────────────────────────────
        stage('Promote dev → qa') {
            when { branch 'dev' }
            steps {
                withCredentials([string(credentialsId: 'github-token', variable: 'GH_TOKEN')]) {
                    sh '''
                        git config user.email "jenkins@tucolmadord.com"
                        git config user.name  "Jenkins CI"
                        git push "https://x-access-token:${GH_TOKEN}@${GH_REMOTE#https://}" \
                            HEAD:refs/heads/qa --force
                        echo "✅ Promovido dev → qa"
                    '''
                }
            }
        }

        // ─── Promoción qa → main ──────────────────────────────────────────
        stage('Promote qa → main') {
            when { branch 'qa' }
            steps {
                withCredentials([string(credentialsId: 'github-token', variable: 'GH_TOKEN')]) {
                    sh '''
                        git config user.email "jenkins@tucolmadord.com"
                        git config user.name  "Jenkins CI"
                        git push "https://x-access-token:${GH_TOKEN}@${GH_REMOTE#https://}" \
                            HEAD:refs/heads/main --force
                        echo "✅ Promovido qa → main"
                    '''
                }
            }
        }

        // ─── CD: Deploy a producción ──────────────────────────────────────
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

        // ─── Release en GitHub ────────────────────────────────────────────
        // Crea el tag vX.Y.Z y la release en GitHub.
        // GitHub Actions (cd-deploy.yml) luego adjunta el instalador .exe
        // a esta misma release usando allow_updates: true.
        stage('Create GitHub Release') {
            when { branch 'main' }
            steps {
                withCredentials([string(credentialsId: 'github-token', variable: 'GH_TOKEN')]) {
                    sh '''
                        # Leer versión desde package.json (sin dependencias externas)
                        VERSION=$(grep -m1 '"version"' frontend/package.json \
                                  | sed 's/.*"version": *"\\([^"]*\\)".*/\\1/')
                        TAG="v${VERSION}"
                        COMMIT="${GIT_COMMIT}"
                        SHORT="${GIT_COMMIT:0:7}"
                        BUILD="${BUILD_NUMBER}"

                        echo "📦 Creando release ${TAG} (build #${BUILD})..."

                        # Crear o reusar tag en GitHub
                        git config user.email "jenkins@tucolmadord.com"
                        git config user.name  "Jenkins CI"

                        REMOTE="https://x-access-token:${GH_TOKEN}@${GH_REMOTE#https://}"
                        git tag -f "$TAG" -m "Release ${TAG}"
                        git push "$REMOTE" "$TAG" --force

                        # Crear release via API (si ya existe, la actualiza)
                        BODY="### Deploy #${BUILD} — Producción\\n\\n\
**Versión:** \\`${TAG}\\`  \\n\
**Commit:** [\\`${SHORT}\\`](https://github.com/${GH_REPO}/commit/${COMMIT})  \\n\
**Rama:** main  \\n\\n\
> Instalador Windows adjuntado automáticamente por GitHub Actions."

                        # Intentar crear; si falla (ya existe), buscar y actualizar
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

                        echo "✅ Release ${TAG} lista en GitHub"
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
