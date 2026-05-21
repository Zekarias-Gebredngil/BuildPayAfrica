FROM node:20-alpine@sha256:fb4cd12c85ee03686f6af5362a0b0d56d50c58a04632e6c0fb8363f609372293 AS build
WORKDIR /app

# Install dependencies first (cached layer — only re-runs when package.json changes)
COPY frontend/package*.json ./
RUN npm ci

# Copy source (cache-busted whenever any source file changes)
COPY frontend/ .

# Remove any stale Angular build cache before building
RUN rm -rf .angular/cache

# Build production bundle
RUN npm run build

# ── Serve with Nginx ──────────────────────────────────────────────────────────
FROM nginx:alpine@sha256:2f07d83bf561b506400dc183b1b2003803e39efbd22451f848adaba14d28c7c7
# Angular 19 outputs to dist/frontend/browser/
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY docker/nginx/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
